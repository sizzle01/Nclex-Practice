import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { fetchNotifications, markAllNotificationsRead } from "../api/notificationsApi";
import { aNotification } from "../test/notifications";
import { NotificationsHistory } from "./NotificationsHistory";

vi.mock("next/navigation", () => ({ useRouter: () => ({ push: vi.fn() }) }));
vi.mock("../api/notificationsApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/notificationsApi")>()),
  fetchNotifications: vi.fn(),
  markAllNotificationsRead: vi.fn(),
}));
const feedMock = vi.mocked(fetchNotifications);
const markAllMock = vi.mocked(markAllNotificationsRead);

describe("NotificationsHistory", () => {
  beforeEach(() => {
    markAllMock.mockReset().mockResolvedValue();
    feedMock.mockReset().mockImplementation(async ({ cursor, unreadOnly }) => {
      if (unreadOnly) return { items: [aNotification({ id: "u", title: "Unread one" })], nextCursor: null };
      return cursor === null
        ? { items: [aNotification({ id: "a", title: "Newest" })], nextCursor: "c-2" }
        : { items: [aNotification({ id: "b", title: "Older", readAt: "2026-10-28T10:00:00Z" })], nextCursor: null };
    });
  });

  it("loads more pages on request", async () => {
    const user = userEvent.setup();
    renderWithAuth(<NotificationsHistory />, { status: "authenticated" });
    expect(await screen.findByText("Newest")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Load more" }));

    expect(await screen.findByText("Older")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Load more" })).not.toBeInTheDocument();
    expect(feedMock).toHaveBeenLastCalledWith({ cursor: "c-2", unreadOnly: false, limit: 20 });
  });

  it("shows only unread notifications when asked", async () => {
    const user = userEvent.setup();
    renderWithAuth(<NotificationsHistory />, { status: "authenticated" });
    await screen.findByText("Newest");

    await user.click(screen.getByRole("button", { name: "Unread" }));

    expect(await screen.findByText("Unread one")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Unread" })).toHaveAttribute("aria-pressed", "true");
  });

  it("marks everything read", async () => {
    const user = userEvent.setup();
    renderWithAuth(<NotificationsHistory />, { status: "authenticated" });
    await screen.findByText("Newest");

    await user.click(screen.getByRole("button", { name: "Mark all as read" }));

    expect(markAllMock).toHaveBeenCalled();
  });
});
