import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { AttemptState, CandidateItem, ItemReveal } from "../api/attemptApi";
import { AttemptPlayer } from "./AttemptPlayer";

// Submitting in the player (F10): tutor-mode item submits and the final submit. Only the network
// and the router are replaced.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn(), PUT: vi.fn(), PATCH: vi.fn() } }));
const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);
const putMock = vi.mocked(apiClient.PUT);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

const attemptId = "attempt-1";

function attemptState(revealMode: "tutor" | "exam"): AttemptState {
  return {
    id: attemptId,
    testSet: { id: "set-1", title: "Comprehensive Practice 1" },
    status: "in_progress",
    settings: { mode: "practice", revealMode, allowBackNav: true, perQuestionSec: null, timeBudgetSec: null, passThreshold: null },
    totalItems: 3,
    currentPosition: 1,
    progress: { answered: 1, locked: 0, flagged: 1 },
    navigator: [
      { position: 1, state: "answered", flagged: false },
      { position: 2, state: "seen", flagged: true },
      { position: 3, state: "unseen", flagged: false },
    ],
    timer: { serverNow: "2026-10-28T09:00:00Z", exam: null, item: null, pause: { used: 0, max: null, maxPauseSec: null, startedAt: null } },
    startedAt: "2026-10-28T08:00:00Z",
    lastActivityAt: "2026-10-28T09:00:00Z",
    staleDraft: false,
    resultAvailable: false,
  };
}

const sataItem: CandidateItem = {
  position: 1,
  questionVersionId: "v-1",
  itemType: "sata",
  stem: "Which findings should the nurse report?",
  content: { options: ["a", "b", "c", "d", "e"].map((id) => ({ id, text: `Finding ${id}` })) },
  exhibits: [],
  caseStudy: null,
  state: "answered",
  response: { selected: ["a", "b", "d"] },
  clientSeq: 3,
  flagged: false,
  struckOptions: [],
  timeAllotmentSec: 120,
  reveal: null,
};

const reveal: ItemReveal = {
  answerKey: { correct: ["a", "b", "e"] },
  grade: {
    points: 1,
    maxPoints: 3,
    isCorrect: false,
    detail: [
      { target: "a", delta: 1, missed: false },
      { target: "b", delta: 1, missed: false },
      { target: "c", delta: 0, missed: false },
      { target: "d", delta: -1, missed: false },
      { target: "e", delta: 0, missed: true },
    ],
  },
  rationale: {
    correct: "Report a, b and e.",
    perOption: { a: "Why a", b: "Why b", c: "Why c", d: "Why d", e: "Why e" },
  },
  educationalObjective: "Recognise signs of deterioration.",
  tags: { clientNeeds: "SECE.SIC", bodySystem: "respiratory", cjmmSkill: "recognize_cues", difficulty: "moderate" },
};

const submitResult = {
  attemptId,
  status: "submitted",
  submitReason: "user",
  result: {
    score: { pointsEarned: 2, maxPoints: 5, pct: 0.4, itemsCorrect: 1, itemsAnswered: 2, itemsTotal: 3 },
    time: { totalMs: 60000, avgPerItemMs: 20000 },
    readiness: null,
  },
};

/** Answers POSTs by path; GETs return the item or the attempt state. */
function serve(revealMode: "tutor" | "exam", posts: Record<string, () => unknown> = {}) {
  const state = attemptState(revealMode);
  postMock.mockImplementation(((path: string) =>
    Promise.resolve(path === "/api/v1/attempts/{id}/open" ? reply(200, state) : posts[path]())) as never);
  getMock.mockImplementation(((path: string) =>
    Promise.resolve(reply(200, path === "/api/v1/attempts/{id}/items/{position}" ? sataItem : state))) as never);
}

function renderPlayer() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <AttemptPlayer attemptId={attemptId} />
    </QueryClientProvider>,
  );
}

// The client's overloaded methods make vitest type the recorded calls as never.
function postsTo(path: string) {
  return (postMock.mock.calls as unknown as [string, { body?: unknown }][]).filter(([p]) => p === path);
}

describe("AttemptPlayer submitting", () => {
  beforeEach(() => {
    localStorage.clear();
    getMock.mockReset();
    postMock.mockReset();
    putMock.mockReset();
    push.mockReset();
  });

  it("asks for confirmation with the unanswered and flagged counts before submitting", async () => {
    serve("exam", { "/api/v1/attempts/{id}/submit": () => reply(200, submitResult) });
    renderPlayer();
    await screen.findByRole("heading", { name: "Which findings should the nurse report?" });

    await userEvent.click(screen.getByRole("button", { name: "Submit test" }));

    const dialog = screen.getByRole("dialog", { name: "Submit this test?" });
    expect(dialog).toHaveTextContent("2 unanswered questions and 1 question marked for review.");
    // Focus starts on the safe choice, so Enter does not end the test by accident.
    expect(within(dialog).getByRole("button", { name: "Keep working" })).toHaveFocus();
    expect(postsTo("/api/v1/attempts/{id}/submit")).toHaveLength(0);
  });

  it("closes the confirmation with Keep working or Escape without submitting", async () => {
    serve("exam");
    renderPlayer();
    await screen.findByRole("heading", { name: "Which findings should the nurse report?" });

    await userEvent.click(screen.getByRole("button", { name: "Submit test" }));
    await userEvent.click(screen.getByRole("button", { name: "Keep working" }));
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: "Submit test" }));
    await userEvent.keyboard("{Escape}");
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    expect(postsTo("/api/v1/attempts/{id}/submit")).toHaveLength(0);
  });

  it("submits and goes to the results page", async () => {
    serve("exam", { "/api/v1/attempts/{id}/submit": () => reply(200, submitResult) });
    renderPlayer();
    await screen.findByRole("heading", { name: "Which findings should the nurse report?" });

    await userEvent.click(screen.getByRole("button", { name: "Submit test" }));
    await userEvent.click(within(screen.getByRole("dialog")).getByRole("button", { name: "Submit test" }));

    await waitFor(() => expect(push).toHaveBeenCalledWith(`/attempts/${attemptId}/results`));
    expect(postsTo("/api/v1/attempts/{id}/submit")[0][1].body).toEqual({ finalResponses: [] });
  });

  it("sends an answer it could not save as a final response", async () => {
    serve("exam", { "/api/v1/attempts/{id}/submit": () => reply(200, submitResult) });
    // Offline: the autosave fails at the network level, so the answer stays queued.
    putMock.mockRejectedValue(new TypeError("Failed to fetch"));
    renderPlayer();
    await userEvent.click(await screen.findByRole("checkbox", { name: "Finding e" }));

    await userEvent.click(screen.getByRole("button", { name: "Submit test" }));
    await userEvent.click(within(screen.getByRole("dialog")).getByRole("button", { name: "Submit test" }));

    await waitFor(() => expect(postsTo("/api/v1/attempts/{id}/submit")).toHaveLength(1));
    expect(postsTo("/api/v1/attempts/{id}/submit")[0][1].body).toEqual({
      finalResponses: [{ position: 1, response: { selected: ["a", "b", "d", "e"] }, clientSeq: 4 }],
    });
  });

  it("says so when the submit fails and stays on the test", async () => {
    serve("exam", { "/api/v1/attempts/{id}/submit": () => reply(500, { code: "INTERNAL", title: "Oops" }) });
    renderPlayer();
    await screen.findByRole("heading", { name: "Which findings should the nurse report?" });

    await userEvent.click(screen.getByRole("button", { name: "Submit test" }));
    await userEvent.click(within(screen.getByRole("dialog")).getByRole("button", { name: "Submit test" }));

    expect(await within(screen.getByRole("dialog")).findByRole("alert")).toHaveTextContent("Could not submit the test");
    expect(push).not.toHaveBeenCalled();
  });

  it("in tutor mode submits one answer and shows its rationale with partial credit", async () => {
    serve("tutor", {
      "/api/v1/attempts/{id}/items/{position}/submit": () => reply(200, { reveal }),
    });
    renderPlayer();
    await screen.findByRole("heading", { name: "Which findings should the nurse report?" });

    await userEvent.click(screen.getByRole("button", { name: "Submit answer" }));

    const rationale = await screen.findByRole("region", { name: "Rationale" });
    expect(within(rationale).getByRole("heading", { name: "Partially correct: 1 of 3 points" })).toBeInTheDocument();
    expect(rationale).toHaveTextContent("Report a, b and e.");
    expect(rationale).toHaveTextContent("Recognise signs of deterioration.");
    // Marks are text, not colour alone.
    expect(screen.getByRole("checkbox", { name: /Finding d ✗ Incorrect/ })).toBeDisabled();
    expect(screen.getByRole("checkbox", { name: /Finding e Correct answer, not selected/ })).not.toBeChecked();
    expect(screen.queryByRole("button", { name: "Submit answer" })).not.toBeInTheDocument();
  });

  it("offers no per-question submit in exam mode", async () => {
    serve("exam");
    renderPlayer();
    await screen.findByRole("heading", { name: "Which findings should the nurse report?" });

    expect(screen.queryByRole("button", { name: "Submit answer" })).not.toBeInTheDocument();
    expect(screen.queryByRole("region", { name: "Rationale" })).not.toBeInTheDocument();
  });
});
