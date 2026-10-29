"use client";

import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import type { Exhibit } from "@/features/player/exhibits/exhibitTypes";
import { isApiError } from "@/lib/api/errors";
import { updateMedia, uploadMedia } from "../api/adminApi";
import { nextId } from "../item-forms/payloads";
import styles from "../item-forms/itemForms.module.css";

const kinds: Array<{ kind: Exhibit["kind"]; label: string }> = [
  { kind: "text", label: "Text" },
  { kind: "table", label: "Table" },
  { kind: "flowsheet", label: "Flowsheet (trend)" },
  { kind: "image", label: "Image" },
  { kind: "audio", label: "Audio" },
  { kind: "video", label: "Video" },
];

const accepts: Partial<Record<Exhibit["kind"], string>> = {
  image: "image/png,image/jpeg,image/webp,image/gif",
  audio: "audio/mpeg,audio/wav,audio/mp4",
  video: "video/mp4,video/webm",
};

interface ExhibitsEditorProps {
  value: Exhibit[];
  onChange(next: Exhibit[]): void;
  disabled: boolean;
}

/**
 * The item's exhibits (interfaces.md section 1): text, tables, flowsheets and media. Tables are
 * typed one row per line, cells separated by "|", which is quick to write and to read back.
 */
export function ExhibitsEditor({ value, onChange, disabled }: ExhibitsEditorProps) {
  function update(index: number, exhibit: Exhibit) {
    onChange(value.map((e, i) => (i === index ? exhibit : e)));
  }

  return (
    <fieldset className={styles.list} disabled={disabled}>
      <legend>Exhibits</legend>
      {value.length === 0 && <p className={styles.hint}>No exhibits. Add a chart, notes or media the question refers to.</p>}
      {value.map((exhibit, index) => (
        <div key={exhibit.id} className={styles.group}>
          <div className={styles.inline}>
            <label>
              Kind
              <select value={exhibit.kind} onChange={(event) => update(index, blankExhibit(exhibit.id, event.target.value as Exhibit["kind"], exhibit.title))}>
                {kinds.map((k) => (
                  <option key={k.kind} value={k.kind}>
                    {k.label}
                  </option>
                ))}
              </select>
            </label>
            <label className={styles.grow}>
              Title
              <input value={exhibit.title} onChange={(event) => update(index, { ...exhibit, title: event.target.value })} />
            </label>
            <button type="button" className={ui.buttonGhost} onClick={() => onChange(value.filter((_, i) => i !== index))}>
              Remove exhibit {index + 1}
            </button>
          </div>
          {exhibit.kind === "text" && (
            <label>
              Text
              <textarea rows={4} value={exhibit.body ?? ""} onChange={(event) => update(index, { ...exhibit, body: event.target.value })} />
            </label>
          )}
          {(exhibit.kind === "table" || exhibit.kind === "flowsheet") && <TableText exhibit={exhibit} onChange={(next) => update(index, next)} />}
          {(exhibit.kind === "image" || exhibit.kind === "audio" || exhibit.kind === "video") && (
            <MediaFields exhibit={exhibit} onChange={(next) => update(index, next)} />
          )}
        </div>
      ))}
      <button
        type="button"
        className={ui.buttonSecondary}
        onClick={() => onChange([...value, blankExhibit(nextId(value.map((e) => e.id), "x"), "text", "")])}
      >
        Add exhibit
      </button>
    </fieldset>
  );
}

function blankExhibit(id: string, kind: Exhibit["kind"], title: string): Exhibit {
  switch (kind) {
    case "text":
      return { id, kind, title, body: "" };
    case "table":
    case "flowsheet":
      return { id, kind, title, table: { columns: ["", ""], rows: [{ label: "", cells: [""] }] } };
    default:
      return { id, kind, title, mediaId: null };
  }
}

/**
 * A table as text: the first line is the column headings, each other line a row whose first cell
 * is its label. Kept as typed until it parses, so a half-typed line doesn't vanish.
 */
function TableText({ exhibit, onChange }: { exhibit: Exhibit; onChange(next: Exhibit): void }) {
  const [text, setText] = useState(() => tableToText(exhibit.table));
  return (
    <label>
      Table: first line the headings, then one row per line, cells separated by |
      <textarea
        rows={5}
        value={text}
        onChange={(event) => {
          setText(event.target.value);
          onChange({ ...exhibit, table: textToTable(event.target.value) });
        }}
      />
    </label>
  );
}

export function tableToText(table: Exhibit["table"]): string {
  if (!table) return "";
  return [table.columns.join(" | "), ...table.rows.map((row) => [row.label, ...row.cells].join(" | "))].join("\n");
}

export function textToTable(text: string): NonNullable<Exhibit["table"]> {
  const lines = text.split("\n").filter((line) => line.trim() !== "");
  const split = (line: string) => line.split("|").map((cell) => cell.trim());
  const [head = "", ...rest] = lines;
  return {
    columns: split(head),
    rows: rest.map((line) => {
      const [label = "", ...cells] = split(line);
      return { label, cells };
    }),
  };
}

/** Upload a file for an image, audio or video exhibit, then describe it so everyone can follow it. */
function MediaFields({ exhibit, onChange }: { exhibit: Exhibit; onChange(next: Exhibit): void }) {
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [altText, setAltText] = useState("");
  const [transcript, setTranscript] = useState("");
  const [captions, setCaptions] = useState<string | null>(null);

  async function run(action: () => Promise<void>, done: string) {
    setBusy(true);
    setMessage(null);
    try {
      await action();
      setMessage(done);
    } catch (error) {
      setMessage(isApiError(error) ? error.message : "Something went wrong. Please try again.");
    } finally {
      setBusy(false);
    }
  }

  async function upload(file: File) {
    await run(async () => {
      const mediaId = await uploadMedia(file);
      onChange({ ...exhibit, mediaId });
    }, "Uploaded. Now describe it below.");
  }

  async function describe() {
    if (!exhibit.mediaId) return;
    const mediaId = exhibit.mediaId;
    await run(
      async () => {
        await updateMedia(
          mediaId,
          // null leaves a field as it is (PATCH).
          exhibit.kind === "image"
            ? { altText, captions: null, transcript: null }
            : { altText: null, captions: captions ?? null, transcript: transcript || null },
        );
      },
      "Saved.",
    );
  }

  return (
    <div className={styles.group}>
      <label>
        {exhibit.mediaId ? "Replace the file" : "Upload a file"}
        <input type="file" accept={accepts[exhibit.kind]} disabled={busy} onChange={(event) => event.target.files?.[0] && void upload(event.target.files[0])} />
      </label>
      {exhibit.mediaId && <p className={styles.hint}>Media id: {exhibit.mediaId}</p>}
      {exhibit.mediaId && exhibit.kind === "image" && (
        <label>
          Alt text (required before review)
          <input value={altText} onChange={(event) => setAltText(event.target.value)} />
        </label>
      )}
      {exhibit.mediaId && exhibit.kind !== "image" && (
        <>
          <label>
            Captions file (WebVTT, required before review)
            <input type="file" accept=".vtt,text/vtt" onChange={(event) => event.target.files?.[0]?.text().then(setCaptions)} />
          </label>
          <label>
            Transcript (required before review)
            <textarea rows={3} value={transcript} onChange={(event) => setTranscript(event.target.value)} />
          </label>
        </>
      )}
      {exhibit.mediaId && (
        <button type="button" className={ui.buttonSecondary} disabled={busy} onClick={() => void describe()}>
          Save media details
        </button>
      )}
      <p role="status" className={styles.hint}>
        {busy ? "Working…" : (message ?? "")}
      </p>
    </div>
  );
}
