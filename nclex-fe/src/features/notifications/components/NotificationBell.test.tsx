import { act, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { ApiError } from "@/lib/api/errors";
import {
  fetchNotifications,
  fetchStreamTicket,
  fetchUnreadCount,
  markAllNotificationsRead,
  markNotificationRead,
} from "../api/notificationsApi";
import { FakeEventSource } from "../test/FakeEventSource";
import { aNotification } from "../test/notifications";
import { NotificationBell } from "./NotificationBell";

const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));
vi.mock("../api/notificationsApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/notificationsApi")>()),
  fetchNotifications: vi.fn(),
  fetchUnreadCount: vi.fn(),
  fetchStreamTicket: vi.fn(),
  markNotificationRead: vi.fn(),
  markAllNotificationsRead: vi.fn(),
}));
const feedMock = vi.mocked(fetchNotifications);
const countMock = vi.mocked(fetchUnreadCount);
const ticketMock = vi.mocked(fetchStreamTicket);
const markReadMock = vi.mocked(markNotificationRead);
const markAllMock = vi.mocked(markAllNotificationsRead);

function renderBell() {
  return renderWithAuth(<NotificationBell />, { status: "authenticated" });
}

describe("NotificationBell", () => {
  beforeEach(() => {
    vi.stubGlobal("EventSource", FakeEventSource);
    FakeEventSource.reset();
    push.mockReset();
    countMock.mockReset().mockResolvedValue(2);
    ticketMock.mockReset().mockResolvedValue({ ticket: "t-1", expiresInSec: 30 });
    feedMock.mockReset().mockResolvedValue({ items: [aNotification()], nextCursor: null });
    markReadMock.mockReset().mockResolvedValue();
    markAllMock.mockReset().mockResolvedValue();
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.unstubAllGlobals();
  });

  it("shows nothing when signed out", () => {
    renderWithAuth(<NotificationBell />, { status: "anonymous" });

    expect(screen.queryByRole("button", { name: /Notifications/ })).not.toBeInTheDocument();
  });

  it("shows the unread count in the badge and the button's name", async () => {
    renderBell();

    expect(await screen.findByRole("button", { name: "Notifications, 2 unread" })).toBeInTheDocument();
  });

  it("updates the badge when the stream says so", async () => {
    renderBell();
    await screen.findByRole("button", { name: "Notifications, 2 unread" });
    await waitFor(() => expect(FakeEventSource.instances).toHaveLength(1));

    act(() => FakeEventSource.latest.emit("unread_count", { count: 5 }));

    expect(await screen.findByRole("button", { name: "Notifications, 5 unread" })).toBeInTheDocument();
  });

  it("polls the count every minute while the stream is down", async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
    ticketMock.mockRejectedValue(new ApiError(503, "STREAM_UNAVAILABLE", [], "Down"));
    renderBell();
    await screen.findByRole("button", { name: "Notifications, 2 unread" });
    expect(countMock).toHaveBeenCalledTimes(1);

    countMock.mockResolvedValue(4);
    await act(() => vi.advanceTimersByTimeAsync(60_000));

    expect(await screen.findByRole("button", { name: "Notifications, 4 unread" })).toBeInTheDocument();
  });

  it("lists the latest notifications when opened", async () => {
    const user = userEvent.setup();
    renderBell();

    await user.click(await screen.findByRole("button", { name: "Notifications, 2 unread" }));

    expect(await screen.findByRole("button", { name: /^Unread:\s*You didn't reach the passing threshold/ })).toBeInTheDocument();
    expect(feedMock).toHaveBeenCalledWith({ cursor: null, unreadOnly: false, limit: 8 });
    expect(screen.getByRole("link", { name: "See all notifications" })).toHaveAttribute("href", "/notifications");
  });

  it("opens a result's breakdown and marks it read when clicked", async () => {
    const user = userEvent.setup();
    renderBell();
    await user.click(await screen.findByRole("button", { name: /Notifications/ }));

    await user.click(await screen.findByRole("button", { name: /You didn't reach the passing threshold/ }));

    expect(markReadMock).toHaveBeenCalledWith("n-1");
    expect(push).toHaveBeenCalledWith("/attempts/a-1/results#breakdown");
    expect(screen.queryByRole("region", { name: "Latest notifications" })).not.toBeInTheDocument();
  });

  it("marks everything read and clears the badge", async () => {
    const user = userEvent.setup();
    renderBell();
    await user.click(await screen.findByRole("button", { name: "Notifications, 2 unread" }));

    await user.click(await screen.findByRole("button", { name: "Mark all as read" }));

    expect(markAllMock).toHaveBeenCalled();
    expect(await screen.findByRole("button", { name: "Notifications" })).toBeInTheDocument();
  });

  it("closes with Escape and returns focus to the bell", async () => {
    const user = userEvent.setup();
    renderBell();
    const bell = await screen.findByRole("button", { name: /Notifications/ });
    await user.click(bell);
    expect(screen.getByRole("region", { name: "Latest notifications" })).toBeInTheDocument();

    await user.keyboard("{Escape}");

    expect(screen.queryByRole("region", { name: "Latest notifications" })).not.toBeInTheDocument();
    expect(bell).toHaveFocus();
    expect(bell).toHaveAttribute("aria-expanded", "false");
  });

  it("does not follow a link to another site", async () => {
    feedMock.mockResolvedValue({ items: [aNotification({ data: { deepLink: "//evil.example" } })], nextCursor: null });
    const user = userEvent.setup();
    renderBell();
    await user.click(await screen.findByRole("button", { name: /Notifications/ }));

    await user.click(await screen.findByRole("button", { name: /You didn't reach/ }));

    expect(markReadMock).toHaveBeenCalled();
    expect(push).not.toHaveBeenCalled();
  });
});
