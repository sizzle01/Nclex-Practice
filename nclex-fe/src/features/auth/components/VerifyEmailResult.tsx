"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { type ReactNode, useEffect, useRef } from "react";
import styles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { verifyEmail } from "../api/authApi";
import { meQueryKey, useAuth } from "../AuthProvider";
import { ResendVerificationButton } from "./ResendVerificationButton";

/**
 * The page a verification link opens (/verify-email?token=). It sends the token as soon as it
 * loads and shows the result. Works whether or not the user is signed in on this device.
 */
export function VerifyEmailResult() {
  const token = useSearchParams().get("token");
  const { status: sessionStatus } = useAuth();
  const queryClient = useQueryClient();
  const { mutate, status, error } = useMutation({
    mutationFn: verifyEmail,
    // The cached profile still says "not verified"; refetch it so the dashboard banner goes away.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: meQueryKey }),
  });

  // Tokens are single-use: a second request would fail with INVALID_TOKEN after the first
  // succeeded. React may run this effect twice in development (StrictMode), so guard it.
  const sent = useRef(false);
  useEffect(() => {
    if (!token || sent.current) return;
    sent.current = true;
    mutate(token);
  }, [token, mutate]);

  const signedIn = sessionStatus === "authenticated";

  if (!token) {
    return (
      <Result title="This link is incomplete">
        <p className={styles.lead}>Open the link in your verification email again, or copy the whole link into your browser.</p>
      </Result>
    );
  }

  if (status === "success") {
    return (
      <Result title="Your email is verified">
        <p className={styles.formSuccess}>Thanks for confirming your email address. You can now take practice tests.</p>
        <Link href={signedIn ? "/dashboard" : "/login"} className={`${ui.button} ${styles.submit}`}>
          {signedIn ? "Go to your dashboard" : "Log in"}
        </Link>
      </Result>
    );
  }

  if (status === "error") {
    return isApiError(error, "INVALID_TOKEN") ? (
      <Result title="This link is invalid or has expired">
        <p className={styles.lead}>Verification links work once and expire after 24 hours. Only the newest link you were sent works.</p>
        {signedIn ? (
          <ResendVerificationButton />
        ) : (
          <p className={styles.footer}>
            <Link href="/login?next=%2Fdashboard">Log in</Link> to get a new link.
          </p>
        )}
      </Result>
    ) : (
      <Result title="We couldn't verify your email">
        <p className={styles.formError}>Something went wrong. Please try again.</p>
        <button type="button" className={`${ui.button} ${styles.submit}`} onClick={() => mutate(token)}>
          Try again
        </button>
      </Result>
    );
  }

  return (
    <Result title="Verifying your email…">
      <p className={styles.lead}>This only takes a moment.</p>
    </Result>
  );
}

/** A polite live region, so screen readers announce the result when it replaces "Verifying…". */
function Result({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className={styles.form} aria-live="polite" aria-labelledby="verify-heading">
      <h1 id="verify-heading">{title}</h1>
      {children}
    </section>
  );
}
