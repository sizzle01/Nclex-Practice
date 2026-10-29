"use client";

import { useRef, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { ReportIssueDialog } from "./ReportIssueDialog";

interface ReportIssueButtonProps {
  questionVersionId: string;
  attemptId?: string;
  disabled?: boolean;
}

/**
 * Opens the "Report issue" dialog for one question, and gives focus back to the button when it
 * closes. Separate from "Mark for review", which is the candidate's own bookmark.
 */
export function ReportIssueButton({ questionVersionId, attemptId, disabled }: ReportIssueButtonProps) {
  const [open, setOpen] = useState(false);
  const buttonRef = useRef<HTMLButtonElement>(null);

  function close() {
    setOpen(false);
    buttonRef.current?.focus();
  }

  return (
    <>
      <button ref={buttonRef} type="button" className={ui.buttonGhost} disabled={disabled} onClick={() => setOpen(true)}>
        <span aria-hidden="true">⚠</span> Report issue
      </button>
      {open && <ReportIssueDialog questionVersionId={questionVersionId} attemptId={attemptId} onClose={close} />}
    </>
  );
}
