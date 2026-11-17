import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { AttemptState, CandidateItem } from "../api/attemptApi";
import { AttemptPlayer } from "./AttemptPlayer";

// The adaptive exam (F19) in the real player: only the network and the router are replaced.
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

const attemptId = "attempt-cat";

/** An adaptive attempt on question `position`, every earlier one answered and locked. */
function adaptiveState(position = 1, status: AttemptState["status"] = "in_progress"): AttemptState {
  return {
    id: attemptId,
    testSet: { id: "set-cat", title: "NCLEX-RN adaptive exam" },
    status,
    settings: { mode: "cat", revealMode: "exam", allowBackNav: false, perQuestionSec: null, timeBudgetSec: null, passThreshold: null },
    totalItems: position,
    currentPosition: position,
    progress: { answered: position - 1, locked: position - 1, flagged: 0 },
    navigator: Array.from({ length: position }, (_, i) => ({
      position: i + 1,
      state: i + 1 < position ? ("locked" as const) : ("seen" as const),
      flagged: false,
    })),
    timer: { serverNow: "2026-11-17T09:00:00Z", exam: null, item: null, pause: { used: 0, max: null, maxPauseSec: null, startedAt: null } },
    startedAt: "2026-11-17T08:00:00Z",
    lastActivityAt: "2026-11-17T09:00:00Z",
    staleDraft: false,
    resultAvailable: status === "submitted",
    adaptive: { answered: position - 1, minItems: 85, maxItems: 150, timed: false },
  };
}

function item(position: number): CandidateItem {
  return {
    position,
    questionVersionId: `v-${position}`,
    itemType: "multiple_choice",
    stem: `Question text ${position}`,
    content: { options: [{ id: "a", text: "Option A" }, { id: "b", text: "Option B" }] },
    exhibits: [],
    caseStudy: null,
    state: "seen",
    response: null,
    clientSeq: 0,
    flagged: false,
    struckOptions: [],
    timeAllotmentSec: 120,
    reveal: null,
  };
}

function serve(state: () => AttemptState) {
  getMock.mockImplementation(((path: string, init: { params: { path: { position?: number } } }) => {
    if (path === "/api/v1/attempts/{id}/items/{position}") return Promise.resolve(reply(200, item(init.params.path.position!)));
    return Promise.resolve(reply(200, state()));
  }) as never);
}

function renderPlayer() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <AttemptPlayer attemptId={attemptId} />
    </QueryClientProvider>,
  );
}

function callsOf(mock: { mock: { calls: unknown } }) {
  return mock.mock.calls as unknown[][];
}

describe("AttemptPlayer, adaptive exam", () => {
  beforeEach(() => {
    localStorage.clear();
    getMock.mockReset();
    postMock.mockReset();
    putMock.mockReset();
    push.mockReset();
    putMock.mockResolvedValue(reply(200, { clientSeq: 1 }));
  });

  it("shows one question at a time with no total, navigator, marking, going back or Submit", async () => {
    postMock.mockResolvedValue(reply(200, adaptiveState(3)));
    serve(() => adaptiveState(3));

    renderPlayer();

    expect(await screen.findByRole("heading", { name: "Question text 3" })).toBeInTheDocument();
    expect(screen.getByText("Question 3")).toBeInTheDocument();
    expect(screen.queryByText(/of 3/)).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "All questions" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Mark for review/ })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "◀ Previous" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Submit test" })).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Next ▶" })).toHaveAccessibleDescription("Your answer is final once you move on.");
  });

  it("asks for an answer before moving on, since the NCLEX does not allow skipping", async () => {
    postMock.mockResolvedValue(reply(200, adaptiveState(1)));
    serve(() => adaptiveState(1));
    renderPlayer();

    await userEvent.click(await screen.findByRole("button", { name: "Next ▶" }));

    expect(screen.getByRole("alert")).toHaveTextContent("Choose an answer to continue.");
    expect(callsOf(postMock).some(([path]) => path === "/api/v1/attempts/{id}/items/{position}/answer")).toBe(false);
  });

  it("sends the answer as final and shows the next question", async () => {
    let current = adaptiveState(1);
    postMock.mockImplementation(((path: string) => {
      if (path === "/api/v1/attempts/{id}/items/{position}/answer") {
        current = adaptiveState(2);
        return Promise.resolve(reply(200, { ended: false, attempt: current }));
      }
      return Promise.resolve(reply(200, current));
    }) as never);
    serve(() => current);
    renderPlayer();

    await userEvent.click(await screen.findByRole("radio", { name: "Option B" }));
    await userEvent.click(screen.getByRole("button", { name: "Next ▶" }));

    expect(await screen.findByRole("heading", { name: "Question text 2" })).toBeInTheDocument();
    expect(screen.getByText("Question 2")).toBeInTheDocument();
    const answer = callsOf(postMock).find(([path]) => path === "/api/v1/attempts/{id}/items/{position}/answer")!;
    const init = answer[1] as { params: { path: { position: number } }; body: { response: unknown; clientSeq: number } };
    expect(init.params.path.position).toBe(1);
    expect(init.body.response).toEqual({ selected: "b" });
    // The same clientSeq as the autosave sent just before, so the stored answer is this one.
    const save = callsOf(putMock).at(-1)![1] as { body: { clientSeq: number } };
    expect(init.body.clientSeq).toBe(save.body.clientSeq);
  });

  it("goes to the results when the exam ends on its own", async () => {
    postMock.mockImplementation(((path: string) => {
      if (path === "/api/v1/attempts/{id}/items/{position}/answer")
        return Promise.resolve(reply(200, { ended: true, attempt: adaptiveState(85, "submitted") }));
      return Promise.resolve(reply(200, adaptiveState(85)));
    }) as never);
    serve(() => adaptiveState(85));
    renderPlayer();

    await userEvent.click(await screen.findByRole("radio", { name: "Option A" }));
    await userEvent.click(screen.getByRole("button", { name: "Next ▶" }));

    expect(await screen.findByRole("heading", { name: "Your exam has ended" })).toBeInTheDocument();
    await waitFor(() => expect(push).toHaveBeenCalledWith(`/attempts/${attemptId}/results`));
  });
});
