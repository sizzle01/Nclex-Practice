import Link from "next/link";
import styles from "./TopBar.module.css";

/**
 * App shell top bar. Placeholder for now: the track badge, notification bell and user menu
 * (interfaces.md 5.2) arrive with the features that need them.
 */
export function TopBar() {
  return (
    <header className={styles.topBar}>
      <Link href="/" className={styles.brand}>
        NCLEX Practice
      </Link>
    </header>
  );
}
