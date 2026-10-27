"use client";

import { useMutation } from "@tanstack/react-query";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { resendVerification } from "../api/authApi";
import styles from "./ResendVerificationButton.module.css";

/**
 * Asks the API to email a new verification link. The outcome is announced in a polite live
 * region, which is always rendered so screen readers notice when its text changes.
 */
export function ResendVerificationButton({ email }: { email?: string }) {
  const mutation = useMutation({ mutationFn: resendVerification });

  return (
    <div className={styles.resend}>
      <button
        type="button"
        className={ui.buttonSecondary}
        onClick={() => mutation.mutate()}
        disabled={mutation.isPending}
      >
        {mutation.isPending ? "Sending…" : "Resend verification email"}
      </button>
      <p role="status" className={styles.message}>
        {resultMessage(mutation.status, mutation.error, email)}
      </p>
    </div>
  );
}

function resultMessage(status: "idle" | "pending" | "success" | "error", error: unknown, email?: string): string {
  if (status === "success") return email ? `We've sent a new link to ${email}.` : "We've sent a new link.";
  if (status !== "error") return "";
  if (isApiError(error, "RATE_LIMITED")) {
    return `You've asked for several links recently. Try again in ${minutesFrom(error.retryAfterSec)}.`;
  }
  return "We couldn't send the email. Please try again.";
}

function minutesFrom(seconds = 60): string {
  const minutes = Math.max(1, Math.ceil(seconds / 60));
  return minutes === 1 ? "1 minute" : `${minutes} minutes`;
}
