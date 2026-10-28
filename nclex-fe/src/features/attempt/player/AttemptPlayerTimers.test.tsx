import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { AttemptState, CandidateItem, TimerState } from "../api/attemptApi";
import { AttemptPlayer } from "./AttemptPlayer";

// Timers, pause and expiry in the player (F09). Only the network and the router are replaced.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn(), PUT: vi.fn(), PATCH: vi.fn() } }));
const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

const attemptId = "attempt-1";
const serverNow = "2026-10-28T09:00:00.000Z";

function examRunning(remainingSec: number): TimerState["exam"] {
  return {
    running: true,
    deadlineAt: new Date(Date.parse(serverNow) + remainingSec * 1000).toISOString(),
    remainingMs: remainingSec * 1000,
  };
}

function attemptState(overrides: Partial<AttemptState> = {}, timer: Partial<TimerState> = {}): AttemptState {
  return {
    id: attemptId,
    testSet: { id: "set-1", title: "Comprehensive Challenge 1" },
    status: "in_progress",
    settings: { mode: "challenge", revealMode: "exam", allowBackNav: true, perQuestionSec: null, timeBudgetSec: 4800, passThreshold: 0.65 },
    totalItems: 2,
    currentPosition: 1,
    progress: { answered: 0, locked: 0, flagged: 0 },
    navigator: [
      { position: 1, state: "seen", flagged: false },
      { position: 2, state: "unseen", flagged: false },
    ],
    timer: {
      serverNow,
      exam: examRunning(600),
      item: null,
      pause: { used: 0, max: 2, maxPauseSec: 600, startedAt: null },
      ...timer,
    },
    startedAt: "2026-10-28T08:00:00Z",
    lastActivityAt: serverNow,
    staleDraft: false,
    resultAvailable: false,
    ...overrides,
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

/** Answers POSTs by path; GETs return the item, or the given attempt state. */
function serve(posts: Record<string, () => unknown>, state: () => AttemptState = () => attemptState()) {
  postMock.mockImplementation(((path: string) => Promise.resolve(posts[path]())) as never);
  getMock.mockImplementation(((path: string, init: { params: { path: { position?: number } } }) =>
    Promise.resolve(
      path === "/api/v1/attempts/{id}/items/{position}" ? reply(200, item(init.params.path.position!)) : reply(200, state()),
    )) as never);
}

function renderPlayer() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <AttemptPlayer attemptId={attemptId} />
    </QueryClientProvider>,
  );
}

function postsTo(path: string) {
  return (postMock.mock.calls as unknown as [string, { body?: unknown; params: { header: Record<string, string> } }][])
    .filter(([p]) => p === path);
}

describe("AttemptPlayer timers and pause", () => {
  beforeEach(() => {
    localStorage.clear();
    getMock.mockReset();
    postMock.mockReset();
    push.mockReset();
    vi.useFakeTimers({ shouldAdvanceTime: true });
    vi.setSystemTime(new Date(serverNow));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("shows the exam time from the server's deadline, whatever the local clock says", async () => {
    vi.setSystemTime(new Date("2026-10-28T15:42:00Z")); // local clock hours fast
    serve({ "/api/v1/attempts/{id}/open": () => reply(200, attemptState()) });

    renderPlayer();

    expect(await screen.findByRole("timer", { name: /Test time remaining/ })).toHaveTextContent("10:00");
    await act(() => vi.advanceTimersByTimeAsync(61_000));
    expect(screen.getByRole("timer", { name: /Test time remaining/ })).toHaveTextContent("08:59");
  });

  it("pauses for a break behind an overlay that hides the question, then resumes", async () => {
    const pausedTimer = { exam: { running: false, deadlineAt: null, remainingMs: 600_000 }, pause: { used: 1, max: 2, maxPauseSec: 600, startedAt: serverNow } };
    serve({
      "/api/v1/attempts/{id}/open": () => reply(200, attemptState()),
      "/api/v1/attempts/{id}/pause": () => reply(200, { status: "paused", timer: attemptState({}, pausedTimer).timer }),
      "/api/v1/attempts/{id}/resume": () => reply(200, attemptState()),
    });
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    renderPlayer();
    await screen.findByRole("heading", { name: "Question text 1" });

    await user.click(screen.getByRole("button", { name: "Pause" }));

    const overlay = await screen.findByRole("dialog", { name: "Test paused" });
    expect(overlay).toHaveAccessibleDescription(/1 of 2 pauses left/);
    expect(screen.queryByRole("heading", { name: "Question text 1" })).not.toBeInTheDocument();
    expect(screen.getByRole("timer", { name: /paused/ })).toHaveTextContent("10:00");
    const [[, pauseInit]] = postsTo("/api/v1/attempts/{id}/pause");
    expect(pauseInit.body).toEqual({ reason: "break" });
    expect(pauseInit.params.header["X-Attempt-Client"]).toMatch(/^c-/);

    await user.click(screen.getByRole("button", { name: "Resume" }));

    expect(await screen.findByRole("heading", { name: "Question text 1" })).toBeInTheDocument();
    expect(screen.queryByRole("dialog", { name: "Test paused" })).not.toBeInTheDocument();
  });

  it("opens a paused attempt behind the overlay without loading the question", async () => {
    const paused = attemptState({ status: "paused" }, { exam: { running: false, deadlineAt: null, remainingMs: 300_000 } });
    serve({ "/api/v1/attempts/{id}/open": () => reply(200, paused) }, () => paused);

    renderPlayer();

    expect(await screen.findByRole("dialog", { name: "Test paused" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Resume" })).toHaveFocus();
    // The client's overloaded methods make vitest type the recorded calls as never.
    const paths = (getMock.mock.calls as unknown as [string][]).map(([path]) => path);
    expect(paths).not.toContain("/api/v1/attempts/{id}/items/{position}");
  });

  it("saves and exits to the dashboard from the ⋯ menu", async () => {
    serve({
      "/api/v1/attempts/{id}/open": () => reply(200, attemptState()),
      "/api/v1/attempts/{id}/pause": () => reply(200, { status: "paused", timer: attemptState().timer }),
    });
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    renderPlayer();
    await screen.findByRole("heading", { name: "Question text 1" });

    await user.click(screen.getByRole("button", { name: "More options" }));
    expect(screen.getByRole("menuitem", { name: "Save & Exit" })).toHaveFocus();
    await user.keyboard("{Enter}");

    await waitFor(() => expect(push).toHaveBeenCalledWith("/dashboard"));
    expect(postsTo("/api/v1/attempts/{id}/pause")[0][1].body).toEqual({ reason: "save_exit" });
  });

  it("closes the ⋯ menu with Escape and returns focus to its button", async () => {
    serve({ "/api/v1/attempts/{id}/open": () => reply(200, attemptState()) });
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    renderPlayer();
    await screen.findByRole("heading", { name: "Question text 1" });

    await user.click(screen.getByRole("button", { name: "More options" }));
    await user.keyboard("{Escape}");

    expect(screen.queryByRole("menu")).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "More options" })).toHaveFocus();
  });

  it("disables Pause when every pause is used, and explains a refused pause", async () => {
    serve({
      "/api/v1/attempts/{id}/open": () => reply(200, attemptState()),
      "/api/v1/attempts/{id}/pause": () => reply(409, { code: "PAUSE_LIMIT_REACHED", title: "No pauses left" }),
    });
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    renderPlayer();
    await screen.findByRole("heading", { name: "Question text 1" });

    await user.click(screen.getByRole("button", { name: "Pause" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("You have used all the pauses this test allows.");
  });

  it("shows No pauses left when the limit is used up", async () => {
    const used = attemptState({}, { pause: { used: 2, max: 2, maxPauseSec: 600, startedAt: null } });
    serve({ "/api/v1/attempts/{id}/open": () => reply(200, used) }, () => used);

    renderPlayer();

    expect(await screen.findByRole("button", { name: "Pause" })).toBeDisabled();
    expect(screen.getByText("No pauses left")).toBeInTheDocument();
  });

  it("at zero disables the answers, waits out the grace, then submits and goes to the results", async () => {
    const ending = attemptState({}, { exam: examRunning(5) });
    serve(
      {
        "/api/v1/attempts/{id}/open": () => reply(200, ending),
        "/api/v1/attempts/{id}/submit": () => reply(409, { code: "ATTEMPT_TIME_EXPIRED", title: "The time limit has passed." }),
      },
      () => ending,
    );
    renderPlayer();
    await screen.findByRole("heading", { name: "Question text 1" });
    expect(screen.getByRole("radio", { name: "Option A" })).toBeEnabled();

    await act(() => vi.advanceTimersByTimeAsync(5_000));
    expect(screen.getByRole("radio", { name: "Option A" })).toBeDisabled();
    expect(screen.getByText("Time is up. Your answers are being submitted…")).toBeInTheDocument();
    expect(postsTo("/api/v1/attempts/{id}/submit")).toHaveLength(0);

    await act(() => vi.advanceTimersByTimeAsync(3_000));

    expect(await screen.findByRole("heading", { name: "Time is up" })).toBeInTheDocument();
    expect(postsTo("/api/v1/attempts/{id}/submit")).toHaveLength(1);
    expect(push).toHaveBeenCalledWith(`/attempts/${attemptId}/results`);
  });

  it("goes to the results when the attempt expired while the browser was closed", async () => {
    serve({ "/api/v1/attempts/{id}/open": () => reply(409, { code: "ATTEMPT_TIME_EXPIRED", title: "The time limit has passed." }) });

    renderPlayer();

    expect(await screen.findByRole("heading", { name: "Time is up" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "See your results" })).toHaveAttribute("href", `/attempts/${attemptId}/results`);
    expect(push).toHaveBeenCalledWith(`/attempts/${attemptId}/results`);
  });

  it("locks the answers when the question timer runs out", async () => {
    const perItem = attemptState(
      { settings: { ...attemptState().settings, perQuestionSec: 30, timeBudgetSec: null } },
      { exam: null, item: { running: true, deadlineAt: new Date(Date.parse(serverNow) + 30_000).toISOString(), remainingMs: 30_000 } },
    );
    serve({ "/api/v1/attempts/{id}/open": () => reply(200, perItem) }, () => perItem);
    renderPlayer();

    expect(await screen.findByRole("timer", { name: /for this question/ })).toHaveTextContent("00:30");
    await act(() => vi.advanceTimersByTimeAsync(30_000));

    expect(screen.getByRole("radio", { name: "Option A" })).toBeDisabled();
    expect(screen.getByText(/Time is up for this question/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Next ▶" })).toBeEnabled();
  });
});
