import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { challengeDetail } from "@/features/practice/components/testSets";
import { testUser } from "@/features/settings/components/testUser";
import { apiClient } from "@/lib/api/client";
import { CustomBuilder } from "./CustomBuilder";
import { testTaxonomy } from "./testTaxonomy";

// AuthProvider installs its middleware on the client when imported, so the mock needs `use`.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);
const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

type CountQuery = { clientNeedsIds: number[]; itemTypes: string[]; pool: string };

/** Answers /me, /taxonomy and the live count; `available` decides the count. */
function serve(available = 25) {
  getMock.mockImplementation(((path: string) => {
    if (path === "/api/v1/me") return Promise.resolve(reply(200, testUser));
    if (path === "/api/v1/taxonomy") return Promise.resolve(reply(200, testTaxonomy));
    return Promise.resolve(reply(200, { available }));
  }) as never);
}

function countQueries(): CountQuery[] {
  // The client's overloads make the recorded calls untyped; each is (path, init).
  return (getMock.mock.calls as unknown as [string, unknown][])
    .filter(([path]) => path === "/api/v1/test-sets/custom/available-count")
    .map(([, init]) => (init as { params: { query: CountQuery } }).params.query);
}

async function renderBuilder() {
  renderWithAuth(<CustomBuilder />, { status: "authenticated" });
  await screen.findByRole("checkbox", { name: "Management of Care" });
}

describe("CustomBuilder", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
    push.mockReset();
  });

  it("shows the live count for the user's track", async () => {
    serve(25);

    await renderBuilder();

    await waitFor(() => expect(screen.getByRole("status")).toHaveTextContent("25 questions match your filters."));
    expect(countQueries()[0]).toMatchObject({ track: "RN", clientNeedsIds: [], pool: "all" });
  });

  it("asks for the count once the filters stop changing, not on every click", async () => {
    serve();
    const user = userEvent.setup();
    await renderBuilder();
    await waitFor(() => expect(countQueries()).toHaveLength(1));

    await user.click(screen.getByRole("checkbox", { name: "Management of Care" }));
    await user.click(screen.getByRole("checkbox", { name: "Safety and Infection Control" }));
    await user.click(screen.getByRole("checkbox", { name: "Select All That Apply" }));

    await waitFor(() => expect(countQueries()).toHaveLength(2));
    expect(countQueries()[1]).toMatchObject({ clientNeedsIds: [2, 3], itemTypes: ["sata"] });
  });

  it("only offers item types that can be served", async () => {
    serve();

    await renderBuilder();

    expect(screen.getByRole("checkbox", { name: "Select All That Apply" })).toBeInTheDocument();
    expect(screen.queryByRole("checkbox", { name: "Bowtie" })).not.toBeInTheDocument();
  });

  it("warns when more questions are asked for than match", async () => {
    serve(3);

    await renderBuilder();

    await waitFor(() =>
      expect(screen.getByRole("status")).toHaveTextContent("3 questions match your filters. Choose fewer questions"),
    );
  });

  it("creates the test and shows it with the number of questions", async () => {
    serve();
    postMock.mockResolvedValue(
      reply(201, {
        testSet: { ...challengeDetail, kind: "custom", title: "Custom test: 10 questions", questionCount: 10 },
        attempt: null,
      }),
    );
    const user = userEvent.setup();
    await renderBuilder();

    await user.click(screen.getByRole("checkbox", { name: "Psychosocial Integrity" }));
    const count = screen.getByLabelText(/Number of questions/);
    await user.clear(count);
    await user.type(count, "10");
    await user.click(screen.getByRole("radio", { name: /Challenge/ }));
    await user.click(screen.getByRole("checkbox", { name: "Timed" }));
    await user.type(screen.getByLabelText(/Time limit for the test/), "15");
    await user.click(screen.getByRole("button", { name: "Create test" }));

    const heading = await screen.findByRole("heading", { name: "Your test is ready" });
    expect(heading).toHaveFocus();
    expect(screen.getByText("Custom test: 10 questions")).toBeInTheDocument();
    expect(screen.getByText("10 questions")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Start" })).toBeEnabled();
    expect(postMock).toHaveBeenCalledWith("/api/v1/test-sets/custom", {
      body: expect.objectContaining({
        track: "RN",
        clientNeedsIds: [5],
        questionCount: 10,
        mode: "challenge",
        revealMode: "tutor",
        timing: { timed: true, timeBudgetSec: 900, perQuestionSec: null },
        startNow: false,
      }),
    });
  });

  it("starts the attempt with the set and opens the player", async () => {
    serve();
    postMock.mockResolvedValue(
      reply(201, { testSet: { ...challengeDetail, kind: "custom" }, attempt: { id: "attempt-9" } }),
    );
    const user = userEvent.setup();
    await renderBuilder();

    await user.click(screen.getByRole("button", { name: "Create and start" }));

    await waitFor(() => expect(push).toHaveBeenCalledWith("/attempts/attempt-9"));
    expect(postMock).toHaveBeenCalledWith("/api/v1/test-sets/custom", { body: expect.objectContaining({ startNow: true }) });
    expect(screen.queryByRole("heading", { name: "Your test is ready" })).not.toBeInTheDocument();
  });

  it("starts a test created earlier from the card's Start button", async () => {
    serve();
    postMock.mockImplementation(((path: string) =>
      Promise.resolve(
        path === "/api/v1/attempts"
          ? reply(201, { id: "attempt-3" })
          : reply(201, { testSet: { ...challengeDetail, id: "set-7", kind: "custom" }, attempt: null }),
      )) as never);
    const user = userEvent.setup();
    await renderBuilder();
    await user.click(screen.getByRole("button", { name: "Create test" }));
    await screen.findByRole("heading", { name: "Your test is ready" });

    await user.click(screen.getByRole("button", { name: "Start" }));

    await waitFor(() => expect(push).toHaveBeenCalledWith("/attempts/attempt-3"));
    expect(postMock).toHaveBeenCalledWith("/api/v1/attempts", { body: { testSetId: "set-7" } });
  });

  it("asks an unverified user to verify their email before starting", async () => {
    serve();
    postMock.mockResolvedValue(reply(403, { code: "EMAIL_NOT_VERIFIED", title: "Verify your email." }));
    const user = userEvent.setup();
    await renderBuilder();

    await user.click(screen.getByRole("button", { name: "Create and start" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Verify your email address to start a test");
    expect(push).not.toHaveBeenCalled();
  });

  it("explains a timed test without a timer before sending anything", async () => {
    serve();
    const user = userEvent.setup();
    await renderBuilder();

    await user.click(screen.getByRole("checkbox", { name: "Timed" }));
    await user.click(screen.getByRole("button", { name: "Create test" }));

    expect(await screen.findByText(/Set a time limit for the test, a time per question, or both/)).toBeInTheDocument();
    expect(postMock).not.toHaveBeenCalled();
  });

  it("says so when the pool shrank before the test was created", async () => {
    serve();
    postMock.mockResolvedValue(
      reply(422, { code: "INSUFFICIENT_QUESTIONS", title: "Not enough questions match these filters.", available: 4 }),
    );
    const user = userEvent.setup();
    await renderBuilder();

    await user.click(screen.getByRole("button", { name: "Create test" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Not enough questions match any more");
  });
});
