"use client";

import Link from "next/link";
import { useRef } from "react";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import styles from "./Dashboard.module.css";
import { DraftsList } from "./DraftsList";
import { PerformanceOverview } from "./PerformanceOverview";
import { RecentResults } from "./RecentResults";
import { VerifyEmailBanner } from "./VerifyEmailBanner";

/**
 * /dashboard (interfaces.md 5.2): the verify-email banner, tests in progress, the latest results,
 * and the overall score with the weakest categories.
 */
export function Dashboard() {
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
        <article className={ui.card}>
          <h2 className={ui.cardTitle}>Recent results</h2>
          <RecentResults />
          <p>
            <Link href="/history" className={ui.buttonGhost}>
              See all history
            </Link>
          </p>
        </article>
        <article className={ui.card}>
          <h2 className={ui.cardTitle}>Performance</h2>
          <PerformanceOverview />
          <p>
            <Link href="/performance" className={ui.buttonSecondary}>
              See your performance
            </Link>
          </p>
        </article>
      </div>
    </section>
  );
}
