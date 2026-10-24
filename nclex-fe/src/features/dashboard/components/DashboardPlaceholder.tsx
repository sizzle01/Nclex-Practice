"use client";

import { useMe } from "@/features/auth/AuthProvider";

/**
 * Placeholder until the real dashboard (drafts, last results, weakest categories) arrives in a
 * later feature (interfaces.md 5.2).
 */
export function DashboardPlaceholder() {
  const { data: me } = useMe();

  return (
    <section aria-labelledby="dashboard-heading">
      <h1 id="dashboard-heading">Dashboard</h1>
      {me && (
        <p>
          Welcome, {me.displayName ?? me.email}. You are preparing for the NCLEX-{me.track}.
        </p>
      )}
      <p>Your practice tests and results will appear here.</p>
    </section>
  );
}
