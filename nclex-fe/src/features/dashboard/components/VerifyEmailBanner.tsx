"use client";

import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import type { UserProfile } from "@/features/auth/api/authApi";
import { ResendVerificationButton } from "@/features/auth/components/ResendVerificationButton";
import styles from "./VerifyEmailBanner.module.css";

// Holds the id of the user who dismissed the banner. sessionStorage, so the reminder comes back in
// the next browser session; keyed by user so another account in the same tab still sees it.
const dismissedKey = "nclex.verifyEmailBanner.dismissedFor";

interface VerifyEmailBannerProps {
  user: UserProfile;
  /** Called after the banner is dismissed, so the page can move keyboard focus somewhere sensible. */
  onDismiss?: () => void;
}

/** Reminds a user with an unverified email to verify it, with a resend button (interfaces.md 5.2). */
export function VerifyEmailBanner({ user, onDismiss }: VerifyEmailBannerProps) {
  const [dismissed, setDismissed] = useState(() => readDismissedFor() === user.id);
  if (dismissed) return null;

  function dismiss() {
    writeDismissedFor(user.id);
    setDismissed(true);
    onDismiss?.();
  }

  return (
    <section aria-labelledby="verify-banner-heading" className={styles.banner}>
      <div className={styles.text}>
        <h2 id="verify-banner-heading" className={styles.title}>
          Verify your email address
        </h2>
        <p className={styles.body}>
          We sent a link to <strong>{user.email}</strong>. You&apos;ll need a verified email to take practice tests
          beyond the free samples.
        </p>
        <ResendVerificationButton email={user.email} />
      </div>
      <button type="button" className={ui.buttonGhost} onClick={dismiss} aria-label="Dismiss the verify email reminder">
        Dismiss
      </button>
    </section>
  );
}

// Storage can be unavailable (private windows, blocked site data); the banner then just shows.
function readDismissedFor(): string | null {
  try {
    return window.sessionStorage.getItem(dismissedKey);
  } catch {
    return null;
  }
}

function writeDismissedFor(userId: string) {
  try {
    window.sessionStorage.setItem(dismissedKey, userId);
  } catch {
    // Not remembered; it stays dismissed until the page reloads.
  }
}
