"use client";

import { useId } from "react";
import { highlightMarkLabels } from "./highlightMarks";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import { optionMarks } from "./revealMarks";
import { toggleIn } from "./selection";
import type { ItemRendererProps } from "./types";

interface HighlightTableContent {
  columns: string[];
  rows: Array<{ id: string; cells: string[]; selectable: boolean }>;
}

interface HighlightResponse {
  selected: string[];
}

/**
 * Highlight table (interfaces.md 2.7): each selectable row has a toggle button (aria-pressed) named
 * after the row's cells. A highlighted row is shaded and its button shows a tick, so the state is
 * never shown by colour alone.
 */
export function HighlightTableRenderer({
  content,
  response,
  onChange,
  disabled,
  labelledBy,
  reveal,
}: ItemRendererProps<HighlightTableContent, HighlightResponse>) {
  const instructionsId = useId();
  const selected = response?.selected ?? [];
  const marks = optionMarks(reveal);
  const order = content.rows.filter((r) => r.selectable).map((r) => r.id);

  function toggle(rowId: string) {
    const next = toggleIn(selected, rowId, order);
    onChange(next.length > 0 ? { selected: next } : null);
  }

  return (
    <div>
      <p id={instructionsId} className={styles.instructions}>
        Highlight the rows that apply. Select a row&apos;s button again to remove the highlight.
      </p>
      <div className={styles.tableScroll}>
        <table className={styles.table} aria-labelledby={labelledBy} aria-describedby={instructionsId}>
          <thead>
            <tr>
              <th scope="col">Highlight</th>
              {content.columns.map((column) => (
                <th key={column} scope="col">
                  {column}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {content.rows.map((row) => {
              const highlighted = selected.includes(row.id);
              return (
                <tr key={row.id} className={highlighted ? styles.highlightedRow : undefined}>
                  <td>
                    {row.selectable && (
                      <>
                        <button
                          type="button"
                          className={styles.rowToggle}
                          aria-pressed={highlighted}
                          aria-label={`Highlight row: ${row.cells.join(", ")}`}
                          disabled={disabled}
                          onClick={() => toggle(row.id)}
                        >
                          {highlighted && <span aria-hidden="true">✓ </span>}
                          Highlight
                        </button>
                        <OptionMarkText
                          mark={marks[row.id]}
                          label={marks[row.id] && highlightMarkLabels[marks[row.id]]}
                        />
                      </>
                    )}
                  </td>
                  {row.cells.map((cell, index) => (
                    <td key={index}>{cell}</td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
