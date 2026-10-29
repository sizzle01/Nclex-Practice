"use client";

import { useId, useState } from "react";
import type { Exhibit } from "@/features/player/exhibits/exhibitTypes";
import { ExhibitPanel } from "@/features/player/exhibits/ExhibitPanel";
import { ItemAnswer } from "@/features/player/renderers/ItemAnswer";
import type { ItemType } from "@/features/player/renderers/types";
import styles from "./editor.module.css";

interface LivePreviewProps {
  itemType: ItemType;
  stem: string;
  content: unknown;
  exhibits: Exhibit[];
}

/**
 * The item as a candidate will see it, drawn by the player's own renderers (F11) from the form as
 * it is now, saved or not. Answers here only try the controls; "Test the key" grades.
 */
export function LivePreview({ itemType, stem, content, exhibits }: LivePreviewProps) {
  const stemId = useId();
  return (
    <section className={styles.preview} aria-label="Candidate preview">
      <h2 className={styles.panelTitle}>Candidate preview</h2>
      <ExhibitPanel exhibits={exhibits} caseStudy={null} />
      <p id={stemId} className={styles.stem}>
        {stem || "The stem appears here."}
      </p>
      {/* A new key when the content changes starts a fresh answer, which can't name removed ids. */}
      <PreviewAnswer key={`${itemType}:${JSON.stringify(content)}`} itemType={itemType} content={content} stemId={stemId} />
    </section>
  );
}

function PreviewAnswer({ itemType, content, stemId }: { itemType: ItemType; content: unknown; stemId: string }) {
  const [response, setResponse] = useState<unknown>(null);
  const [struck, setStruck] = useState<string[]>([]);
  return (
    <ItemAnswer
      itemType={itemType}
      content={content}
      response={response}
      onChange={setResponse}
      disabled={false}
      struckOptions={struck}
      onToggleStrike={(id) => setStruck((s) => (s.includes(id) ? s.filter((x) => x !== id) : [...s, id]))}
      labelledBy={stemId}
      reveal={null}
    />
  );
}
