"use client";

import { useEffect, useId, useRef, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import styles from "./player.module.css";

interface AttemptMenuProps {
  disabled: boolean;
  onSaveAndExit: () => void;
}

/**
 * The ⋯ menu in the top bar, holding Save &amp; Exit (interfaces.md 5.3). A menu button: Enter or
 * Space opens it with focus on the first item, and Escape closes it and returns focus.
 */
export function AttemptMenu({ disabled, onSaveAndExit }: AttemptMenuProps) {
  const [open, setOpen] = useState(false);
  const menuId = useId();
  const buttonRef = useRef<HTMLButtonElement>(null);
  const firstItemRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (open) firstItemRef.current?.focus();
  }, [open]);

  function close() {
    setOpen(false);
    buttonRef.current?.focus();
  }

  return (
    <div className={styles.menu} onKeyDown={(event) => event.key === "Escape" && open && close()}>
      <button
        ref={buttonRef}
        type="button"
        className={ui.buttonGhost}
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={menuId}
        aria-label="More options"
        disabled={disabled}
        onClick={() => setOpen((value) => !value)}
      >
        ⋯
      </button>
      {open && (
        <div id={menuId} role="menu" className={`${ui.card} ${styles.menuList}`}>
          <button
            ref={firstItemRef}
            type="button"
            role="menuitem"
            className={ui.buttonGhost}
            onClick={() => {
              setOpen(false);
              onSaveAndExit();
            }}
          >
            Save &amp; Exit
          </button>
        </div>
      )}
    </div>
  );
}
