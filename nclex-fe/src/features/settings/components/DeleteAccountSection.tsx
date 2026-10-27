"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { TextField } from "@/components/forms/TextField";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { applyServerErrors } from "@/features/auth/components/serverErrors";
import { hardNavigate } from "@/lib/browser/hardNavigate";
import { deleteAccount } from "../api/settingsApi";
import { type DeleteAccountFormInput, type DeleteAccountFormValues, deleteAccountSchema } from "../schemas";
import styles from "./settings.module.css";

const fields = ["currentPassword"] as const;

/**
 * Delete account (DELETE /me), in two steps: a button that only opens the confirmation, then the
 * confirmation itself, which explains what happens and asks for the password.
 */
export function DeleteAccountSection() {
  const [confirming, setConfirming] = useState(false);
  const openButtonRef = useRef<HTMLButtonElement>(null);

  function cancel() {
    setConfirming(false);
    // The confirmation (and the focused element in it) is gone; return focus to where it started.
    requestAnimationFrame(() => openButtonRef.current?.focus());
  }

  return (
    <section className={`${ui.card} ${styles.section} ${styles.danger}`} aria-labelledby="delete-heading">
      <h2 id="delete-heading">Delete account</h2>
      {confirming ? (
        <DeleteConfirmation onCancel={cancel} />
      ) : (
        <>
          <p className={ui.muted}>Permanently delete your account and your personal data.</p>
          <div className={styles.actions}>
            <button ref={openButtonRef} type="button" className={ui.buttonSecondary} onClick={() => setConfirming(true)}>
              Delete my account…
            </button>
          </div>
        </>
      )}
    </section>
  );
}

function DeleteConfirmation({ onCancel }: { onCancel: () => void }) {
  const [formError, setFormError] = useState<string | null>(null);
  const headingRef = useRef<HTMLHeadingElement>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<DeleteAccountFormInput, unknown, DeleteAccountFormValues>({
    resolver: zodResolver(deleteAccountSchema),
    defaultValues: { currentPassword: "" },
  });

  // The confirmation replaces the button that had focus, so move focus to its heading and screen
  // readers read the warning first (interfaces.md section 4).
  useEffect(() => headingRef.current?.focus(), []);

  const mutation = useMutation({
    mutationFn: deleteAccount,
    // A full page load discards the in-memory session and every cached response. The API has
    // already ended all sessions and cleared the refresh cookie.
    onSuccess: () => hardNavigate("/account-deleted"),
    onError(error) {
      setFormError(applyServerErrors(error, setError, fields));
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate(values);
  });

  return (
    <form className={formStyles.form} onSubmit={onSubmit} noValidate aria-labelledby="delete-confirm-heading">
      <h3 id="delete-confirm-heading" ref={headingRef} tabIndex={-1}>
        Are you sure?
      </h3>
      <p>Deleting your account:</p>
      <ul>
        <li>signs you out on every device;</li>
        <li>removes your email address, name and password;</li>
        <li>keeps your past answers only anonymously, to improve question statistics.</li>
      </ul>
      <p>
        <strong>This cannot be undone.</strong> Enter your password to confirm.
      </p>

      {formError && (
        <p role="alert" className={formStyles.formError}>
          {formError}
        </p>
      )}

      <TextField
        id="delete-current-password"
        label="Current password"
        type="password"
        autoComplete="current-password"
        error={errors.currentPassword?.message}
        {...register("currentPassword")}
      />

      <div className={styles.actions}>
        <button type="submit" className={`${ui.button} ${styles.dangerButton}`} disabled={mutation.isPending}>
          {mutation.isPending ? "Deleting…" : "Delete my account permanently"}
        </button>
        <button type="button" className={ui.buttonGhost} onClick={onCancel} disabled={mutation.isPending}>
          Cancel
        </button>
      </div>
    </form>
  );
}
