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
      <Link href="/" className={styles.brand}>
        NCLEX Practice
      </Link>
      <UserMenu />
    </header>
  );
}
