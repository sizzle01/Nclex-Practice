import type { Metadata } from "next";
import Link from "next/link";
import { AuthCard } from "@/components/app-shell/AuthCard";
import styles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";

export const metadata: Metadata = { title: "Account deleted | NCLEX Practice" };

/** Where the settings page goes after DELETE /me succeeds. */
export default function AccountDeletedPage() {
  return (
    <AuthCard>
      <section className={styles.form} aria-labelledby="deleted-heading">
        <h1 id="deleted-heading">Your account has been deleted</h1>
        <p className={styles.formSuccess}>You have been signed out on every device, and your personal data has been removed.</p>
        <Link href="/" className={`${ui.button} ${styles.submit}`}>
          Back to the home page
        </Link>
      </section>
    </AuthCard>
  );
}
