"use client";

import Link from "next/link";
import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { useMarkAllRead, useNotificationFeed } from "../api/notificationQueries";
import { NotificationList } from "./NotificationList";
import styles from "./notifications.module.css";
import { useOpenNotification } from "./useOpenNotification";

/** /notifications (interfaces.md 5.2): the full history, all or unread only, a page at a time. */
export function NotificationsHistory() {
  const [unreadOnly, setUnreadOnly] = useState(false);
  const feed = useNotificationFeed({ unreadOnly });
  const markAll = useMarkAllRead();
  const openNotification = useOpenNotification();
  const items = feed.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className={styles.page}>
      <div className={styles.pageHeader}>
        <h1>Notifications</h1>
        <Link href="/settings/notifications" className={ui.buttonGhost}>
          Notification settings
        </Link>
      </div>

      <div className={styles.toolbar}>
        <div role="group" aria-label="Show" className={styles.filters}>
          <button type="button" aria-pressed={!unreadOnly} className={styles.filter} onClick={() => setUnreadOnly(false)}>
            All
          </button>
          <button type="button" aria-pressed={unreadOnly} className={styles.filter} onClick={() => setUnreadOnly(true)}>
            Unread
          </button>
        </div>
        <button type="button" className={ui.buttonSecondary} disabled={markAll.isPending} onClick={() => markAll.mutate()}>
          Mark all as read
        </button>
      </div>

      <section className={ui.card} aria-label={unreadOnly ? "Unread notifications" : "All notifications"}>
        {feed.isPending ? (
          <p role="status">Loading your notifications…</p>
        ) : feed.isError ? (
          <p role="alert">We couldn&apos;t load your notifications. Please refresh the page.</p>
        ) : (
          <NotificationList
            items={items}
            onOpen={openNotification}
            emptyText={unreadOnly ? "No unread notifications." : "You have no notifications yet."}
          />
        )}
      </section>

      {feed.hasNextPage && (
        <button
          type="button"
          className={`${ui.buttonSecondary} ${styles.more}`}
          disabled={feed.isFetchingNextPage}
          onClick={() => void feed.fetchNextPage()}
        >
          {feed.isFetchingNextPage ? "Loading…" : "Load more"}
        </button>
      )}
    </div>
  );
}
