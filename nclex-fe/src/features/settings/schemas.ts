import { z } from "zod";
import { displayNameMaxLength, emailMaxLength, passwordMaxLength, passwordMinLength } from "@/features/auth/schemas";

// The same rules as the API (api.md section 2), checked in the browser first for instant feedback.

const currentPassword = z.string().min(1, "Enter your current password.");

export const profileSchema = z.object({
  displayName: z
    .string()
    .trim()
    .max(displayNameMaxLength, `Display name must be at most ${displayNameMaxLength} characters.`),
  track: z.enum(["RN", "PN"], { error: "Choose the exam you are preparing for." }),
  timezone: z.string().min(1, "Choose your time zone."),
  // "" from an empty date input; the API cannot clear the date yet, so empty means "leave it".
  targetExamDate: z.string(),
});

export const changePasswordSchema = z
  .object({
    currentPassword,
    newPassword: z
      .string()
      .min(passwordMinLength, `Use at least ${passwordMinLength} characters.`)
      .max(passwordMaxLength, `Use at most ${passwordMaxLength} characters.`),
    confirmPassword: z.string(),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    message: "The passwords don't match.",
    path: ["confirmPassword"],
  });

export const changeEmailSchema = z.object({
  newEmail: z
    .string()
    .trim()
    .min(1, "Enter your new email address.")
    .max(emailMaxLength, `Email must be at most ${emailMaxLength} characters.`)
    .pipe(z.email("Enter a valid email address, like name@example.com.")),
  currentPassword,
});

export const deleteAccountSchema = z.object({ currentPassword });

export type ProfileFormInput = z.input<typeof profileSchema>;
export type ProfileFormValues = z.output<typeof profileSchema>;
export type ChangePasswordFormInput = z.input<typeof changePasswordSchema>;
export type ChangePasswordFormValues = z.output<typeof changePasswordSchema>;
export type ChangeEmailFormInput = z.input<typeof changeEmailSchema>;
export type ChangeEmailFormValues = z.output<typeof changeEmailSchema>;
export type DeleteAccountFormInput = z.input<typeof deleteAccountSchema>;
export type DeleteAccountFormValues = z.output<typeof deleteAccountSchema>;
