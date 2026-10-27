"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { FocusedHeading } from "@/components/forms/FocusedHeading";
import { TextField } from "@/components/forms/TextField";
import styles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { forgotPassword } from "../api/authApi";
import { type ForgotPasswordFormInput, type ForgotPasswordFormValues, forgotPasswordSchema } from "../schemas";
import { applyServerErrors } from "./serverErrors";

const fields = ["email"] as const;

/**
 * Asks for a password-reset link. The confirmation reads the same whether or not the email has an
 * account, matching the API, which never reveals that (architecture.md section 6).
 */
export function ForgotPasswordForm() {
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    getValues,
    formState: { errors },
  } = useForm<ForgotPasswordFormInput, unknown, ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" },
  });

  const mutation = useMutation({
    mutationFn: forgotPassword,
    onError(error) {
      setFormError(applyServerErrors(error, setError, fields));
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate(values.email);
  });

  if (mutation.isSuccess) {
    return (
      <div className={styles.form}>
        <FocusedHeading>Check your email</FocusedHeading>
        <p className={styles.formSuccess}>
          If an account exists for {getValues("email").trim()}, we&apos;ve sent it a link to reset your password. The
          link expires in 30 minutes.
        </p>
        <p className={styles.footer}>
          <Link href="/login">Back to log in</Link>
        </p>
      </div>
    );
  }

  return (
    <form className={styles.form} onSubmit={onSubmit} noValidate aria-labelledby="forgot-heading">
      <h1 id="forgot-heading">Forgot your password?</h1>
      <p className={styles.lead}>Enter your account&apos;s email and we&apos;ll send you a link to choose a new one.</p>

      {formError && (
        <p role="alert" className={styles.formError}>
          {formError}
        </p>
      )}

      <TextField label="Email" type="email" autoComplete="email" error={errors.email?.message} {...register("email")} />

      <button type="submit" className={`${ui.button} ${styles.submit}`} disabled={mutation.isPending}>
        {mutation.isPending ? "Sending…" : "Send reset link"}
      </button>

      <p className={styles.footer}>
        Remembered it? <Link href="/login">Log in</Link>
      </p>
    </form>
  );
}
