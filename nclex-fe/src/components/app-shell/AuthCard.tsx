import type { ReactNode } from "react";
import styles from "./AuthCard.module.css";

/** Centred card that frames the log-in and sign-up forms. */
export function AuthCard({ children }: { children: ReactNode }) {
  return (
    <div className={styles.wrapper}>
      <div className={styles.card}>{children}</div>
    </div>
  );
}
