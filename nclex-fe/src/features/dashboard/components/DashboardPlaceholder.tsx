"use client";

import { useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import styles from "./DashboardPlaceholder.module.css";
import { VerifyEmailBanner } from "./VerifyEmailBanner";

/**
 * Placeholder until the real dashboard (drafts, last results, weakest categories) arrives in a
 * later feature (interfaces.md 5.2).
 */
export function DashboardPlaceholder() {
  const { data: me } = useMe();
  const headingRef = useRef<HTMLHeadingElement>(null);

  return (
    <section aria-labelledby="dashboard-heading" className={styles.dashboard}>
      {me && !me.emailVerified && <VerifyEmailBanner user={me} onDismiss={() => headingRef.current?.focus()} />}

      <div className={styles.welcome}>
        <div>
          <h1 id="dashboard-heading" ref={headingRef} tabIndex={-1}>
            Dashboard
          </h1>
          {me && (
            <p className={styles.greeting}>
              Welcome, {me.displayName ?? me.email}. You are preparing for the NCLEX-{me.track}.
            </p>
          )}
        </div>
        {me && <span className={`${ui.badge} ${styles.trackBadge}`}>NCLEX-{me.track}</span>}
      </div>

      <div className={styles.grid}>
        <article className={`${ui.card} ${styles.empty}`}>
          <h2 className={ui.cardTitle}>Practice tests</h2>
          <p className={ui.muted}>Your practice tests and results will appear here.</p>
        </article>
        <article className={`${ui.card} ${styles.empty}`}>
          <h2 className={ui.cardTitle}>Performance</h2>
          <p className={ui.muted}>Once you finish a test, your strongest and weakest categories show up here.</p>
        </article>
      </div>
    </section>
  );
}
