"use client";

import { useId, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { RationalePanel } from "@/features/attempt/reveal/RationalePanel";
import { ExhibitPanel } from "../exhibits/ExhibitPanel";
import { MediaProvider } from "../media/MediaContext";
import { ItemAnswer } from "../renderers/ItemAnswer";
import { caseStudyItem, exhibitItem, galleryMedia } from "./galleryExhibits";
import styles from "./gallery.module.css";
import type { GalleryItem } from "./galleryTypes";
import { sampleItems } from "./sampleItems";

const galleryItems: Array<{ title: string; item: GalleryItem }> = [
  ...sampleItems.map((item) => ({ title: item.itemType, item })),
  { title: "multiple_choice with exhibits", item: exhibitItem },
  { title: "matrix_mc in a case study", item: caseStudyItem },
];

/**
 * /dev/items (development only): every MVP renderer with sample data, to check layout, keyboard
 * use and reveal marks by hand. Each sample keeps its own answer and strikes in local state and
 * shows the response it would send to the save queue.
 */
export function ItemGallery() {
  return (
    <MediaProvider resolve={galleryMedia}>
      <div className={styles.page}>
        <header>
          <h1>Item renderers</h1>
          <p className={ui.muted}>
            Development only. Answer each sample with the keyboard, tick &quot;Show the reveal&quot; to see the marks.
          </p>
          <nav aria-label="Samples">
            <ul className={styles.toc}>
              {galleryItems.map(({ title }) => (
                <li key={title}>
                  <a href={`#${anchor(title)}`}>{title}</a>
                </li>
              ))}
            </ul>
          </nav>
        </header>
        {galleryItems.map(({ title, item }) => (
          <GallerySample key={title} title={title} item={item} />
        ))}
      </div>
    </MediaProvider>
  );
}

function GallerySample({ title, item }: { title: string; item: GalleryItem }) {
  const stemId = useId();
  const [response, setResponse] = useState<unknown>(null);
  const [struck, setStruck] = useState<string[]>([]);
  const [revealed, setRevealed] = useState(false);
  const [locked, setLocked] = useState(false);

  function toggleStrike(optionId: string) {
    setStruck((current) =>
      current.includes(optionId) ? current.filter((id) => id !== optionId) : [...current, optionId],
    );
  }

  return (
    <section id={anchor(title)} className={`${ui.card} ${styles.sample}`} aria-labelledby={`${stemId}-title`}>
      <h2 id={`${stemId}-title`} className={styles.sampleTitle}>
        {title}
      </h2>
      <div className={styles.controls}>
        <label>
          <input type="checkbox" checked={revealed} onChange={(event) => setRevealed(event.target.checked)} /> Show the
          reveal
        </label>
        <label>
          <input type="checkbox" checked={locked} onChange={(event) => setLocked(event.target.checked)} /> Locked
        </label>
      </div>
      <div className={styles.split}>
        <ExhibitPanel exhibits={item.exhibits ?? []} caseStudy={item.caseStudy ?? null} />
        <div className={styles.question}>
          <p id={stemId} className={styles.stem}>
            {item.stem}
          </p>
          <ItemAnswer
            itemType={item.itemType}
            content={item.content}
            response={revealed ? item.revealResponse : response}
            onChange={setResponse}
            disabled={revealed || locked}
            struckOptions={struck}
            onToggleStrike={toggleStrike}
            labelledBy={stemId}
            reveal={revealed ? item.reveal : null}
          />
          {revealed && <RationalePanel reveal={item.reveal} itemType={item.itemType} content={item.content} />}
          <details className={styles.response}>
            <summary>Response sent to the save queue</summary>
            <pre>{JSON.stringify(revealed ? item.revealResponse : response, null, 2)}</pre>
          </details>
        </div>
      </div>
    </section>
  );
}

function anchor(title: string) {
  return title.replace(/[^a-z0-9]+/gi, "-");
}
