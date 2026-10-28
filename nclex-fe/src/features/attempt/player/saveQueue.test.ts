import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api/errors";
import { createSaveQueue, type PendingWrite, type SendWrite } from "./saveQueue";

const storageKey = "nclex.attempt.a-1.pending";

function setup(send: SendWrite = vi.fn().mockResolvedValue(undefined)) {
  const queue = createSaveQueue({ clientId: "c-1", send, storage: localStorage, storageKey });
  return { queue, state: () => queue.getState(), send: vi.mocked(send) };
}

function stored(): Record<string, PendingWrite> {
  return JSON.parse(localStorage.getItem(storageKey) ?? "{}");
}

function apiError(status: number, code: string) {
  return new ApiError(status, code, [], code);
}

describe("save queue", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    localStorage.clear();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("waits 750 ms and sends only the latest answer for an item", async () => {
    const { state, send } = setup();

    state().enqueue(1, { selected: "a" });
    await vi.advanceTimersByTimeAsync(500);
    state().enqueue(1, { selected: "b" });
    await vi.advanceTimersByTimeAsync(749);
    expect(send).not.toHaveBeenCalled();

    await vi.advanceTimersByTimeAsync(1);

    expect(send).toHaveBeenCalledTimes(1);
    expect(send).toHaveBeenCalledWith(1, { response: { selected: "b" }, clientSeq: 2 }, { keepalive: false });
    expect(state().status).toBe("saved");
    expect(state().pending).toEqual({});
  });

  it("numbers writes above the sequence the server already holds", async () => {
    const { state, send } = setup();
    state().noteServerSeq(3, 41);

    state().enqueue(3, { selected: "a" });
    await state().flush();

    expect(send.mock.calls[0][1].clientSeq).toBe(42);
  });

  it("sends at once on flush, one write per item", async () => {
    const { state, send } = setup();
    state().enqueue(1, { selected: "a" });
    state().enqueue(2, { selected: "b" });

    await state().flush({ keepalive: true });

    expect(send.mock.calls.map(([position, , options]) => [position, options.keepalive])).toEqual([
      [1, true],
      [2, true],
    ]);
  });

  it("keeps answers on the device while offline and retries with backoff", async () => {
    const send = vi.fn<SendWrite>().mockRejectedValueOnce(new TypeError("Failed to fetch")).mockRejectedValueOnce(new TypeError("Failed to fetch")).mockResolvedValue(undefined);
    const { state } = setup(send);

    state().enqueue(1, { selected: "a" });
    await state().flush();

    expect(state().status).toBe("offline");
    expect(stored()).toEqual({ 1: { response: { selected: "a" }, clientSeq: 1 } });

    await vi.advanceTimersByTimeAsync(500); // first retry fails too
    expect(send).toHaveBeenCalledTimes(2);
    await vi.advanceTimersByTimeAsync(999);
    expect(send).toHaveBeenCalledTimes(2);
    await vi.advanceTimersByTimeAsync(1); // the second retry waits twice as long

    expect(send).toHaveBeenCalledTimes(3);
    expect(state().status).toBe("saved");
    expect(localStorage.getItem(storageKey)).toBeNull();
  });

  it("caps the retry delay at 5 seconds", async () => {
    const send = vi.fn<SendWrite>().mockRejectedValue(new TypeError("Failed to fetch"));
    const { state } = setup(send);
    state().enqueue(1, { selected: "a" });
    await state().flush();

    // 0.5 + 1 + 2 + 4 seconds, then every 5 seconds.
    await vi.advanceTimersByTimeAsync(7_500);
    const callsBefore = send.mock.calls.length;
    await vi.advanceTimersByTimeAsync(5_000);

    expect(send.mock.calls.length).toBe(callsBefore + 1);
  });

  it("picks up unsent answers from an earlier page load and numbers new writes above them", async () => {
    localStorage.setItem(
      storageKey,
      JSON.stringify({ 1: { response: { selected: "a" }, clientSeq: 7 }, 2: { response: { selected: "c" }, clientSeq: 8 } }),
    );
    const { state, send } = setup();

    state().enqueue(2, { selected: "d" });
    await state().flush();

    expect(send.mock.calls.map(([position, write]) => [position, write])).toEqual([
      [1, { response: { selected: "a" }, clientSeq: 7 }],
      [2, { response: { selected: "d" }, clientSeq: 9 }],
    ]);
  });

  it("ignores unreadable saved answers", () => {
    localStorage.setItem(storageKey, "not json");

    const { state } = setup();

    expect(state().pending).toEqual({});
  });

  it("stops and keeps the answers when another device takes the lease", async () => {
    const send = vi.fn<SendWrite>().mockRejectedValue(apiError(409, "ATTEMPT_OPENED_ELSEWHERE"));
    const { state } = setup(send);
    state().enqueue(1, { selected: "a" });

    await state().flush();
    await state().flush();

    expect(state().leaseLost).toBe(true);
    expect(send).toHaveBeenCalledTimes(1);
    expect(state().pending[1].response).toEqual({ selected: "a" });

    send.mockResolvedValue(undefined);
    state().leaseRestored();
    await state().flush();
    expect(state().pending).toEqual({});
  });

  it("drops an answer the server refused, and carries on with the rest", async () => {
    const send = vi.fn<SendWrite>().mockRejectedValueOnce(apiError(409, "ITEM_LOCKED")).mockResolvedValue(undefined);
    const { state } = setup(send);
    state().enqueue(1, { selected: "a" });
    state().enqueue(2, { selected: "b" });

    await state().flush();

    expect(send).toHaveBeenCalledTimes(2);
    expect(state().pending).toEqual({});
    expect(state().status).toBe("error");
  });

  it("keeps an answer given while the previous one was being sent", async () => {
    let finishFirst!: () => void;
    const send = vi
      .fn<SendWrite>()
      .mockImplementationOnce(() => new Promise<void>((resolve) => (finishFirst = resolve)))
      .mockResolvedValue(undefined);
    const { state } = setup(send);
    state().enqueue(1, { selected: "a" });
    const first = state().flush();

    state().enqueue(1, { selected: "b" });
    finishFirst();
    await first;

    expect(state().pending[1].response).toEqual({ selected: "b" });
    await state().flush();
    expect(send.mock.calls[1][1]).toEqual({ response: { selected: "b" }, clientSeq: 2 });
  });
});
