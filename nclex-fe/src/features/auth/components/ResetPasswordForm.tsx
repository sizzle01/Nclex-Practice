"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { FocusedHeading } from "@/components/forms/FocusedHeading";
import { TextField } from "@/components/forms/TextField";
import styles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { resetPassword } from "../api/authApi";
import { passwordMinLength, type ResetPasswordFormInput, type ResetPasswordFormValues, resetPasswordSchema } from "../schemas";
import { clearSession } from "../session/sessionStore";
import { applyServerErrors } from "./serverErrors";

const fields = ["newPassword"] as const;

/** The page a reset link opens (/reset-password?token=): choose a new password. */
export function ResetPasswordForm() {
  const token = useSearchParams().get("token");

  if (!token) {
    return (
      <div className={styles.form}>
        <h1>This link is incomplete</h1>
        <p className={styles.lead}>Open the link in your reset email again, or ask for a new one.</p>
        <Link href="/forgot-password" className={`${ui.button} ${styles.submit}`}>
          Get a new reset link
        </Link>
      </div>
    );
  }

  return <NewPasswordForm token={token} />;
}

function NewPasswordForm({ token }: { token: string }) {
  const [formError, setFormError] = useState<string | null>(null);
  const [linkExpired, setLinkExpired] = useState(false);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<ResetPasswordFormInput, unknown, ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: "", confirmPassword: "", signOutEverywhere: true },
  });

  const mutation = useMutation({
    mutationFn: resetPassword,
    onSuccess(_, request) {
      // Every session was revoked, including this browser's, so forget it here too.
      if (request.signOutEverywhere) clearSession();
    },
    onError(error) {
      if (isApiError(error, "INVALID_TOKEN")) {
        setLinkExpired(true);
      } else {
        setFormError(applyServerErrors(error, setError, fields));
      }
    },
  });

  const onSubmit = handleSubmit(({ newPassword, signOutEverywhere }) => {
    setFormError(null);
    setLinkExpired(false);
    mutation.mutate({ token, newPassword, signOutEverywhere });
  });

  if (mutation.isSuccess) {
    return (
      <div className={styles.form}>
        <FocusedHeading>Your password has been changed</FocusedHeading>
        <p className={styles.formSuccess}>You can now log in with your new password.</p>
        <Link href="/login" className={`${ui.button} ${styles.submit}`}>
          Log in
        </Link>
      </div>
    );
  }

  return (
    <form className={styles.form} onSubmit={onSubmit} noValidate aria-labelledby="reset-heading">
      <h1 id="reset-heading">Choose a new password</h1>

      {linkExpired && (
        <p role="alert" className={styles.formError}>
          This link is invalid or has expired. Reset links work once, for 30 minutes.{" "}
          <Link href="/forgot-password">Get a new link</Link>
        </p>
      )}
      {formError && (
        <p role="alert" className={styles.formError}>
          {formError}
        </p>
      )}

      <TextField
        label="New password"
        type="password"
        autoComplete="new-password"
        hint={`At least ${passwordMinLength} characters.`}
        error={errors.newPassword?.message}
        {...register("newPassword")}
      />
      <TextField
        label="Confirm new password"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register("confirmPassword")}
      />

      <label className={styles.checkbox}>
        <input type="checkbox" {...register("signOutEverywhere")} />
        <span>Sign out on every device (recommended if you think someone else knows your password)</span>
      </label>

      <button type="submit" className={`${ui.button} ${styles.submit}`} disabled={mutation.isPending}>
        {mutation.isPending ? "Saving…" : "Save new password"}
      </button>
    </form>
  );
}
