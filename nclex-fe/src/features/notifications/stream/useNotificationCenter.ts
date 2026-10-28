"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef, useState } from "react";
import { notificationKeys, useMarkAllRead, useMarkRead, useNotificationFeed, useUnreadCount } from "../api/notificationQueries";
import type { NotificationView } from "../api/notificationsApi";
import { connectNotificationStream } from "./notificationStream";

/** How many notifications the bell's panel shows. */
export const bellPageSize = 8;

/**
 * The bell's state (interfaces.md 5.5): the unread count fed by SSE, with a 60-second polling
 * fallback while the stream is down, and the first page of the feed once the panel opens.
 */
export function useNotificationCenter({ enabled, panelOpen }: { enabled: boolean; panelOpen: boolean }) {
  const queryClient = useQueryClient();
  const [connected, setConnected] = useState(false);
  const disconnectRef = useRef<(() => void) | null>(null);

  const unread = useUnreadCount({ enabled, poll: !connected });
  const feed = useNotificationFeed({ pageSize: bellPageSize, enabled: enabled && panelOpen });
  const markReadMutation = useMarkRead();
  const markAllMutation = useMarkAllRead();

  const connect = useCallback(() => {
    disconnectRef.current?.();
    disconnectRef.current = connectNotificationStream({
      onUnreadCount: (count) => queryClient.setQueryData(notificationKeys.unreadCount, count),
      // The feeds refetch rather than splice the new item in, so paging stays consistent.
      onNotification: () => queryClient.invalidateQueries({ queryKey: notificationKeys.feeds }),
      onConnectedChange: setConnected,
    });
  }, [queryClient]);

  useEffect(() => {
    if (!enabled) return;
    connect();
    return () => {
      disconnectRef.current?.();
      disconnectRef.current = null;
      setConnected(false);
    };
  }, [enabled, connect]);

  return {
    unreadCount: unread.data ?? 0,
    items: feed.data?.pages[0]?.items ?? [],
    isLoadingItems: feed.isPending && panelOpen,
    connected,
    connect,
    markRead: (notification: NotificationView) => markReadMutation.mutateAsync(notification),
    markAllRead: () => markAllMutation.mutateAsync(),
  };
}
