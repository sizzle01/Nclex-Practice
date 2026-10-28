import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { AttemptState, CandidateItem } from "../api/attemptApi";
import { AttemptPlayer } from "./AttemptPlayer";
import { pendingStorageKey } from "./saveQueue";

// Only the network and the router are replaced; the session hook, save queue and renderers are real.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn(), PUT: vi.fn(), PATCH: vi.fn() } }));
const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);
const putMock = vi.mocked(apiClient.PUT);
const patchMock = vi.mocked(apiClient.PATCH);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

const attemptId = "attempt-1";

function attemptState(currentPosition = 1): AttemptState {
  return {
    id: attemptId,
    testSet: { id: "set-1", title: "Comprehensive Practice 1" },
    status: "in_progress",
    settings: { mode: "practice", revealMode: "tutor", allowBackNav: true, perQuestionSec: null, timeBudgetSec: null, passThreshold: null },
    totalItems: 2,
    currentPosition,
    progress: { answered: 0, locked: 0, flagged: 0 },
    navigator: [
      { position: 1, state: "seen", flagged: false },
      { position: 2, state: "unseen", flagged: false },
    ],
    timer: { serverNow: "2026-10-28T09:00:00Z", exam: null, item: null, pause: { used: 0, max: null, maxPauseSec: null, startedAt: null } },
    startedAt: "2026-10-28T08:00:00Z",
    lastActivityAt: "2026-10-28T09:00:00Z",
    staleDraft: false,
    resultAvailable: false,
  };
}

function item(position: number, overrides: Partial<CandidateItem> = {}): CandidateItem {
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
    ...overrides,
  };
}

function serveItems(items: Record<number, CandidateItem>, state: () => AttemptState = () => attemptState()) {
  getMock.mockImplementation(((path: string, init: { params: { path: { position?: number } } }) => {
    if (path === "/api/v1/attempts/{id}/items/{position}") return Promise.resolve(reply(200, items[init.params.path.position!]));
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

// The client's overloaded methods make vitest type the recorded calls as never.
function callsOf(mock: { mock: { calls: unknown } }) {
  return mock.mock.calls as unknown[][];
}

function clientIdOf(call: unknown[]) {
  return (call[1] as { params: { header: Record<string, string> } }).params.header["X-Attempt-Client"];
}

describe("AttemptPlayer", () => {
  beforeEach(() => {
    localStorage.clear();
    getMock.mockReset();
    postMock.mockReset();
    putMock.mockReset();
    patchMock.mockReset();
  });

  it("opens the attempt with a client id and shows the current question", async () => {
    postMock.mockResolvedValue(reply(200, attemptState()));
    serveItems({ 1: item(1), 2: item(2) });

    renderPlayer();

    expect(await screen.findByRole("heading", { name: "Question text 1" })).toHaveFocus();
    expect(screen.getByText("Question 1 of 2")).toBeInTheDocument();
    const [path] = callsOf(postMock)[0];
    expect(path).toBe("/api/v1/attempts/{id}/open");
    expect(clientIdOf(callsOf(postMock)[0])).toMatch(/^c-/);
    expect(screen.getByRole("button", { name: "◀ Previous" })).toBeDisabled();
  });

  it("restores a saved answer and autosaves a new one with the lease header", async () => {
    postMock.mockResolvedValue(reply(200, attemptState()));
    serveItems({ 1: item(1, { response: { selected: "a" }, clientSeq: 4, state: "answered" }), 2: item(2) });
    putMock.mockResolvedValue(reply(200, { clientSeq: 5 }));
    renderPlayer();

    expect(await screen.findByRole("radio", { name: "Option A" })).toBeChecked();
    await userEvent.click(screen.getByRole("radio", { name: "Option B" }));
    expect(screen.getByRole("radio", { name: "Option B" })).toBeChecked();

    await waitFor(() => expect(putMock).toHaveBeenCalled(), { timeout: 2000 });
    const [, init] = callsOf(putMock)[0] as unknown as [string, { body: unknown; params: { header: Record<string, string> } }];
    expect(init.body).toEqual({ response: { selected: "b" }, clientSeq: 5 });
    expect(init.params.header["X-Attempt-Client"]).toBe(clientIdOf(callsOf(postMock)[0]));
    expect(await screen.findByRole("status")).toHaveTextContent("Saved");
  });

  it("sends answers left on this device by an earlier visit once the attempt is open", async () => {
    localStorage.setItem(pendingStorageKey(attemptId), JSON.stringify({ 2: { response: { selected: "a" }, clientSeq: 9 } }));
    postMock.mockResolvedValue(reply(200, attemptState()));
    serveItems({ 1: item(1), 2: item(2) });
    putMock.mockResolvedValue(reply(200, { clientSeq: 9 }));

    renderPlayer();

    await waitFor(() => expect(putMock).toHaveBeenCalled());
    expect(callsOf(putMock)[0][1]).toMatchObject({ params: { path: { id: attemptId, position: 2 } }, body: { clientSeq: 9 } });
  });

  it("moves to the next question", async () => {
    let current = 1;
    postMock.mockImplementation(((path: string) =>
      Promise.resolve(path.endsWith("/navigate") ? reply(200, { currentPosition: (current = 2) }) : reply(200, attemptState()))) as never);
    serveItems({ 1: item(1), 2: item(2) }, () => attemptState(current));
    renderPlayer();

    await userEvent.click(await screen.findByRole("button", { name: "Next ▶" }));

    expect(await screen.findByRole("heading", { name: "Question text 2" })).toBeInTheDocument();
    expect(screen.getByText("Question 2 of 2")).toBeInTheDocument();
    expect(postMock).toHaveBeenCalledWith("/api/v1/attempts/{id}/navigate", expect.objectContaining({ body: { toPosition: 2 } }));
  });

  it("offers Continue here when another device has taken the attempt", async () => {
    postMock.mockImplementation(((path: string) =>
      Promise.resolve(
        path.endsWith("/navigate")
          ? reply(409, { code: "ATTEMPT_OPENED_ELSEWHERE", title: "This attempt is open on another device." })
          : reply(200, attemptState()),
      )) as never);
    serveItems({ 1: item(1), 2: item(2) });
    renderPlayer();

    await userEvent.click(await screen.findByRole("button", { name: "Next ▶" }));
    const takeOver = await screen.findByRole("button", { name: "Continue here" });
    expect(screen.getByRole("radio", { name: "Option A" })).toBeDisabled();

    await userEvent.click(takeOver);

    await waitFor(() => expect(screen.queryByRole("alertdialog")).not.toBeInTheDocument());
    const opens = callsOf(postMock).filter(([path]) => path === "/api/v1/attempts/{id}/open");
    expect(opens).toHaveLength(2);
    expect(clientIdOf(opens[1])).toBe(clientIdOf(opens[0]));
  });

  it("strikes an option through at once and saves the full list with the lease header", async () => {
    postMock.mockResolvedValue(reply(200, attemptState()));
    serveItems({ 1: item(1, { struckOptions: ["a"] }), 2: item(2) });
    patchMock.mockResolvedValue(reply(200, { position: 1, flagged: false, struckOptions: ["a", "b"] }));
    renderPlayer();

    await userEvent.click(await screen.findByRole("button", { name: "Strike out Option B" }));

    expect(screen.getByRole("button", { name: "Strike out Option B" })).toHaveAttribute("aria-pressed", "true");
    const [path, init] = callsOf(patchMock)[0] as [string, { body: unknown; params: { header: Record<string, string> } }];
    expect(path).toBe("/api/v1/attempts/{id}/items/{position}");
    expect(init.body).toEqual({ struckOptions: ["a", "b"] });
    expect(init.params.header["X-Attempt-Client"]).toBe(clientIdOf(callsOf(postMock)[0]));
    expect(putMock).not.toHaveBeenCalled();
  });

  it("puts a strike back when the server refuses it", async () => {
    postMock.mockResolvedValue(reply(200, attemptState()));
    serveItems({ 1: item(1), 2: item(2) });
    patchMock.mockResolvedValue(reply(500, { code: "INTERNAL", title: "Failed." }));
    renderPlayer();

    await userEvent.click(await screen.findByRole("button", { name: "Strike out Option A" }));

    await waitFor(() =>
      expect(screen.getByRole("button", { name: "Strike out Option A" })).toHaveAttribute("aria-pressed", "false"),
    );
  });

  it("shows the item's exhibits beside the question", async () => {
    postMock.mockResolvedValue(reply(200, attemptState()));
    const exhibits = [
      { id: "v", kind: "flowsheet", title: "Vital signs", table: { columns: ["08:00"], rows: [{ label: "HR", cells: ["88"] }] } },
    ];
    serveItems({ 1: item(1, { exhibits }), 2: item(2) });
    renderPlayer();

    expect(await screen.findByRole("complementary", { name: "Exhibits" })).toBeInTheDocument();
    expect(screen.getByRole("table", { name: "Vital signs" })).toBeInTheDocument();
  });

  it("explains when the attempt can no longer be opened", async () => {
    postMock.mockResolvedValue(reply(409, { code: "ATTEMPT_NOT_ACTIVE", title: "Not active." }));

    renderPlayer();

    expect(await screen.findByRole("alert")).toHaveTextContent("already been submitted or discarded");
    expect(screen.getByRole("link", { name: "Back to the dashboard" })).toHaveAttribute("href", "/dashboard");
  });
});
