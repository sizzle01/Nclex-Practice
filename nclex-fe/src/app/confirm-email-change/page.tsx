import type { Metadata } from "next";
import { Suspense } from "react";
import { AuthCard } from "@/components/app-shell/AuthCard";
import { ConfirmEmailChangeResult } from "@/features/auth/components/ConfirmEmailChangeResult";

export const metadata: Metadata = { title: "Confirm your new email | NCLEX Practice" };

// Suspense: the result reads ?token= from the URL, which is only known when the page is requested.
export default function ConfirmEmailChangePage() {
  return (
    <AuthCard>
      <Suspense>
        <ConfirmEmailChangeResult />
      </Suspense>
    </AuthCard>
  );
}
