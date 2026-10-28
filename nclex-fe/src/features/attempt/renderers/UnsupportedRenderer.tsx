import ui from "@/components/ui/ui.module.css";

/** Shown for item types whose renderer has not been built yet (F11). */
export function UnsupportedRenderer() {
  return (
    <p role="note" className={ui.muted}>
      This question type can&apos;t be answered in the player yet. You can skip it and come back later.
    </p>
  );
}
