"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { TextField } from "@/components/forms/TextField";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { applyServerErrors } from "@/features/auth/components/serverErrors";
import { isApiError } from "@/lib/api/errors";
import { requestEmailChange } from "../api/settingsApi";
import { type ChangeEmailFormInput, type ChangeEmailFormValues, changeEmailSchema } from "../schemas";
import styles from "./settings.module.css";

const fields = ["newEmail", "currentPassword"] as const;
const emptyValues: ChangeEmailFormInput = { newEmail: "", currentPassword: "" };

/**
 * Change email (POST /me/email). Nothing changes straight away: a link goes to the new address,
 * and the account switches over when it is followed (/confirm-email-change).
 */
export function ChangeEmailForm({ currentEmail }: { currentEmail: string }) {
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors },
  } = useForm<ChangeEmailFormInput, unknown, ChangeEmailFormValues>({
    resolver: zodResolver(changeEmailSchema),
    defaultValues: emptyValues,
  });

  const mutation = useMutation({
    mutationFn: requestEmailChange,
    onSuccess: () => reset(emptyValues),
    onError(error) {
      if (isApiError(error, "EMAIL_TAKEN")) {
        setError("newEmail", { type: "server", message: "Another account already uses this email address." });
        setFormError(null);
      } else {
        setFormError(applyServerErrors(error, setError, fields));
      }
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate(values);
  });

  const sentTo = mutation.isSuccess ? mutation.variables.newEmail : null;

  return (
    <section className={`${ui.card} ${styles.section}`} aria-labelledby="email-heading">
      <h2 id="email-heading">Email address</h2>
      <p className={ui.muted}>
        You sign in with <strong>{currentEmail}</strong>.
      </p>
      <form className={formStyles.form} onSubmit={onSubmit} noValidate>
        {formError && (
          <p role="alert" className={formStyles.formError}>
            {formError}
          </p>
        )}

        <TextField
          id="email-new"
          label="New email address"
          type="email"
          autoComplete="email"
          error={errors.newEmail?.message}
          {...register("newEmail")}
        />
        <TextField
          id="email-current-password"
          label="Current password"
          type="password"
          autoComplete="current-password"
          error={errors.currentPassword?.message}
          {...register("currentPassword")}
        />

        <p role="status" className={sentTo ? formStyles.formSuccess : undefined}>
          {sentTo
            ? `Check ${sentTo} for a confirmation link. You'll keep signing in with ${currentEmail} until you follow it.`
            : ""}
        </p>

        <div className={styles.actions}>
          <button type="submit" className={ui.button} disabled={mutation.isPending}>
            {mutation.isPending ? "Sending…" : "Send confirmation link"}
          </button>
        </div>
      </form>
    </section>
  );
}
