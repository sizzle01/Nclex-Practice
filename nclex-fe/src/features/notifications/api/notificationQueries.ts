import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  fetchNotifications,
  fetchPreferences,
  fetchUnreadCount,
  markAllNotificationsRead,
  markNotificationRead,
  type NotificationView,
  replacePreferences,
} from "./notificationsApi";

/** How often the badge is refreshed while the live stream is down (architecture.md 5.6). */
export const pollIntervalMs = 60_000;

export const notificationKeys = {
  all: ["notifications"] as const,
  feeds: ["notifications", "feed"] as const,
  feed: (unreadOnly: boolean) => ["notifications", "feed", { unreadOnly }] as const,
  unreadCount: ["notifications", "unread-count"] as const,
  preferences: ["notifications", "preferences"] as const,
};

/** The feed a page at a time; idle until `enabled` (the bell loads it only when opened). */
export function useNotificationFeed({ unreadOnly = false, pageSize = 20, enabled = true } = {}) {
  return useInfiniteQuery({
    queryKey: notificationKeys.feed(unreadOnly),
    queryFn: ({ pageParam }) => fetchNotifications({ cursor: pageParam, unreadOnly, limit: pageSize }),
    initialPageParam: null as string | null,
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
    enabled,
  });
}

/** The unread count; polled every minute only while the live stream is not connected. */
export function useUnreadCount({ enabled = true, poll = false } = {}) {
  return useQuery({
    queryKey: notificationKeys.unreadCount,
    queryFn: fetchUnreadCount,
    enabled,
    refetchInterval: poll ? pollIntervalMs : false,
  });
}

/** Marks one read, then refreshes the badge and the feeds. */
export function useMarkRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (notification: NotificationView) => markNotificationRead(notification.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all }),
  });
}

/** Marks everything read; the badge drops to zero straight away. */
export function useMarkAllRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: markAllNotificationsRead,
    onSuccess: () => {
      queryClient.setQueryData(notificationKeys.unreadCount, 0);
      return queryClient.invalidateQueries({ queryKey: notificationKeys.feeds });
    },
  });
}

export function usePreferences() {
  return useQuery({ queryKey: notificationKeys.preferences, queryFn: fetchPreferences });
}

export function useReplacePreferences() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: replacePreferences,
    onSuccess: (preferences) => queryClient.setQueryData(notificationKeys.preferences, preferences),
  });
}

/**
 * The page a notification opens (`data.deepLink`), if it has one. Only app-relative paths are
 * followed, so a notification can never send the user to another site.
 */
export function deepLinkOf(notification: NotificationView): string | null {
  const data = notification.data as { deepLink?: unknown } | null;
  const link = data?.deepLink;
  return typeof link === "string" && link.startsWith("/") && !link.startsWith("//") ? link : null;
}
