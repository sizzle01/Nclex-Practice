"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { TextField } from "@/components/forms/TextField";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import type { UserProfile } from "@/features/auth/api/authApi";
import { meQueryKey } from "@/features/auth/AuthProvider";
import { applyServerErrors } from "@/features/auth/components/serverErrors";
import { updateMe } from "../api/settingsApi";
import { type ProfileFormInput, type ProfileFormValues, profileSchema } from "../schemas";
import styles from "./settings.module.css";

const fields = ["displayName", "track", "timezone", "targetExamDate"] as const;

/** Display name, exam track, time zone and target exam date (PATCH /me). */
export function ProfileForm({ user }: { user: UserProfile }) {
  const queryClient = useQueryClient();
  const [formError, setFormError] = useState<string | null>(null);
  const timezones = useMemo(() => timezoneOptions(user.timezone), [user.timezone]);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isDirty },
    reset,
  } = useForm<ProfileFormInput, unknown, ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: valuesFrom(user),
  });

  const mutation = useMutation({
    mutationFn: updateMe,
    onSuccess(profile) {
      queryClient.setQueryData(meQueryKey, profile);
      reset(valuesFrom(profile));
    },
    onError(error) {
      setFormError(applyServerErrors(error, setError, fields));
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate({
      displayName: values.displayName,
      track: values.track,
      timezone: values.timezone,
      targetExamDate: values.targetExamDate || undefined,
    });
  });

  return (
    <section className={`${ui.card} ${styles.section}`} aria-labelledby="profile-heading">
      <h2 id="profile-heading">Profile</h2>
      <form className={formStyles.form} onSubmit={onSubmit} noValidate>
        {formError && (
          <p role="alert" className={formStyles.formError}>
            {formError}
          </p>
        )}

        <TextField
          label="Display name"
          autoComplete="nickname"
          hint="Optional. Shown on your dashboard."
          error={errors.displayName?.message}
          {...register("displayName")}
        />

        <fieldset
          className={formStyles.fieldset}
          aria-invalid={errors.track ? true : undefined}
          aria-describedby={errors.track ? "field-track-error" : undefined}
        >
          <legend>Which exam are you preparing for?</legend>
          <label className={formStyles.radio}>
            <input type="radio" value="RN" {...register("track")} /> NCLEX-RN (Registered Nurse)
          </label>
          <label className={formStyles.radio}>
            <input type="radio" value="PN" {...register("track")} /> NCLEX-PN (Practical Nurse)
          </label>
          {errors.track && (
            <p id="field-track-error" className={formStyles.error}>
              {errors.track.message}
            </p>
          )}
        </fieldset>

        <div className={formStyles.field}>
          <label htmlFor="field-timezone">Time zone</label>
          <select
            id="field-timezone"
            aria-invalid={errors.timezone ? true : undefined}
            aria-describedby={errors.timezone ? "field-timezone-error" : undefined}
            {...register("timezone")}
          >
            {timezones.map((zone) => (
              <option key={zone} value={zone}>
                {zone.replaceAll("_", " ")}
              </option>
            ))}
          </select>
          {errors.timezone && (
            <p id="field-timezone-error" className={formStyles.error}>
              {errors.timezone.message}
            </p>
          )}
        </div>

        <TextField
          label="Target exam date"
          type="date"
          hint="Optional. When you plan to sit the NCLEX."
          error={errors.targetExamDate?.message}
          {...register("targetExamDate")}
        />

        <p role="status" className={mutation.isSuccess && !isDirty ? formStyles.formSuccess : undefined}>
          {mutation.isSuccess && !isDirty ? "Your profile has been saved." : ""}
        </p>

        <div className={styles.actions}>
          <button type="submit" className={ui.button} disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : "Save profile"}
          </button>
        </div>
      </form>
    </section>
  );
}

function valuesFrom(user: UserProfile): ProfileFormInput {
  return {
    displayName: user.displayName ?? "",
    track: user.track,
    timezone: user.timezone,
    targetExamDate: user.targetExamDate ?? "",
  };
}

// IANA ids from the browser, which the API accepts. "UTC" (the default) is not always in the
// list, so the user's current value is added if missing.
function timezoneOptions(current: string): string[] {
  const zones = Intl.supportedValuesOf("timeZone");
  return zones.includes(current) ? zones : [current, ...zones];
}
