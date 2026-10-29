"use client";

import ui from "@/components/ui/ui.module.css";
import { toggle } from "./ChoiceForms";
import { type ItemFormProps, nextId } from "./payloads";
import styles from "./itemForms.module.css";

/**
 * Highlight text (interfaces.md 2.7): the passage split into segments. Only selectable segments
 * can be highlighted, and only they need a rationale. "Split into sentences" turns a pasted
 * passage into segments.
 */
export function HighlightTextForm({ value, onChange, disabled }: ItemFormProps<"highlight_text">) {
  const { content, answerKey } = value;
  const segments = content.segments;

  function setSegments(next: typeof segments) {
    const correct = answerKey.correct.filter((id) => next.some((s) => s.id === id && s.selectable));
    onChange({ content: { segments: next }, answerKey: { correct } });
  }

  function splitPassage() {
    const passage = segments.map((s) => s.text).join(" ");
    const sentences = passage.match(/[^.!?]+[.!?]*/g)?.map((s) => s.trim()).filter(Boolean) ?? [];
    setSegments(sentences.map((text, i) => ({ id: `s${i + 1}`, text, selectable: true })));
  }

  return (
    <fieldset className={styles.list} disabled={disabled}>
      <legend>Passage segments</legend>
      <ol className={styles.rows}>
        {segments.map((segment, index) => (
          <li key={segment.id} className={styles.row}>
            <span className={styles.id} aria-hidden="true">
              {segment.id}
            </span>
            <input
              className={styles.grow}
              aria-label={`Segment ${segment.id} text`}
              value={segment.text}
              onChange={(event) => setSegments(segments.map((s, i) => (i === index ? { ...s, text: event.target.value } : s)))}
            />
            <label className={styles.mark}>
              <input
                type="checkbox"
                checked={segment.selectable}
                onChange={() => setSegments(segments.map((s, i) => (i === index ? { ...s, selectable: !s.selectable } : s)))}
              />
              Selectable
            </label>
            <label className={styles.mark}>
              <input
                type="checkbox"
                disabled={!segment.selectable}
                checked={answerKey.correct.includes(segment.id)}
                onChange={() => onChange({ content, answerKey: { correct: toggle(answerKey.correct, segment.id) } })}
              />
              Correct
            </label>
            <button type="button" className={ui.buttonGhost} aria-label={`Remove segment ${segment.id}`} onClick={() => setSegments(segments.filter((_, i) => i !== index))}>
              Remove
            </button>
          </li>
        ))}
      </ol>
      <div className={styles.row}>
        <button
          type="button"
          className={ui.buttonSecondary}
          onClick={() => setSegments([...segments, { id: nextId(segments.map((s) => s.id), "s"), text: "", selectable: true }])}
        >
          Add segment
        </button>
        <button type="button" className={ui.buttonGhost} onClick={splitPassage}>
          Split into sentences
        </button>
      </div>
    </fieldset>
  );
}

/** Highlight table (2.7): table rows that candidates highlight whole. */
export function HighlightTableForm({ value, onChange, disabled }: ItemFormProps<"highlight_table">) {
  const { content, answerKey } = value;

  function set(columns: string[], rows: typeof content.rows) {
    const correct = answerKey.correct.filter((id) => rows.some((r) => r.id === id && r.selectable));
    onChange({ content: { columns, rows }, answerKey: { correct } });
  }

  return (
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
                    onChange={(event) => set(content.columns.map((x, i) => (i === c ? event.target.value : x)), content.rows)}
                  />
                </th>
              ))}
              <th scope="col">Selectable</th>
              <th scope="col">Correct</th>
            </tr>
          </thead>
          <tbody>
            {content.rows.map((row, r) => (
              <tr key={row.id}>
                {row.cells.map((cell, c) => (
                  <td key={c}>
                    <input
                      aria-label={`Row ${row.id}, column ${c + 1}`}
                      value={cell}
                      onChange={(event) =>
                        set(content.columns, content.rows.map((x, i) => (i === r ? { ...x, cells: x.cells.map((y, j) => (j === c ? event.target.value : y)) } : x)))
                      }
                    />
                  </td>
                ))}
                <td>
                  <input
                    type="checkbox"
                    aria-label={`Row ${row.id} selectable`}
                    checked={row.selectable}
                    onChange={() => set(content.columns, content.rows.map((x, i) => (i === r ? { ...x, selectable: !x.selectable } : x)))}
                  />
                </td>
                <td>
                  <input
                    type="checkbox"
                    aria-label={`Row ${row.id} correct`}
                    disabled={!row.selectable}
                    checked={answerKey.correct.includes(row.id)}
                    onChange={() => onChange({ content, answerKey: { correct: toggle(answerKey.correct, row.id) } })}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className={styles.row}>
        <button
          type="button"
          className={ui.buttonSecondary}
          onClick={() => set(content.columns, [...content.rows, { id: nextId(content.rows.map((r) => r.id), "r"), cells: content.columns.map(() => ""), selectable: true }])}
        >
          Add row
        </button>
        <button type="button" className={ui.buttonGhost} disabled={content.rows.length <= 1} onClick={() => set(content.columns, content.rows.slice(0, -1))}>
          Remove last row
        </button>
        <button type="button" className={ui.buttonSecondary} onClick={() => set([...content.columns, ""], content.rows.map((r) => ({ ...r, cells: [...r.cells, ""] })))}>
          Add column
        </button>
        <button
          type="button"
          className={ui.buttonGhost}
          disabled={content.columns.length <= 1}
          onClick={() => set(content.columns.slice(0, -1), content.rows.map((r) => ({ ...r, cells: r.cells.slice(0, -1) })))}
        >
          Remove last column
        </button>
      </div>
    </fieldset>
  );
}
