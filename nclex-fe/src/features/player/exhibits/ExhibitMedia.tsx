"use client";

import { useMedia } from "../media/MediaContext";
import styles from "./exhibits.module.css";
import type { Exhibit } from "./exhibitTypes";

/**
 * An image, audio or video exhibit. Images carry their alt text; audio and video carry WebVTT
 * captions and a transcript link (interfaces.md section 4, Media). When the media can't be served
 * the candidate is told so, rather than seeing an empty box.
 */
export function ExhibitMedia({ exhibit }: { exhibit: Exhibit }) {
  const media = useMedia(exhibit.mediaId);
  if (!media) {
    return (
      <p role="note" className={styles.unavailable}>
        {exhibit.title}: this {exhibit.kind} can&apos;t be shown right now.
      </p>
    );
  }

  if (exhibit.kind === "image") {
    return (
      <figure className={styles.figure}>
        {/* Media come from the CDN at their own sizes, so next/image's optimiser does not apply. */}
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={media.url} alt={media.altText ?? ""} className={styles.image} />
        <figcaption>{exhibit.title}</figcaption>
      </figure>
    );
  }

  const Player = exhibit.kind === "video" ? "video" : "audio";
  return (
    <figure className={styles.figure}>
      <Player controls preload="metadata" src={media.url} className={styles.player} aria-label={exhibit.title}>
        {media.captionsUrl && <track kind="captions" src={media.captionsUrl} srcLang="en" label="English" default />}
      </Player>
      <figcaption>
        {exhibit.title}
        {media.transcriptUrl && (
          <>
            {" "}
            <a href={media.transcriptUrl} target="_blank" rel="noreferrer">
              Read the transcript
            </a>
          </>
        )}
      </figcaption>
    </figure>
  );
}
