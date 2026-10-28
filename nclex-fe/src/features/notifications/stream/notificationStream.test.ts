import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api/errors";
import { fetchStreamTicket } from "../api/notificationsApi";
import { FakeEventSource } from "../test/FakeEventSource";
import { aNotification } from "../test/notifications";
import { connectNotificationStream, reconnectDelayMs } from "./notificationStream";

vi.mock("../api/notificationsApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/notificationsApi")>()),
  fetchStreamTicket: vi.fn(),
}));
const ticketMock = vi.mocked(fetchStreamTicket);

function handlers() {
  return { onNotification: vi.fn(), onUnreadCount: vi.fn(), onConnectedChange: vi.fn() };
}

describe("connectNotificationStream", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.stubGlobal("EventSource", FakeEventSource);
    FakeEventSource.reset();
    let n = 0;
    ticketMock.mockReset().mockImplementation(async () => ({ ticket: `t-${++n}`, expiresInSec: 30 }));
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.unstubAllGlobals();
  });

  it("opens the stream with a ticket and passes events on", async () => {
    const on = handlers();
    connectNotificationStream(on);
    await vi.waitFor(() => expect(FakeEventSource.instances).toHaveLength(1));

    FakeEventSource.latest.emit("unread_count", { count: 3 });
    FakeEventSource.latest.emit("notification", aNotification());

    expect(FakeEventSource.latest.url).toBe("/api/v1/notifications/stream?ticket=t-1");
    expect(on.onConnectedChange).toHaveBeenLastCalledWith(true);
    expect(on.onUnreadCount).toHaveBeenCalledWith(3);
    expect(on.onNotification).toHaveBeenCalledWith(aNotification());
  });

  it("reconnects with a new ticket when the stream drops", async () => {
    const on = handlers();
    connectNotificationStream(on);
    await vi.waitFor(() => expect(FakeEventSource.instances).toHaveLength(1));

    FakeEventSource.latest.fail();
    expect(on.onConnectedChange).toHaveBeenLastCalledWith(false);
    expect(FakeEventSource.instances[0].closed).toBe(true);
    await vi.advanceTimersByTimeAsync(1_000);

    expect(FakeEventSource.instances).toHaveLength(2);
    expect(FakeEventSource.latest.url).toContain("ticket=t-2");
  });

  it("backs off while tickets are refused", async () => {
    ticketMock.mockRejectedValue(new ApiError(503, "STREAM_UNAVAILABLE", [], "Down"));
    connectNotificationStream(handlers());
    await vi.advanceTimersByTimeAsync(0);
    expect(ticketMock).toHaveBeenCalledTimes(1);

    await vi.advanceTimersByTimeAsync(1_000);
    expect(ticketMock).toHaveBeenCalledTimes(2);
    await vi.advanceTimersByTimeAsync(1_999);
    expect(ticketMock).toHaveBeenCalledTimes(2);
    await vi.advanceTimersByTimeAsync(1);
    expect(ticketMock).toHaveBeenCalledTimes(3);
  });

  it("stops for good when closed", async () => {
    const close = connectNotificationStream(handlers());
    await vi.waitFor(() => expect(FakeEventSource.instances).toHaveLength(1));

    close();
    await vi.advanceTimersByTimeAsync(60_000);

    expect(FakeEventSource.latest.closed).toBe(true);
    expect(FakeEventSource.instances).toHaveLength(1);
  });
});

describe("reconnectDelayMs", () => {
  it("doubles from one second up to thirty", () => {
    expect([1, 2, 3, 4, 5, 6, 7].map(reconnectDelayMs)).toEqual([1_000, 2_000, 4_000, 8_000, 16_000, 30_000, 30_000]);
  });
});
