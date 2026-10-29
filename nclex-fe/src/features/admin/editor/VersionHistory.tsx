"use client";

import { toNumber } from "@/lib/format/toNumber";
import type { AdminVersionView } from "../api/adminApi";
import { statusLabel } from "../statusLabels";
import styles from "./editor.module.css";

interface VersionHistoryProps {
  versions: AdminVersionView[];
  selectedId: string;
  onSelect(versionId: string): void;
}

/** Every version, newest first, with who wrote and reviewed it; pick one to view or work on. */
export function VersionHistory({ versions, selectedId, onSelect }: VersionHistoryProps) {
  return (
    <section aria-labelledby="history-heading">
      <h2 id="history-heading" className={styles.panelTitle}>
        Version history
      </h2>
      <ol className={styles.history}>
        {versions.map((version) => (
          <li key={version.id} className={styles.historyItem} aria-current={version.id === selectedId ? "true" : undefined}>
            <button type="button" onClick={() => onSelect(version.id)} aria-pressed={version.id === selectedId}>
              Version {toNumber(version.version)}
            </button>
            <span>{statusLabel(version.status)}</span>
            <span>by {version.authorName}</span>
            {version.reviewerName && <span>· reviewed by {version.reviewerName}</span>}
            {version.reviewNotes && <span>· “{version.reviewNotes}”</span>}
            <span>· {new Date(version.createdAt).toLocaleDateString()}</span>
          </li>
        ))}
      </ol>
    </section>
  );
}
