"use client";

import { createContext, type ReactNode, useContext } from "react";

/** Where a media asset is served from, and the text that makes it accessible (database.md media_assets). */
export interface MediaAsset {
  url: string;
  /** Required for images before publish. */
  altText?: string;
  /** WebVTT captions, required for audio and video before publish. */
  captionsUrl?: string;
  transcriptUrl?: string;
}

/** Turns a mediaId from an item payload into a playable asset, or null when it can't be served. */
export type MediaResolver = (mediaId: string) => MediaAsset | null;

/**
 * No candidate endpoint serves media yet (see the F11 walkthrough, Follow-ups), so by default
 * nothing resolves and the renderers say the media can't be shown. The dev gallery provides
 * local sample files.
 */
const noMedia: MediaResolver = () => null;

const MediaContext = createContext<MediaResolver>(noMedia);

export function MediaProvider({ resolve, children }: { resolve: MediaResolver; children: ReactNode }) {
  return <MediaContext.Provider value={resolve}>{children}</MediaContext.Provider>;
}

/** The asset for a mediaId, or null when there is no id or it can't be served. */
export function useMedia(mediaId: string | null | undefined): MediaAsset | null {
  const resolve = useContext(MediaContext);
  return mediaId ? resolve(mediaId) : null;
}
