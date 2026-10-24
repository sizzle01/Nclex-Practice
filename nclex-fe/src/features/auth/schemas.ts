import { z } from "zod";

// The same rules as the API (api.md section 2, architecture.md section 6), checked in the browser
// first so users get instant feedback. The API checks them again; it is the authority.
export const emailMaxLength = 254;
export const passwordMinLength = 10;
export const passwordMaxLength = 128;
export const displayNameMaxLength = 80;

const email = z
  .string()
  .trim()
  .min(1, "Enter your email address.")
  .max(emailMaxLength, `Email must be at most ${emailMaxLength} characters.`)
  .pipe(z.email("Enter a valid email address, like name@example.com."));

export const signupSchema = z.object({
  email,
  password: z
    .string()
    .min(passwordMinLength, `Use at least ${passwordMinLength} characters.`)
    .max(passwordMaxLength, `Use at most ${passwordMaxLength} characters.`),
  track: z.enum(["RN", "PN"], { error: "Choose the exam you are preparing for." }),
  displayName: z
    .string()
    .trim()
    .max(displayNameMaxLength, `Display name must be at most ${displayNameMaxLength} characters.`),
});

export const loginSchema = z.object({
  email,
  password: z.string().min(1, "Enter your password."),
});

export type SignupFormInput = z.input<typeof signupSchema>;
export type SignupFormValues = z.output<typeof signupSchema>;
export type LoginFormInput = z.input<typeof loginSchema>;
export type LoginFormValues = z.output<typeof loginSchema>;
