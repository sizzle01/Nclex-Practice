"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { TextField } from "@/components/forms/TextField";
import styles from "@/components/forms/forms.module.css";
import { isApiError } from "@/lib/api/errors";
import { login } from "../api/authApi";
import { useAuth } from "../AuthProvider";
import { type LoginFormInput, type LoginFormValues, loginSchema } from "../schemas";
import { nextPathFrom } from "./nextPath";
import { applyServerErrors } from "./serverErrors";

const fields = ["email", "password"] as const;

/** Login form. On success, goes back to the page that required sign-in (?next=) or the dashboard. */
export function LoginForm() {
  const { signIn } = useAuth();
  const router = useRouter();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormInput, unknown, LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess(response) {
      signIn(response);
      router.replace(nextPathFrom(window.location.search));
    },
    onError(error) {
      setFormError(
        isApiError(error, "INVALID_CREDENTIALS")
          ? "The email or password is incorrect."
          : applyServerErrors(error, setError, fields),
      );
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate(values);
  });

  return (
    <form className={styles.form} onSubmit={onSubmit} noValidate aria-labelledby="login-heading">
      <h1 id="login-heading">Log in</h1>

      {formError && (
        <p role="alert" className={styles.formError}>
          {formError}
        </p>
      )}

      <TextField label="Email" type="email" autoComplete="email" error={errors.email?.message} {...register("email")} />
      <TextField
        label="Password"
        type="password"
        autoComplete="current-password"
        error={errors.password?.message}
        {...register("password")}
      />

      <button type="submit" className={styles.submit} disabled={mutation.isPending}>
        {mutation.isPending ? "Logging in…" : "Log in"}
      </button>

      <p>
        New here? <Link href="/signup">Create an account</Link>
      </p>
    </form>
  );
}
