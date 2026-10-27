"use client";

import Link from "next/link";
import ui from "@/components/ui/ui.module.css";
import { useAuth } from "@/features/auth/AuthProvider";

/** Landing-page call to action: sign-up and log-in for visitors, the dashboard for signed-in users. */
export function HomeActions({ className }: { className?: string }) {
  const { status } = useAuth();

  if (status === "authenticated") {
    return (
      <div className={className}>
        <Link href="/dashboard" className={ui.button}>
          Go to your dashboard
        </Link>
      </div>
    );
  }

  return (
    <div className={className}>
      <Link href="/signup" className={ui.button}>
        Get started free
      </Link>
      <Link href="/login" className={ui.buttonSecondary}>
        Log in
      </Link>
    </div>
  );
}
