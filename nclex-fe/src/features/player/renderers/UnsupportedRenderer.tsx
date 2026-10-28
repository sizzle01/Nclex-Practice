import ui from "@/components/ui/ui.module.css";

/** Shown for phase 2 item types, whose renderers come in their own feature. */
export function UnsupportedRenderer() {
  return (
    <p role="note" className={ui.muted}>
      This question type can&apos;t be answered in the player yet. You can skip it and come back later.
    </p>
  );
}
