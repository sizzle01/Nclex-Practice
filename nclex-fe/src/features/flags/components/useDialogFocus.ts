import { type KeyboardEvent, type RefObject, useEffect } from "react";

const focusable = 'a[href], button:not([disabled]), input:not([disabled]), textarea:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])';

/**
 * Keyboard behaviour of a modal dialog (interfaces.md section 4): focus moves into it when it
 * opens, Tab and Shift+Tab stay inside it, and Escape closes it. Returns the keydown handler to
 * put on the dialog element.
 */
export function useDialogFocus(dialogRef: RefObject<HTMLElement | null>, onEscape: () => void) {
  useEffect(() => {
    dialogRef.current?.querySelector<HTMLElement>(focusable)?.focus();
  }, [dialogRef]);

  return (event: KeyboardEvent<HTMLElement>) => {
    if (event.key === "Escape") {
      event.stopPropagation();
      onEscape();
      return;
    }
    if (event.key !== "Tab" || !dialogRef.current) return;

    const elements = [...dialogRef.current.querySelectorAll<HTMLElement>(focusable)];
    if (elements.length === 0) return;
    const first = elements[0];
    const last = elements[elements.length - 1];
    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  };
}
