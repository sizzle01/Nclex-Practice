import type { Metadata } from "next";
import { Suspense } from "react";
import { AuthCard } from "@/components/app-shell/AuthCard";
import { ResetPasswordForm } from "@/features/auth/components/ResetPasswordForm";

export const metadata: Metadata = { title: "Reset password | NCLEX Practice" };

// Suspense: the form reads ?token= from the URL, which is only known when the page is requested.
export default function ResetPasswordPage() {
  return (
    <AuthCard>
      <Suspense>
        <ResetPasswordForm />
      </Suspense>
    </AuthCard>
  );
}
