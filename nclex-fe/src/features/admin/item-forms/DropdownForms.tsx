"use client";

import ui from "@/components/ui/ui.module.css";
import { OptionListEditor } from "./OptionListEditor";
import { type Blank, type DropdownCell, type ItemFormProps, nextId } from "./payloads";
import styles from "./itemForms.module.css";

interface BlanksEditorProps {
  blanks: Blank[];
  byBlank: Record<string, string>;
  disabled: boolean;
  onChange(blanks: Blank[], byBlank: Record<string, string>): void;
}

/** Each blank's choices and its correct choice. Rationale keys are "blank:option" (2.6). */
function BlanksEditor({ blanks, byBlank, disabled, onChange }: BlanksEditorProps) {
  return (
    <>
      {blanks.map((blank, index) => (
        <OptionListEditor
          key={blank.id}
          legend={`Blank ${blank.id} choices`}
          options={blank.options}
          disabled={disabled}
          onChange={(options) => {
            const next = blanks.map((b, i) => (i === index ? { ...b, options } : b));
            const correct = options.some((o) => o.id === byBlank[blank.id]) ? byBlank[blank.id] : (options[0]?.id ?? "");
            onChange(next, { ...byBlank, [blank.id]: correct });
          }}
          renderMark={(option) => (
            <label className={styles.mark}>
              <input
                type="radio"
                name={`blank-${blank.id}-correct`}
                checked={byBlank[blank.id] === option.id}
                onChange={() => onChange(blanks, { ...byBlank, [blank.id]: option.id })}
              />
              Correct
            </label>
          )}
        />
      ))}
    </>
  );
}

function newBlank(blanks: Blank[]): Blank {
  return { id: nextId(blanks.map((b) => b.id), "b"), options: [{ id: "a", text: "" }, { id: "b", text: "" }, { id: "c", text: "" }] };
}

/** Drop-down cloze (2.6): a sentence with {{b1}}-style placeholders, one per blank. */
export function DropdownClozeForm({ value, onChange, disabled }: ItemFormProps<"dropdown_cloze">) {
  const { content, answerKey } = value;

  function addBlank() {
    const blank = newBlank(content.blanks);
    onChange({
      content: { template: `${content.template} {{${blank.id}}}`.trim(), blanks: [...content.blanks, blank] },
      answerKey: { byBlank: { ...answerKey.byBlank, [blank.id]: "a" } },
    });
  }

  function removeBlank(id: string) {
    const byBlank = Object.fromEntries(Object.entries(answerKey.byBlank).filter(([blankId]) => blankId !== id));
    onChange({
      content: { template: content.template.replace(`{{${id}}}`, "").replace(/\s{2,}/g, " "), blanks: content.blanks.filter((b) => b.id !== id) },
      answerKey: { byBlank },
    });
  }

  return (
    <>
      <label className={styles.list}>
        Sentence, with a placeholder such as {"{{b1}}"} for each blank
        <textarea
          rows={3}
          disabled={disabled}
          value={content.template}
          onChange={(event) => onChange({ content: { ...content, template: event.target.value }, answerKey })}
        />
      </label>
      <BlanksEditor
        blanks={content.blanks}
        byBlank={answerKey.byBlank}
        disabled={disabled}
        onChange={(blanks, byBlank) => onChange({ content: { ...content, blanks }, answerKey: { byBlank } })}
      />
      <div className={styles.row}>
        <button type="button" className={ui.buttonSecondary} disabled={disabled} onClick={addBlank}>
          Add blank
        </button>
        {content.blanks.map((blank) => (
          <button key={blank.id} type="button" className={ui.buttonGhost} disabled={disabled} onClick={() => removeBlank(blank.id)}>
            Remove blank {blank.id}
          </button>
        ))}
      </div>
    </>
  );
}

/** Drop-down table (2.6): a table whose cells are text or a blank. */
export function DropdownTableForm({ value, onChange, disabled }: ItemFormProps<"dropdown_table">) {
  const { content, answerKey } = value;

  function setCell(rowIndex: number, cellIndex: number, cell: DropdownCell) {
    const rows = content.rows.map((row, r) =>
      r === rowIndex ? { ...row, cells: row.cells.map((c, i) => (i === cellIndex ? cell : c)) } : row,
    );
    sync(content.columns, rows, content.blanks);
  }

  // Keeps exactly one blank per blank cell, and the key in step with the blanks.
  function sync(columns: string[], rows: typeof content.rows, blanks: Blank[]) {
    const used = rows.flatMap((row) => row.cells.flatMap((cell) => ("blankId" in cell ? [cell.blankId] : [])));
    const kept = blanks.filter((b) => used.includes(b.id));
    const byBlank = Object.fromEntries(kept.map((b) => [b.id, answerKey.byBlank[b.id] ?? b.options[0]?.id ?? ""]));
    onChange({ content: { columns, rows, blanks: kept }, answerKey: { byBlank } });
  }

  function makeBlank(rowIndex: number, cellIndex: number) {
    const blank = newBlank(content.blanks);
    const rows = content.rows.map((row, r) =>
      r === rowIndex ? { ...row, cells: row.cells.map((c, i) => (i === cellIndex ? { blankId: blank.id } : c)) } : row,
    );
    const blanks = [...content.blanks, blank];
    onChange({ content: { ...content, rows, blanks }, answerKey: { byBlank: { ...answerKey.byBlank, [blank.id]: "a" } } });
  }

  return (
    <>
      <fieldset className={styles.list} disabled={disabled}>
        <legend>Table</legend>
        <div className={styles.gridWrap}>
          <table className={styles.grid}>
            <thead>
              <tr>
                {content.columns.map((column, c) => (
                  <th key={c} scope="col">
                    <input
                      aria-label={`Column ${c + 1} heading`}
                      value={column}
                      onChange={(event) => sync(content.columns.map((x, i) => (i === c ? event.target.value : x)), content.rows, content.blanks)}
                    />
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {content.rows.map((row, r) => (
                <tr key={row.id}>
                  {row.cells.map((cell, c) => (
                    <td key={c}>
                      {"blankId" in cell ? (
                        <button type="button" className={ui.buttonGhost} onClick={() => setCell(r, c, { text: "" })}>
                          Blank {cell.blankId}: make text
                        </button>
                      ) : (
                        <div className={styles.row}>
                          <input
                            aria-label={`Row ${r + 1}, column ${c + 1} text`}
                            value={cell.text}
                            onChange={(event) => setCell(r, c, { text: event.target.value })}
                          />
                          <button type="button" className={ui.buttonGhost} aria-label={`Make row ${r + 1}, column ${c + 1} a blank`} onClick={() => makeBlank(r, c)}>
                            Make blank
                          </button>
                        </div>
                      )}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className={styles.row}>
          <button
            type="button"
            className={ui.buttonSecondary}
            onClick={() =>
              sync(content.columns, [...content.rows, { id: nextId(content.rows.map((x) => x.id), "r"), cells: content.columns.map(() => ({ text: "" })) }], content.blanks)
            }
          >
            Add row
          </button>
          <button type="button" className={ui.buttonGhost} disabled={content.rows.length <= 1} onClick={() => sync(content.columns, content.rows.slice(0, -1), content.blanks)}>
            Remove last row
          </button>
          <button
            type="button"
            className={ui.buttonSecondary}
            onClick={() => sync([...content.columns, ""], content.rows.map((row) => ({ ...row, cells: [...row.cells, { text: "" }] })), content.blanks)}
          >
            Add column
          </button>
          <button
            type="button"
            className={ui.buttonGhost}
            disabled={content.columns.length <= 1}
            onClick={() => sync(content.columns.slice(0, -1), content.rows.map((row) => ({ ...row, cells: row.cells.slice(0, -1) })), content.blanks)}
          >
            Remove last column
          </button>
        </div>
      </fieldset>
      <BlanksEditor
        blanks={content.blanks}
        byBlank={answerKey.byBlank}
        disabled={disabled}
        onChange={(blanks, byBlank) => onChange({ content: { ...content, blanks }, answerKey: { byBlank } })}
      />
    </>
  );
}
