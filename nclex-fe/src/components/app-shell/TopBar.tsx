import Link from "next/link";
import { UserMenu } from "@/features/auth/components/UserMenu";
import { NotificationBell } from "@/features/notifications/components/NotificationBell";
import { ThemeSwitcher } from "@/features/theme/components/ThemeSwitcher";
import styles from "./TopBar.module.css";

/**
 * App shell top bar with the theme switcher, the notification bell (F12) and the user menu. The track badge
 * (interfaces.md 5.2) arrives with the feature that needs it.
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
          <span className={styles.brandName}>NCLEX Practice</span>
        </Link>
        <div className={styles.actions}>
          <ThemeSwitcher />
          <NotificationBell />
          <UserMenu />
        </div>
      </div>
    </header>
  );
}
