"use client";

import { usePathname, useRouter } from "next/navigation";
import { type ReactNode, useEffect } from "react";
import { useAuth } from "../AuthProvider";

/**
 * Renders its children only for signed-in users. Anyone else is sent to /login, with ?next= so
 * they come back here afterwards. The check runs in the browser because the session lives there:
 * the access token is in memory and the refresh cookie is only sent to /api/v1/auth.
 * This is a convenience, not security: the API rejects requests without a valid token anyway.
 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { status } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (status === "anonymous") router.replace(`/login?next=${encodeURIComponent(pathname)}`);
  }, [status, router, pathname]);

  if (status === "authenticated") return <>{children}</>;

  return (
    <p role="status" aria-live="polite">
      {status === "unknown" ? "Checking your session…" : "Redirecting to log in…"}
    </p>
  );
}
