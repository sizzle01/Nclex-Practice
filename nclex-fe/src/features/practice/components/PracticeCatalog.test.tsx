import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { PracticeCatalog } from "./PracticeCatalog";
import { challengeDetail, challengeSet, practiceSet, sampleSet } from "./testSets";

// AuthProvider installs its middleware on the client when imported, so the mock needs `use`.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

const replace = vi.fn();
let searchParams = new URLSearchParams();
vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace, push: vi.fn() }),
  usePathname: () => "/practice",
  useSearchParams: () => searchParams,
}));

function reply(body: unknown) {
  return { data: body, error: undefined, response: new Response(null, { status: 200 }) } as never;
}

/** Answers GET /test-sets with the given pages (by cursor) and GET /test-sets/{id} with the challenge set. */
function serve(pages: Record<string, { items: unknown[]; nextCursor: string | null }>) {
  getMock.mockImplementation(((path: string, init: { params: { query?: { cursor?: string } } }) => {
    if (path === "/api/v1/test-sets/{id}") return Promise.resolve(reply(challengeDetail));
    return Promise.resolve(reply(pages[init.params.query?.cursor ?? "first"]));
  }) as never);
}

describe("PracticeCatalog", () => {
  beforeEach(() => {
    getMock.mockReset();
    replace.mockReset();
    searchParams = new URLSearchParams();
  });

  it("lists the seeded sets as cards, in the API's order", async () => {
    serve({ first: { items: [sampleSet, practiceSet, challengeSet], nextCursor: null } });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    const list = await screen.findByRole("list", { name: "Practice sets" });
    expect(within(list).getAllByRole("heading").map((h) => h.textContent)).toEqual([
      "RN Sample Set",
      "Comprehensive Practice 1",
      "Comprehensive Challenge 1",
    ]);
    expect(getMock).toHaveBeenCalledWith("/api/v1/test-sets", {
      params: { query: { kind: "prebuilt", limit: 20, cursor: undefined } },
    });
  });

  it("puts the chosen set in the URL so its details can be linked to", async () => {
    serve({ first: { items: [sampleSet, challengeSet], nextCursor: null } });
    const user = userEvent.setup();
    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    const card = (await screen.findByRole("heading", { name: "Comprehensive Challenge 1" })).closest("article")!;
    await user.click(within(card).getByRole("button", { name: "View details" }));

    expect(replace).toHaveBeenCalledWith(`/practice?set=${challengeSet.id}`, { scroll: false });
  });

  it("shows the details of the set in the URL", async () => {
    serve({ first: { items: [sampleSet, challengeSet], nextCursor: null } });
    searchParams = new URLSearchParams({ set: challengeSet.id });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    const panel = await screen.findByRole("region", { name: "Comprehensive Challenge 1" });
    expect(within(panel).getByRole("button", { name: "Start" })).toBeEnabled();
  });

  it("loads more sets when there is another page", async () => {
    serve({
      first: { items: [sampleSet], nextCursor: "page-2" },
      "page-2": { items: [practiceSet], nextCursor: null },
    });
    const user = userEvent.setup();
    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    await user.click(await screen.findByRole("button", { name: "Show more sets" }));

    expect(await screen.findByRole("heading", { name: "Comprehensive Practice 1" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Show more sets" })).not.toBeInTheDocument();
  });

  it("says so when the track has no sets yet", async () => {
    serve({ first: { items: [], nextCursor: null } });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    expect(await screen.findByText(/no practice sets for your track yet/)).toBeInTheDocument();
  });
});
