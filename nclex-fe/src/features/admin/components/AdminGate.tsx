"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { useMe } from "@/features/auth/AuthProvider";
import type { UserRole } from "../api/adminApi";
import { isContentStaff } from "../roles";

interface AdminGateProps {
  children: ReactNode;
  /** Who may see this page; content staff when left out. */
  allow?: (role: UserRole | undefined) => boolean;
}

/**
 * Shows the admin console only to staff with the right role. Like RequireAuth this is a
 * convenience, not security: the API answers 403 to anyone else (api.md section 8).
 */
export function AdminGate({ children, allow = isContentStaff }: AdminGateProps) {
  const { data: me, isPending } = useMe();

  if (isPending) {
    return (
      <p role="status" aria-live="polite">
        Checking your access…
      </p>
    );
  }

  if (!allow(me?.role)) {
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
