"use client";

import { useRouter } from "next/navigation";
import { deepLinkOf, useMarkRead } from "../api/notificationQueries";
import type { NotificationView } from "../api/notificationsApi";

/**
 * What clicking a notification does (interfaces.md 5.5): mark it read, then open its deep link,
 * e.g. a result's breakdown. Navigation does not wait for the read to be saved.
 */
export function useOpenNotification(afterOpen?: () => void) {
  const router = useRouter();
  const markRead = useMarkRead();

  return (notification: NotificationView) => {
    if (notification.readAt === null) markRead.mutate(notification);
    const link = deepLinkOf(notification);
    if (link) {
      afterOpen?.();
      router.push(link);
    }
  };
}
