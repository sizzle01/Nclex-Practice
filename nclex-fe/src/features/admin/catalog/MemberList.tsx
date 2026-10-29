"use client";

import ui from "@/components/ui/ui.module.css";
import type { MemberQuestion } from "../api/adminApi";
import styles from "../components/admin.module.css";
import { typeLabel } from "../item-forms/payloads";

interface MemberListProps {
  label: string;
  members: MemberQuestion[];
  onChange(next: MemberQuestion[]): void;
  /** Sets serve items in this order, so they can be moved; banks have no order. */
  ordered: boolean;
}

/** The questions in a set or bank, with buttons to reorder and remove them (keyboard-friendly). */
export function MemberList({ label, members, onChange, ordered }: MemberListProps) {
  function move(index: number, by: -1 | 1) {
    const next = [...members];
    [next[index], next[index + by]] = [next[index + by], next[index]];
    onChange(next);
  }

  if (members.length === 0) return <p>No questions yet.</p>;

  const List = ordered ? "ol" : "ul";
  return (
    <List className={styles.memberList} aria-label={label}>
      {members.map((member, index) => (
        <li key={member.questionId}>
          <span>
            {member.stem} <span className={styles.status}>{typeLabel(member.itemType)}</span>
            {(member.status === "retired" || member.publishedVersion == null) && (
              <span className={styles.status}> Not served: {member.status === "retired" ? "retired" : "unpublished"}</span>
            )}
          </span>
          <span className={styles.actions}>
            {ordered && (
              <>
                <button type="button" className={ui.buttonGhost} aria-label={`Move up: ${member.stem}`} disabled={index === 0} onClick={() => move(index, -1)}>
                  ↑
                </button>
                <button
                  type="button"
                  className={ui.buttonGhost}
                  aria-label={`Move down: ${member.stem}`}
                  disabled={index === members.length - 1}
                  onClick={() => move(index, 1)}
                >
                  ↓
                </button>
              </>
            )}
            <button type="button" className={ui.buttonGhost} aria-label={`Remove: ${member.stem}`} onClick={() => onChange(members.filter((_, i) => i !== index))}>
              Remove
            </button>
          </span>
        </li>
      ))}
    </List>
  );
}
