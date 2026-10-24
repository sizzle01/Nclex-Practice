import type { Metadata } from "next";
import { RedirectIfSignedIn } from "@/features/auth/components/RedirectIfSignedIn";
import { SignupForm } from "@/features/auth/components/SignupForm";

export const metadata: Metadata = { title: "Create account | NCLEX Practice" };

export default function SignupPage() {
  return (
    <RedirectIfSignedIn>
      <SignupForm />
    </RedirectIfSignedIn>
  );
}
