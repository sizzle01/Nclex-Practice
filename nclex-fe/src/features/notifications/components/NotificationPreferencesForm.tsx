"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { usePreferences, useReplacePreferences } from "../api/notificationQueries";
import type { NotificationCategory, NotificationPreferences } from "../api/notificationsApi";
import { type PreferencesFormValues, preferencesSchema } from "../schemas";
import styles from "./notifications.module.css";

const copy: Record<NotificationCategory, { label: string; hint: string }> = {
  results: { label: "Results", hint: "When a test you submitted has been graded." },
  reminders: { label: "Reminders", hint: "Tests you started but haven't finished for a few days." },
  engagement: { label: "Streaks and goals", hint: "Progress towards your study goals." },
  content: { label: "Your reports", hint: "When we've reviewed a question you reported." },
  security: {
    label: "Security",
    hint: "Password and email changes, new sign-ins and lockouts. These always go to the app and to your email.",
  },
};

/** /settings/notifications (interfaces.md 5.2): in-app and email toggles per category. */
export function NotificationPreferencesForm() {
  const preferences = usePreferences();

  return (
    <div className={styles.page}>
      <div className={styles.pageHeader}>
        <h1>Notification settings</h1>
        <Link href="/settings/account" className={ui.buttonGhost}>
          Account settings
        </Link>
      </div>
      {preferences.data ? (
        <PreferencesForm saved={preferences.data} />
      ) : (
        <p role={preferences.isError ? "alert" : "status"}>
          {preferences.isError ? "We couldn't load your settings. Please refresh the page." : "Loading your settings…"}
        </p>
      )}
    </div>
  );
}

function PreferencesForm({ saved }: { saved: NotificationPreferences }) {
  const replace = useReplacePreferences();
  const [formError, setFormError] = useState<string | null>(null);
  const { register, handleSubmit, reset, formState } = useForm<PreferencesFormValues>({
    resolver: zodResolver(preferencesSchema),
    defaultValues: saved,
  });

  useEffect(() => reset(saved), [saved, reset]);

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null);
    try {
      await replace.mutateAsync({
        // Locked rows are sent as on whatever the form holds; the API refuses anything else.
        preferences: values.preferences.map(({ category, inApp, email, locked }) =>
          locked ? { category, inApp: true, email: true } : { category, inApp, email },
        ),
      });
    } catch {
      setFormError("We couldn't save your settings. Please try again.");
    }
  });

  return (
    <form className={`${ui.card} ${formStyles.form}`} onSubmit={onSubmit} noValidate>
      {formError && (
        <p role="alert" className={formStyles.formError}>
          {formError}
        </p>
      )}
      {saved.preferences.map((preference, index) => {
        const { label, hint } = copy[preference.category];
        const hintId = `notification-hint-${preference.category}`;
        return (
          <fieldset key={preference.category} className={styles.preference} aria-describedby={hintId}>
            <legend>{label}</legend>
            <p id={hintId} className={ui.muted}>
              {hint}
            </p>
            <div className={styles.channels}>
              <label className={formStyles.checkbox}>
                <input type="checkbox" disabled={preference.locked} {...register(`preferences.${index}.inApp`)} />
                In the app
              </label>
              <label className={formStyles.checkbox}>
                <input type="checkbox" disabled={preference.locked} {...register(`preferences.${index}.email`)} />
                By email
              </label>
            </div>
          </fieldset>
        );
      })}
      <div className={styles.formActions}>
        <button type="submit" className={ui.button} disabled={replace.isPending || !formState.isDirty}>
          {replace.isPending ? "Saving…" : "Save settings"}
        </button>
        <p role="status" className={replace.isSuccess && !formState.isDirty ? formStyles.formSuccess : undefined}>
          {replace.isSuccess && !formState.isDirty ? "Your notification settings have been saved." : ""}
        </p>
      </div>
    </form>
  );
}
