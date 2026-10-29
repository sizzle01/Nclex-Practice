"use client";

import ui from "@/components/ui/ui.module.css";
import { toggle } from "./ChoiceForms";
import { type Header, type ItemFormProps, nextId } from "./payloads";
import styles from "./itemForms.module.css";

interface HeadersEditorProps {
  legend: string;
  noun: string;
  headers: Header[];
  prefix: string;
  disabled: boolean;
  onChange(next: Header[]): void;
}

/** The row or column headings of a matrix. */
function HeadersEditor({ legend, noun, headers, prefix, disabled, onChange }: HeadersEditorProps) {
  return (
    <fieldset className={styles.list} disabled={disabled}>
      <legend>{legend}</legend>
      <ol className={styles.rows}>
        {headers.map((header, index) => (
          <li key={header.id} className={styles.row}>
            <span className={styles.id} aria-hidden="true">
              {header.id}
            </span>
            <input
              className={styles.grow}
              aria-label={`${noun} ${header.id} text`}
              value={header.text}
              onChange={(event) => onChange(headers.map((h, i) => (i === index ? { ...h, text: event.target.value } : h)))}
            />
            <button type="button" className={ui.buttonGhost} aria-label={`Remove ${noun.toLowerCase()} ${header.id}`} onClick={() => onChange(headers.filter((_, i) => i !== index))}>
              Remove
            </button>
          </li>
        ))}
      </ol>
      <button type="button" className={ui.buttonSecondary} onClick={() => onChange([...headers, { id: nextId(headers.map((h) => h.id), prefix), text: "" }])}>
        Add {noun.toLowerCase()}
      </button>
    </fieldset>
  );
}

/** Matrix multiple choice (interfaces.md 2.4): one correct column per row. */
export function MatrixMcForm({ value, onChange, disabled }: ItemFormProps<"matrix_mc">) {
  const { content, answerKey } = value;
  const byRow = (rows: Header[], columns: Header[]) =>
    Object.fromEntries(
      rows.map((row) => [row.id, columns.some((c) => c.id === answerKey.byRow[row.id]) ? answerKey.byRow[row.id] : (columns[0]?.id ?? "")]),
    );

  return (
    <>
      <HeadersEditor legend="Rows" noun="Row" prefix="r" headers={content.rows} disabled={disabled}
        onChange={(rows) => onChange({ content: { ...content, rows }, answerKey: { byRow: byRow(rows, content.columns) } })} />
      <HeadersEditor legend="Columns" noun="Column" prefix="c" headers={content.columns} disabled={disabled}
        onChange={(columns) => onChange({ content: { ...content, columns }, answerKey: { byRow: byRow(content.rows, columns) } })} />
      <KeyGrid
        caption="Answer key: the correct column for each row"
        rows={content.rows}
        columns={content.columns}
        disabled={disabled}
        input={(row, column) => (
          <input
            type="radio"
            name={`key-${row.id}`}
            aria-label={`${label(row)}: ${label(column)}`}
            checked={answerKey.byRow[row.id] === column.id}
            onChange={() => onChange({ content, answerKey: { byRow: { ...answerKey.byRow, [row.id]: column.id } } })}
          />
        )}
      />
    </>
  );
}

/**
 * Matrix multiple response (2.5): any number of correct cells, scored per group, where a group is
 * a row or a column. Cell ids are "row:column".
 */
export function MatrixMrForm({ value, onChange, disabled }: ItemFormProps<"matrix_mr">) {
  const { content, answerKey } = value;
  const cells = Object.values(answerKey.byGroup).flat();

  // Regroups the selected cells whenever rows, columns or the group axis change.
  function keyFor(rows: Header[], columns: Header[], axis: "row" | "column", selected: string[]) {
    const groups = axis === "row" ? rows : columns;
    const valid = selected.filter((cell) => {
      const [r, c] = cell.split(":");
      return rows.some((x) => x.id === r) && columns.some((x) => x.id === c);
    });
    return {
      byGroup: Object.fromEntries(
        groups.map((g) => [g.id, valid.filter((cell) => cell.split(":")[axis === "row" ? 0 : 1] === g.id)]),
      ),
    };
  }

  return (
    <>
      <div className={styles.inline}>
        <label>
          Score per
          <select
            disabled={disabled}
            value={content.groupAxis}
            onChange={(event) => {
              const groupAxis = event.target.value as "row" | "column";
              onChange({ content: { ...content, groupAxis }, answerKey: keyFor(content.rows, content.columns, groupAxis, cells) });
            }}
          >
            <option value="row">Row</option>
            <option value="column">Column</option>
          </select>
        </label>
      </div>
      <HeadersEditor legend="Rows" noun="Row" prefix="r" headers={content.rows} disabled={disabled}
        onChange={(rows) => onChange({ content: { ...content, rows }, answerKey: keyFor(rows, content.columns, content.groupAxis, cells) })} />
      <HeadersEditor legend="Columns" noun="Column" prefix="c" headers={content.columns} disabled={disabled}
        onChange={(columns) => onChange({ content: { ...content, columns }, answerKey: keyFor(content.rows, columns, content.groupAxis, cells) })} />
      <KeyGrid
        caption="Answer key: tick every correct cell"
        rows={content.rows}
        columns={content.columns}
        disabled={disabled}
        input={(row, column) => {
          const cell = `${row.id}:${column.id}`;
          return (
            <input
              type="checkbox"
              aria-label={`${label(row)}: ${label(column)}`}
              checked={cells.includes(cell)}
              onChange={() => onChange({ content, answerKey: keyFor(content.rows, content.columns, content.groupAxis, toggle(cells, cell)) })}
            />
          );
        }}
      />
    </>
  );
}

interface KeyGridProps {
  caption: string;
  rows: Header[];
  columns: Header[];
  disabled: boolean;
  input(row: Header, column: Header): React.ReactNode;
}

/** A table of the matrix with one key control per cell, labelled by its row and column. */
function KeyGrid({ caption, rows, columns, disabled, input }: KeyGridProps) {
  return (
    <fieldset className={styles.list} disabled={disabled}>
      <legend>{caption}</legend>
      <div className={styles.gridWrap}>
        <table className={styles.grid}>
          <thead>
            <tr>
              <td />
              {columns.map((column) => (
                <th key={column.id} scope="col">
                  {label(column)}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.id}>
                <th scope="row">{label(row)}</th>
                {columns.map((column) => (
                  <td key={column.id}>{input(row, column)}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </fieldset>
  );
}

function label(header: Header) {
  return header.text || header.id;
}
