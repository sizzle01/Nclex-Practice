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
import { signup } from "../api/authApi";
import { useAuth } from "../AuthProvider";
import { type SignupFormInput, type SignupFormValues, passwordMinLength, signupSchema } from "../schemas";
import { nextPathFrom } from "./nextPath";
import { applyServerErrors } from "./serverErrors";

const fields = ["email", "password", "track", "displayName"] as const;

/** Sign-up form: email, password, exam track (RN/PN) and optional display name. */
export function SignupForm() {
  const { signIn } = useAuth();
  const router = useRouter();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<SignupFormInput, unknown, SignupFormValues>({
    resolver: zodResolver(signupSchema),
    defaultValues: { email: "", password: "", displayName: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: SignupFormValues) =>
      signup({
        email: values.email,
        password: values.password,
        track: values.track,
        displayName: values.displayName || null,
      }),
    onSuccess(response) {
      signIn(response);
      router.replace(nextPathFrom(window.location.search));
    },
    onError(error) {
      if (isApiError(error, "EMAIL_TAKEN")) {
        setError("email", { type: "server", message: "An account with this email already exists." }, { shouldFocus: true });
        return;
      }
      setFormError(applyServerErrors(error, setError, fields));
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate(values);
  });

  return (
    <form className={styles.form} onSubmit={onSubmit} noValidate aria-labelledby="signup-heading">
      <h1 id="signup-heading">Create your account</h1>

      {formError && (
        <p role="alert" className={styles.formError}>
          {formError}
        </p>
      )}

      <TextField label="Email" type="email" autoComplete="email" error={errors.email?.message} {...register("email")} />

      <TextField
        label="Password"
        type="password"
        autoComplete="new-password"
        hint={`At least ${passwordMinLength} characters.`}
        error={errors.password?.message}
        {...register("password")}
      />

      <fieldset
        className={styles.fieldset}
        aria-invalid={errors.track ? true : undefined}
        aria-describedby={errors.track ? "field-track-error" : undefined}
      >
        <legend>Which exam are you preparing for?</legend>
        <label className={styles.radio}>
          <input type="radio" value="RN" {...register("track")} /> NCLEX-RN (Registered Nurse)
        </label>
        <label className={styles.radio}>
          <input type="radio" value="PN" {...register("track")} /> NCLEX-PN (Practical Nurse)
        </label>
        {errors.track && (
          <p id="field-track-error" className={styles.error}>
            {errors.track.message}
          </p>
        )}
      </fieldset>

      <TextField
        label="Display name (optional)"
        autoComplete="nickname"
        error={errors.displayName?.message}
        {...register("displayName")}
      />

      <button type="submit" className={styles.submit} disabled={mutation.isPending}>
        {mutation.isPending ? "Creating account…" : "Create account"}
      </button>

      <p>
        Already have an account? <Link href="/login">Log in</Link>
      </p>
    </form>
  );
}
