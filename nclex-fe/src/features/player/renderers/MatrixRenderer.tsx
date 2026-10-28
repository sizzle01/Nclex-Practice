"use client";

import { type KeyboardEvent, useId } from "react";
import { narrowScreenQuery, useMediaQuery } from "@/lib/browser/useMediaQuery";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import { choiceMarks, type OptionMark } from "./revealMarks";
import { compactGroups, toggleIn } from "./selection";
import type { ItemRendererProps } from "./types";

interface Header {
  id: string;
  text: string;
}

interface MatrixContent {
  rows: Header[];
  columns: Header[];
  /** Matrix multiple response only: whether each row or each column is scored as a group. */
  groupAxis?: "row" | "column";
}

interface MatrixMcResponse {
  byRow: Record<string, string | null>;
}

interface GroupedResponse {
  byGroup: Record<string, string[]>;
}

/** Matrix multiple choice (interfaces.md 2.4): one radio button per cell, one answer per row. */
export function MatrixMcRenderer(props: ItemRendererProps<MatrixContent, MatrixMcResponse>) {
  const { response, onChange } = props;
  const byRow = response?.byRow ?? {};
  const selected = new Set(Object.entries(byRow).flatMap(([row, column]) => (column ? [cellId(row, column)] : [])));

  function select(rowId: string, columnId: string) {
    const answered = compactGroups({ ...byRow, [rowId]: columnId }, () => false);
    onChange(answered ? { byRow: answered } : null);
  }

  return (
    <MatrixGrid
      {...props}
      multiple={false}
      selected={selected}
      onSelect={select}
      instructions="Choose one option in each row."
      marks={choiceMarks("matrix_mc", props.reveal)}
    />
  );
}

/**
 * Matrix multiple response (interfaces.md 2.5): a checkbox per cell. The response groups the ticked
 * cells ("row:column") by row or by column, following the content's groupAxis.
 */
export function MatrixMrRenderer(props: ItemRendererProps<MatrixContent, GroupedResponse>) {
  const { content, response, onChange } = props;
  const byGroup = response?.byGroup ?? {};
  const selected = new Set(Object.values(byGroup).flat());
  const byColumn = content.groupAxis === "column";

  function select(rowId: string, columnId: string) {
    const groupId = byColumn ? columnId : rowId;
    const groupCells = byColumn
      ? content.rows.map((row) => cellId(row.id, columnId))
      : content.columns.map((column) => cellId(rowId, column.id));
    const next = { ...byGroup, [groupId]: toggleIn(byGroup[groupId] ?? [], cellId(rowId, columnId), groupCells) };
    const answered = compactGroups(next, (cells) => cells.length === 0);
    onChange(answered ? { byGroup: answered } : null);
  }

  return (
    <MatrixGrid
      {...props}
      multiple
      selected={selected}
      onSelect={select}
      instructions={`Select all that apply in each ${byColumn ? "column" : "row"}.`}
      marks={choiceMarks("matrix_mr", props.reveal)}
    />
  );
}

interface MatrixGridProps {
  content: MatrixContent;
  disabled: boolean;
  labelledBy: string;
  multiple: boolean;
  selected: Set<string>;
  onSelect(rowId: string, columnId: string): void;
  instructions: string;
  marks: Record<string, OptionMark>;
}

/**
 * The grid both matrix types share. On wide screens it is a table with row and column headers, and
 * the arrow keys move between cells while Space selects (interfaces.md section 4). On narrow
 * screens it stacks into one card per row, so nothing scrolls sideways.
 */
function MatrixGrid({
  content,
  disabled,
  labelledBy,
  multiple,
  selected,
  onSelect,
  instructions,
  marks,
}: MatrixGridProps) {
  const instructionsId = useId();
  const stacked = useMediaQuery(narrowScreenQuery);
  const type = multiple ? "checkbox" : "radio";
  const nameFor = (rowId: string) => `${labelledBy}-${rowId}`;

  if (stacked) {
    return (
      <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId} className={styles.groups}>
        <p id={instructionsId} className={styles.instructions}>
          {instructions}
        </p>
        {content.rows.map((row) => (
          <fieldset key={row.id} className={styles.fieldset}>
            <legend className={styles.legend}>{row.text}</legend>
            <div className={styles.options}>
              {content.columns.map((column) => {
                const id = cellId(row.id, column.id);
                return (
                  <div key={column.id} className={styles.optionRow}>
                    <label className={styles.option}>
                      <input
                        type={type}
                        name={nameFor(row.id)}
                        checked={selected.has(id)}
                        disabled={disabled}
                        onChange={() => onSelect(row.id, column.id)}
                      />
                      <span>{column.text}</span>
                      <OptionMarkText mark={marks[id]} />
                    </label>
                  </div>
                );
              })}
            </div>
          </fieldset>
        ))}
      </div>
    );
  }

  return (
    <div>
      <p id={instructionsId} className={styles.instructions}>
        {instructions} Use the arrow keys to move between cells and Space to select.
      </p>
      <div className={styles.tableScroll}>
        <table
          className={styles.table}
          aria-labelledby={labelledBy}
          aria-describedby={instructionsId}
          onKeyDown={(event) => moveBetweenCells(event, content.rows.length, content.columns.length)}
        >
          <thead>
            <tr>
              <td />
              {content.columns.map((column) => (
                <th key={column.id} scope="col">
                  {column.text}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {content.rows.map((row, r) => (
              <tr key={row.id}>
                <th scope="row">{row.text}</th>
                {content.columns.map((column, c) => {
                  const id = cellId(row.id, column.id);
                  return (
                    <td key={column.id} className={styles.cell}>
                      <label className={styles.cellLabel}>
                        <input
                          type={type}
                          name={nameFor(row.id)}
                          aria-label={`${row.text}: ${column.text}`}
                          data-cell={`${r},${c}`}
                          checked={selected.has(id)}
                          disabled={disabled}
                          onChange={() => onSelect(row.id, column.id)}
                        />
                        <OptionMarkText mark={marks[id]} />
                      </label>
                    </td>
                  );
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

/** A matrix cell id, as the key and response use it (interfaces.md 2.5). */
function cellId(rowId: string, columnId: string) {
  return `${rowId}:${columnId}`;
}

const arrowSteps: Record<string, [number, number]> = {
  ArrowUp: [-1, 0],
  ArrowDown: [1, 0],
  ArrowLeft: [0, -1],
  ArrowRight: [0, 1],
};

/**
 * Arrow keys move focus to the neighbouring cell without selecting it. Native radio buttons would
 * otherwise select as they move, and only along their own row; here Space is always the key that
 * selects, the same for radio and checkbox matrices.
 */
function moveBetweenCells(event: KeyboardEvent<HTMLTableElement>, rows: number, columns: number) {
  const step = arrowSteps[event.key];
  const position = (event.target as HTMLElement).dataset.cell;
  if (!step || !position) return;
  event.preventDefault();
  const [r, c] = position.split(",").map(Number);
  const nextRow = Math.min(rows - 1, Math.max(0, r + step[0]));
  const nextColumn = Math.min(columns - 1, Math.max(0, c + step[1]));
  event.currentTarget.querySelector<HTMLInputElement>(`[data-cell="${nextRow},${nextColumn}"]`)?.focus();
}
