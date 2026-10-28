"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState } from "react";
import { useAuth } from "@/features/auth/AuthProvider";
import { useNotificationCenter } from "../stream/useNotificationCenter";
import { NotificationList } from "./NotificationList";
import styles from "./notifications.module.css";
import { useOpenNotification } from "./useOpenNotification";

/**
 * The top bar's bell (interfaces.md 5.5): an unread badge kept current over SSE, and a panel
 * with the latest notifications. Like the user menu it is a disclosure, not an ARIA menu, so Tab
 * and Enter work as everywhere else; Escape closes it and returns focus to the bell.
 */
export function NotificationBell() {
  const { status } = useAuth();
  const [open, setOpen] = useState(false);
  const panelId = useId();
  const containerRef = useRef<HTMLDivElement>(null);
  const toggleRef = useRef<HTMLButtonElement>(null);
  const center = useNotificationCenter({ enabled: status === "authenticated", panelOpen: open });
  const openNotification = useOpenNotification(() => setOpen(false));

  useEffect(() => {
    if (!open) return;
    function closeOnOutsideClick(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) setOpen(false);
    }
    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
        toggleRef.current?.focus();
      }
    }
    document.addEventListener("mousedown", closeOnOutsideClick);
    document.addEventListener("keydown", closeOnEscape);
    return () => {
      document.removeEventListener("mousedown", closeOnOutsideClick);
      document.removeEventListener("keydown", closeOnEscape);
    };
  }, [open]);

  if (status !== "authenticated") return null;

  const count = center.unreadCount;
  const label = count === 0 ? "Notifications" : `Notifications, ${count} unread`;

  return (
    <div ref={containerRef} className={styles.bell}>
      <button
        ref={toggleRef}
        type="button"
        className={styles.bellButton}
        aria-label={label}
        aria-expanded={open}
        aria-controls={panelId}
        onClick={() => setOpen((value) => !value)}
      >
        <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M6 8a6 6 0 1 1 12 0c0 7 3 9 3 9H3s3-2 3-9" strokeLinecap="round" strokeLinejoin="round" />
          <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
        {count > 0 && (
          <span className={styles.badge} aria-hidden="true">
            {count > 99 ? "99+" : count}
          </span>
        )}
      </button>
      {/* Announces new arrivals to screen readers without moving focus. */}
      <span role="status" className={styles.visuallyHidden}>
        {count > 0 ? `${count} unread notifications` : ""}
      </span>

      {open && (
        <div id={panelId} className={styles.panel} role="region" aria-label="Latest notifications">
          <div className={styles.panelHeader}>
            <h2 className={styles.panelTitle}>Notifications</h2>
            {count > 0 && (
              <button type="button" className={styles.linkButton} onClick={() => void center.markAllRead()}>
                Mark all as read
              </button>
            )}
          </div>
          {center.isLoadingItems ? (
            <p className={styles.empty} role="status">
              Loading…
            </p>
          ) : (
            <NotificationList items={center.items} onOpen={openNotification} emptyText="You're all caught up." />
          )}
          <div className={styles.panelFooter}>
            <Link href="/notifications" onClick={() => setOpen(false)}>
              See all notifications
            </Link>
            <Link href="/settings/notifications" onClick={() => setOpen(false)}>
              Settings
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}
