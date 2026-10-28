"use client";

import Link from "next/link";
import { useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import styles from "./DashboardPlaceholder.module.css";
import { DraftsList } from "./DraftsList";
import { VerifyEmailBanner } from "./VerifyEmailBanner";

/**
 * The dashboard so far: tests in progress (F08). Last results and weakest categories arrive with
 * results (F10) and performance (interfaces.md 5.2).
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
        <article className={ui.card}>
          <h2 className={ui.cardTitle}>Continue where you left off</h2>
          <DraftsList />
          <p>
            <Link href="/practice" className={ui.buttonSecondary}>
              Browse practice sets
            </Link>
          </p>
        </article>
        <article className={`${ui.card} ${styles.empty}`}>
          <h2 className={ui.cardTitle}>Performance</h2>
          <p className={ui.muted}>Once you finish a test, your strongest and weakest categories show up here.</p>
        </article>
      </div>
    </section>
  );
}
