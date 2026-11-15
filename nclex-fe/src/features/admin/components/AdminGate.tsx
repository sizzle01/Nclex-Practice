"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { useMe } from "@/features/auth/AuthProvider";
import type { UserRole } from "../api/adminApi";
import { canAuthor, canReview, isAdmin, isContentStaff } from "../roles";

/** The role groups of api.md section 8 that a page can require. */
export type AdminAccess = "staff" | "author" | "reviewer" | "admin";

// Pages are server components, and a server component may only pass serialisable props to a
// client component, so pages name the group and the check function is looked up here.
const checks: Record<AdminAccess, (role: UserRole | undefined) => boolean> = {
  staff: isContentStaff,
  author: canAuthor,
  reviewer: canReview,
  admin: isAdmin,
};

interface AdminGateProps {
  children: ReactNode;
  /** Who may see this page; content staff when left out. */
  allow?: AdminAccess;
}

/**
 * Shows the admin console only to staff with the right role. Like RequireAuth this is a
 * convenience, not security: the API answers 403 to anyone else (api.md section 8).
 */
export function AdminGate({ children, allow = "staff" }: AdminGateProps) {
  const { data: me, isPending } = useMe();

  if (isPending) {
    return (
      <p role="status" aria-live="polite">
        Checking your access…
      </p>
    );
  }

  if (!checks[allow](me?.role)) {
    return (
      <section aria-labelledby="no-access-heading">
        <h1 id="no-access-heading">You don&apos;t have access to this page</h1>
        <p>
          This part of the admin console is for content staff with a different role. <Link href="/dashboard">Back to your dashboard</Link>
        </p>
      </section>
    );
  }

  return <>{children}</>;
}
