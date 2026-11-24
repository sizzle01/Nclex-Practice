import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { PracticeCatalog } from "./PracticeCatalog";
import {
  caseStudySet,
  challengeDetail,
  challengeSet,
  practiceSet,
  sampleSet,
  topicAbgSet,
  topicPreeclampsiaSet,
  untimedChallengeSet,
} from "./testSets";

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
    localStorage.clear();
  });

  it("groups the sets into sections, in the API's order within each", async () => {
    serve({ first: { items: [challengeSet, untimedChallengeSet, caseStudySet, sampleSet, practiceSet], nextCursor: null } });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    await screen.findByRole("tablist", { name: "Filter practice sets" });
    expect(screen.getAllByRole("heading", { level: 2 }).map((h) => h.textContent)).toEqual([
      "Start here (1)",
      "Practice sets (1)",
      "Case studies (1)",
      "Mock exams: timed (1)",
      "Mock exams: untimed (1)",
    ]);
    const exams = screen.getByRole("list", { name: "Mock exams: timed" });
    expect(within(exams).getAllByRole("heading").map((h) => h.textContent)).toEqual(["Comprehensive Challenge 1"]);
    expect(getMock).toHaveBeenCalledWith("/api/v1/test-sets", {
      // No kind: prebuilt sets and adaptive exams come together (F19).
      params: { query: { limit: 50, cursor: undefined } },
    });
  });

  it("shows each tab's set count and selects All by default", async () => {
    serve({ first: { items: [sampleSet, practiceSet, challengeSet], nextCursor: null } });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    const tabs = await screen.findAllByRole("tab");
    expect(tabs.map((tab) => tab.textContent)).toEqual(["All 3", "Practice 2", "Timed 1", "Untimed 2", "Adaptive 0", "Topics 0"]);
    expect(screen.getByRole("tab", { name: "All 3" })).toHaveAttribute("aria-selected", "true");
    expect(screen.getByRole("tabpanel")).toHaveAccessibleName("All 3");
  });

  it("shows only the sets of the tab in the URL", async () => {
    serve({ first: { items: [sampleSet, practiceSet, challengeSet], nextCursor: null } });
    searchParams = new URLSearchParams({ filter: "timed" });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: "Comprehensive Challenge 1" })).toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: "RN Sample Set" })).not.toBeInTheDocument();
    expect(screen.getByRole("tab", { name: "Timed 1" })).toHaveAttribute("aria-selected", "true");
  });

  it("puts the chosen tab in the URL and keeps the open set", async () => {
    serve({ first: { items: [sampleSet, challengeSet], nextCursor: null } });
    searchParams = new URLSearchParams({ set: challengeSet.id });
    const user = userEvent.setup();
    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    await user.click(await screen.findByRole("tab", { name: "Timed 1" }));

    expect(replace).toHaveBeenCalledWith(`/practice?filter=timed&set=${challengeSet.id}`, { scroll: false });
  });

  it("moves between tabs with the arrow keys, Home and End", async () => {
    serve({ first: { items: [sampleSet, challengeSet], nextCursor: null } });
    const user = userEvent.setup();
    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    const all = await screen.findByRole("tab", { name: "All 2" });
    expect(screen.getByRole("tab", { name: "Practice 1" })).toHaveAttribute("tabindex", "-1");
    all.focus();
    await user.keyboard("{ArrowRight}");
    expect(replace).toHaveBeenLastCalledWith("/practice?filter=practice", { scroll: false });
    expect(screen.getByRole("tab", { name: "Practice 1" })).toHaveFocus();
    // The mocked router never changes the URL, so All stays selected and each key moves from it.
    await user.keyboard("{ArrowLeft}");
    expect(replace).toHaveBeenLastCalledWith("/practice?filter=topics", { scroll: false });
    await user.keyboard("{End}");
    expect(replace).toHaveBeenLastCalledWith("/practice?filter=topics", { scroll: false });
    await user.keyboard("{Home}");
    expect(replace).toHaveBeenLastCalledWith("/practice", { scroll: false });
  });

  it("lists topic tests under their review-book sections in the Topics tab", async () => {
    serve({ first: { items: [sampleSet, topicAbgSet, topicPreeclampsiaSet], nextCursor: null } });
    searchParams = new URLSearchParams({ filter: "topics" });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: /^Fundamental/ })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: /^Maternity/ })).toBeInTheDocument();
    // Topics starts collapsed (F28): the cards appear once a section is opened.
    expect(screen.queryByRole("heading", { name: "ABG" })).not.toBeInTheDocument();
    await userEvent.setup().click(screen.getByRole("button", { name: /^Fundamental/ }));
    expect(screen.getByRole("heading", { name: "ABG" })).toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: "RN Sample Set" })).not.toBeInTheDocument();
    expect(screen.getByRole("tab", { name: "Topics 2" })).toHaveAttribute("aria-selected", "true");
  });

  it("says adaptive exams are coming when none are published", async () => {
    serve({ first: { items: [sampleSet, challengeSet], nextCursor: null } });
    searchParams = new URLSearchParams({ filter: "adaptive" });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    expect(await screen.findByText(/Adaptive exams are coming soon/)).toBeInTheDocument();
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

  it("loads every page before grouping, so no section is missing sets", async () => {
    serve({
      first: { items: [sampleSet], nextCursor: "page-2" },
      "page-2": { items: [practiceSet], nextCursor: null },
    });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: "Comprehensive Practice 1" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "RN Sample Set" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Show more/ })).not.toBeInTheDocument();
  });

  it("says so when the track has no sets yet", async () => {
    serve({ first: { items: [], nextCursor: null } });

    renderWithAuth(<PracticeCatalog />, { status: "authenticated" });

    expect(await screen.findByText(/no practice sets for your track yet/)).toBeInTheDocument();
  });
});
