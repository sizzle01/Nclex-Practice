import type { Metadata } from "next";
import { AuthCard } from "@/components/app-shell/AuthCard";
import { LoginForm } from "@/features/auth/components/LoginForm";
import { RedirectIfSignedIn } from "@/features/auth/components/RedirectIfSignedIn";

export const metadata: Metadata = { title: "Log in | NCLEX Practice" };

export default function LoginPage() {
  return (
    <RedirectIfSignedIn>
      <AuthCard>
        <LoginForm />
      </AuthCard>
    </RedirectIfSignedIn>
  );
}
