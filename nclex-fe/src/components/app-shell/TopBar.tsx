import Link from "next/link";
import { UserMenu } from "@/features/auth/components/UserMenu";
import styles from "./TopBar.module.css";

/**
 * App shell top bar with the user menu. The track badge and notification bell (interfaces.md 5.2)
 * arrive with the features that need them.
 */
export function TopBar() {
  return (
    <header className={styles.topBar}>
      <div className={styles.inner}>
        <Link href="/" className={styles.brand}>
          <span className={styles.logo} aria-hidden="true">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none">
              <path d="M10 3h4v7h7v4h-7v7h-4v-7H3v-4h7z" fill="currentColor" />
            </svg>
          </span>
          NCLEX Practice
        </Link>
        <UserMenu />
      </div>
    </header>
  );
}
