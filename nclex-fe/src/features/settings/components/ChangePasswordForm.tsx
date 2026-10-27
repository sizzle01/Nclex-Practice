"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { TextField } from "@/components/forms/TextField";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { applyServerErrors } from "@/features/auth/components/serverErrors";
import { passwordMinLength } from "@/features/auth/schemas";
import { changePassword } from "../api/settingsApi";
import { type ChangePasswordFormInput, type ChangePasswordFormValues, changePasswordSchema } from "../schemas";
import styles from "./settings.module.css";

const fields = ["currentPassword", "newPassword"] as const;
const emptyValues: ChangePasswordFormInput = { currentPassword: "", newPassword: "", confirmPassword: "" };

/** Change password (POST /me/password). Other devices are signed out; this one stays signed in. */
export function ChangePasswordForm() {
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors },
  } = useForm<ChangePasswordFormInput, unknown, ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: emptyValues,
  });

  const mutation = useMutation({
    mutationFn: changePassword,
    // Passwords should not linger in the form once they have done their job.
    onSuccess: () => reset(emptyValues),
    onError(error) {
      setFormError(applyServerErrors(error, setError, fields));
    },
  });

  const onSubmit = handleSubmit(({ currentPassword, newPassword }) => {
    setFormError(null);
    mutation.mutate({ currentPassword, newPassword });
  });

  return (
    <section className={`${ui.card} ${styles.section}`} aria-labelledby="password-heading">
      <h2 id="password-heading">Password</h2>
      <form className={formStyles.form} onSubmit={onSubmit} noValidate>
        {formError && (
          <p role="alert" className={formStyles.formError}>
            {formError}
          </p>
        )}

        <TextField
          id="password-current"
          label="Current password"
          type="password"
          autoComplete="current-password"
          error={errors.currentPassword?.message}
          {...register("currentPassword")}
        />
        <TextField
          id="password-new"
          label="New password"
          type="password"
          autoComplete="new-password"
          hint={`At least ${passwordMinLength} characters.`}
          error={errors.newPassword?.message}
          {...register("newPassword")}
        />
        <TextField
          id="password-confirm"
          label="Confirm new password"
          type="password"
          autoComplete="new-password"
          error={errors.confirmPassword?.message}
          {...register("confirmPassword")}
        />

        <p role="status" className={mutation.isSuccess ? formStyles.formSuccess : undefined}>
          {mutation.isSuccess ? "Your password has been changed. Your other devices have been signed out." : ""}
        </p>

        <div className={styles.actions}>
          <button type="submit" className={ui.button} disabled={mutation.isPending}>
            {mutation.isPending ? "Changing…" : "Change password"}
          </button>
        </div>
      </form>
    </section>
  );
}
