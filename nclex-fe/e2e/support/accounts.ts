import { expect, type Page } from "@playwright/test";
import { linkInEmail } from "./mailpit";

export const password = "correct-horse-battery";

/** A fresh address per test, so tests never share an account or an inbox. */
export function newEmail(): string {
  return `e2e-${Date.now()}-${Math.random().toString(36).slice(2, 8)}@example.test`;
}

/** Signs up through the form; the app signs the new account in. */
export async function signUp(page: Page, email = newEmail()): Promise<string> {
  await page.goto("/signup");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password").fill(password);
  await page.getByLabel(/NCLEX-RN/).check();
  await page.getByRole("button", { name: "Create account" }).click();
  await expect(page).toHaveURL(/\/dashboard/);
  return email;
}

/** Opens the verification link from the email, as a candidate would. */
export async function verifyEmail(page: Page, email: string): Promise<void> {
  await page.goto(await linkInEmail(email, "/verify-email"));
  await expect(page.getByRole("heading", { name: "Your email is verified" })).toBeVisible();
}

export async function signUpVerified(page: Page): Promise<string> {
  const email = await signUp(page);
  await verifyEmail(page, email);
  return email;
}

export async function logIn(page: Page, email: string): Promise<void> {
  await page.goto("/login");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password").fill(password);
  await page.getByRole("button", { name: "Log in" }).click();
  await expect(page).toHaveURL(/\/dashboard/);
}
