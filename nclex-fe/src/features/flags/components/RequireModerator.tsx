"use client";

import type { ReactNode } from "react";
import { canReview } from "@/features/admin/roles";
import { useMe } from "@/features/auth/AuthProvider";
import { RequireAuth } from "@/features/auth/components/RequireAuth";

/**
 * Shows its children only to reviewers and admins, who work the content-report queue
 * (api.md section 8: R and Ad). The admin shell already lets authors in, so this narrows it.
 * Like RequireAuth this is a convenience: the API refuses everyone else with 403 anyway.
 */
export function RequireModerator({ children }: { children: ReactNode }) {
  return (
    <RequireAuth>
      <ModeratorOnly>{children}</ModeratorOnly>
    </RequireAuth>
  );
}

function ModeratorOnly({ children }: { children: ReactNode }) {
  const me = useMe();
  if (me.isPending) return <p role="status">Checking your access…</p>;
  if (!canReview(me.data?.role)) {
    return <p role="alert">This page is for reviewers and admins.</p>;
  }
  return <>{children}</>;
}
