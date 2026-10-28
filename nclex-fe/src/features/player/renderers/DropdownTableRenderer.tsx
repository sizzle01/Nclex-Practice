"use client";

import { type Blank, BlankSelect, withBlank } from "./BlankSelect";
import styles from "./renderers.module.css";
import type { ItemRendererProps } from "./types";

type Cell = { text: string; blankId?: null } | { blankId: string; text?: null };

interface DropdownTableContent {
  columns: string[];
  rows: Array<{ id: string; cells: Cell[] }>;
  blanks: Blank[];
}

interface DropdownResponse {
  byBlank: Record<string, string | null>;
}

/**
 * Drop-down table (interfaces.md 2.6): a table whose cells are text or a select. The first cell of
 * each row is its header, and each select is named by its column and row, e.g. "Interpretation for
 * Potassium".
 */
export function DropdownTableRenderer({
  content,
  response,
  onChange,
  disabled,
  labelledBy,
  reveal,
}: ItemRendererProps<DropdownTableContent, DropdownResponse>) {
  const byBlank = response?.byBlank ?? {};
  const blanks = new Map(content.blanks.map((blank) => [blank.id, blank]));

  return (
    <div>
      <p className={styles.instructions}>Choose an option for each blank in the table.</p>
      <div className={styles.tableScroll}>
        <table className={styles.table} aria-labelledby={labelledBy}>
          <thead>
            <tr>
              {content.columns.map((column) => (
                <th key={column} scope="col">
                  {column}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {content.rows.map((row) => {
              const rowName = row.cells.find((cell) => cell.text)?.text ?? row.id;
              return (
                <tr key={row.id}>
                  {row.cells.map((cell, index) => {
                    const blank = cell.blankId ? blanks.get(cell.blankId) : undefined;
                    if (!blank) {
                      return index === 0 ? (
                        <th key={index} scope="row">
                          {cell.text}
                        </th>
                      ) : (
                        <td key={index}>{cell.text}</td>
                      );
                    }
                    return (
                      <td key={index}>
                        <BlankSelect
                          blank={blank}
                          value={byBlank[blank.id] ?? null}
                          label={`${content.columns[index] ?? "Blank"} for ${rowName}`}
                          disabled={disabled}
                          onChange={(optionId) => onChange(withBlank(byBlank, blank.id, optionId))}
                          reveal={reveal}
                        />
                      </td>
                    );
                  })}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
