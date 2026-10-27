import type { Metadata } from "next";
import { Suspense } from "react";
import { AuthCard } from "@/components/app-shell/AuthCard";
import { VerifyEmailResult } from "@/features/auth/components/VerifyEmailResult";

export const metadata: Metadata = { title: "Verify your email | NCLEX Practice" };

// Suspense: the result reads ?token= from the URL, which is only known when the page is requested.
export default function VerifyEmailPage() {
  return (
    <AuthCard>
      <Suspense>
        <VerifyEmailResult />
      </Suspense>
    </AuthCard>
  );
}
