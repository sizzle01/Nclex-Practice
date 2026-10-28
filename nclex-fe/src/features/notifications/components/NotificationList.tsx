"use client";

import type { NotificationView } from "../api/notificationsApi";
import styles from "./notifications.module.css";

const timeFormat = new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" });

/**
 * Notifications as a list of buttons, shared by the bell's panel and the /notifications page.
 * Each one is a button so it works with Tab and Enter; unread state is in the accessible name,
 * not only in the dot.
 */
export function NotificationList({
  items,
  onOpen,
  emptyText,
}: {
  items: NotificationView[];
  onOpen: (notification: NotificationView) => void;
  emptyText: string;
}) {
  if (items.length === 0) return <p className={styles.empty}>{emptyText}</p>;

  return (
    <ul className={styles.list}>
      {items.map((notification) => {
        const unread = notification.readAt === null;
        return (
          <li key={notification.id}>
            <button
              type="button"
              className={`${styles.item} ${unread ? styles.unread : ""}`}
              onClick={() => onOpen(notification)}
            >
              <span className={styles.dot} aria-hidden="true" />
              <span className={styles.text}>
                <span className={styles.title}>
                  {unread && <span className={styles.visuallyHidden}>Unread: </span>}
                  {notification.title}
                </span>
                <span className={styles.body}>{notification.body}</span>
                <time className={styles.time} dateTime={notification.createdAt}>
                  {timeFormat.format(new Date(notification.createdAt))}
                </time>
              </span>
            </button>
          </li>
        );
      })}
    </ul>
  );
}
