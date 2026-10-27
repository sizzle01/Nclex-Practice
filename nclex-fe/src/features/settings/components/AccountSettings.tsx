"use client";

import { useMe } from "@/features/auth/AuthProvider";
import { ChangeEmailForm } from "./ChangeEmailForm";
import { ChangePasswordForm } from "./ChangePasswordForm";
import { DeleteAccountSection } from "./DeleteAccountSection";
import { ProfileForm } from "./ProfileForm";
import styles from "./settings.module.css";

/** /settings/account (interfaces.md section 5.2): profile, track, email, password and deletion. */
export function AccountSettings() {
  const { data: me, isError } = useMe();

  return (
    <div className={styles.page}>
      <h1>Account settings</h1>
      {me ? (
        <>
          <ProfileForm user={me} />
          <ChangeEmailForm currentEmail={me.email} />
          <ChangePasswordForm />
          <DeleteAccountSection />
        </>
      ) : (
        <p role="status">{isError ? "We couldn't load your account. Please refresh the page." : "Loading your account…"}</p>
      )}
    </div>
  );
}
