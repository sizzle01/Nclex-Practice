"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { type ReactNode, useEffect, useRef } from "react";
import styles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { confirmEmailChange } from "../api/authApi";
import { meQueryKey, useAuth } from "../AuthProvider";

/**
 * The page the link sent to a new email address opens (/confirm-email-change?token=). It sends the
 * token as soon as it loads, like VerifyEmailResult, and works whether or not the user is signed in.
 */
export function ConfirmEmailChangeResult() {
  const token = useSearchParams().get("token");
  const { status: sessionStatus } = useAuth();
  const queryClient = useQueryClient();
  const { mutate, status, error } = useMutation({
    mutationFn: confirmEmailChange,
    // The cached profile still shows the old address.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: meQueryKey }),
  });

  // Single-use token: guard against React running the effect twice in development.
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
        <p className={styles.lead}>Open the link in your confirmation email again, or copy the whole link into your browser.</p>
      </Result>
    );
  }

  if (status === "success") {
    return (
      <Result title="Your email address has been changed">
        <p className={styles.formSuccess}>From now on, sign in with your new email address.</p>
        <Link href={signedIn ? "/settings/account" : "/login"} className={`${ui.button} ${styles.submit}`}>
          {signedIn ? "Back to account settings" : "Log in"}
        </Link>
      </Result>
    );
  }

  if (status === "error") {
    if (isApiError(error, "INVALID_TOKEN")) {
      return (
        <Result title="This link is invalid or has expired">
          <p className={styles.lead}>
            Confirmation links work once and expire after 24 hours. Only the newest link you were sent works.
          </p>
          <SettingsLink signedIn={signedIn} />
        </Result>
      );
    }
    if (isApiError(error, "EMAIL_TAKEN")) {
      return (
        <Result title="This email address is already in use">
          <p className={styles.formError}>Another account started using this address after you asked for the change.</p>
          <SettingsLink signedIn={signedIn} />
        </Result>
      );
    }
    return (
      <Result title="We couldn't change your email">
        <p className={styles.formError}>Something went wrong. Please try again.</p>
        <button type="button" className={`${ui.button} ${styles.submit}`} onClick={() => mutate(token)}>
          Try again
        </button>
      </Result>
    );
  }

  return (
    <Result title="Confirming your new email…">
      <p className={styles.lead}>This only takes a moment.</p>
    </Result>
  );
}

function SettingsLink({ signedIn }: { signedIn: boolean }) {
  return (
    <p className={styles.footer}>
      {signedIn ? (
        <Link href="/settings/account">Go to account settings</Link>
      ) : (
        <Link href="/login?next=%2Fsettings%2Faccount">Log in</Link>
      )}{" "}
      to ask for a new link.
    </p>
  );
}

/** A polite live region, so screen readers announce the result when it replaces "Confirming…". */
function Result({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className={styles.form} aria-live="polite" aria-labelledby="confirm-email-heading">
      <h1 id="confirm-email-heading">{title}</h1>
      {children}
    </section>
  );
}
