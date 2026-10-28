import type { SaveStatus } from "./saveQueue";
import styles from "./player.module.css";

const messages: Record<SaveStatus, string> = {
  idle: "",
  saving: "Saving…",
  saved: "✓ Saved",
  // Wording from interfaces.md 5.4.
  offline: "Offline – answers are saved on this device and will sync",
  error: "Your last answer could not be saved.",
};

/** The save indicator in the bottom bar; a polite live region so changes are announced. */
export function SaveStatusText({ status }: { status: SaveStatus }) {
  const problem = status === "offline" || status === "error";
  return (
    <p role="status" className={`${styles.saveStatus} ${problem ? styles.saveStatusProblem : ""}`}>
      {messages[status]}
    </p>
  );
}
