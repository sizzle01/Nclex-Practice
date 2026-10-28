"use client";

import { useState } from "react";
import { useMedia } from "../media/MediaContext";
import styles from "./exhibits.module.css";
import type { Exhibit } from "./exhibitTypes";

/**
 * An image, audio or video exhibit. Images carry their alt text; audio and video carry WebVTT
 * captions and, when there is one, a transcript link (interfaces.md section 4, Media). When the
 * media can't be served the candidate is told so, rather than seeing an empty box.
 */
export function ExhibitMedia({ exhibit }: { exhibit: Exhibit }) {
  const { asset, loading, refresh } = useMedia(exhibit.mediaId);
  const [failedUrl, setFailedUrl] = useState<string | null>(null);

  if (loading) {
    return <p className={styles.unavailable}>Loading {exhibit.title}…</p>;
  }

  if (!asset || asset.url === failedUrl) {
    return (
      <p role="note" className={styles.unavailable}>
        {exhibit.title}: this {exhibit.kind} can&apos;t be shown right now.
      </p>
    );
  }

  // Signed links expire. If the browser is refused one, ask once for fresh links; a second
  // failure of the same file shows the note above.
  function onLoadError() {
    if (failedUrl !== null) {
      setFailedUrl(asset!.url);
      return;
    }

    setFailedUrl("");
    refresh();
  }

  if (exhibit.kind === "image") {
    return (
      <figure className={styles.figure}>
        {/* Media come from the API or a CDN at their own sizes, so next/image's optimiser does not apply. */}
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={asset.url} alt={asset.altText ?? ""} className={styles.image} onError={onLoadError} />
        <figcaption>{exhibit.title}</figcaption>
      </figure>
    );
  }

  const Player = exhibit.kind === "video" ? "video" : "audio";
  return (
    <figure className={styles.figure}>
      <Player
        controls
        preload="metadata"
        src={asset.url}
        className={styles.player}
        aria-label={exhibit.title}
        onError={onLoadError}
      >
        {asset.captionsUrl && <track kind="captions" src={asset.captionsUrl} srcLang="en" label="English" default />}
      </Player>
      <figcaption>
        {exhibit.title}
        {asset.transcriptUrl && (
          <>
            {" "}
            <a href={asset.transcriptUrl} target="_blank" rel="noreferrer">
              Read the transcript
            </a>
          </>
        )}
      </figcaption>
    </figure>
  );
}
