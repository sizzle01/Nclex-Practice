"use client";

import { useQuery } from "@tanstack/react-query";
import { createContext, type ReactNode, useContext } from "react";
import { fetchMedia, mediaQueryKey } from "./mediaApi";

/** Where a media asset is served from, and the text that makes it accessible (database.md media_assets). */
export interface MediaAsset {
  url: string;
  /** Required for images before publish. */
  altText?: string | null;
  /** WebVTT captions, required for audio and video before publish. */
  captionsUrl?: string | null;
  transcriptUrl?: string | null;
}

/** Turns a mediaId into an asset without asking the API; used by the dev gallery's local files. */
export type MediaResolver = (mediaId: string) => MediaAsset | null;

/** What a component knows about one media asset. */
export interface MediaState {
  asset: MediaAsset | null;
  loading: boolean;
  /** Asks the API for fresh signed links, e.g. after the browser was refused an expired one. */
  refresh(): void;
}

// Signed links work for 10 minutes (api.md section 10). A link younger than 5 minutes is reused,
// so a media element that mounts now still has at least 5 minutes to load.
const linkReuseMs = 5 * 60 * 1000;

const MediaContext = createContext<MediaResolver | null>(null);

/** Serves media from a local resolver instead of the API, for the dev gallery and tests. */
export function MediaProvider({ resolve, children }: { resolve: MediaResolver; children: ReactNode }) {
  return <MediaContext.Provider value={resolve}>{children}</MediaContext.Provider>;
}

/**
 * The asset for a mediaId: from the API (GET /media/{id}, with signed links), or from the
 * MediaProvider's resolver when there is one. A null asset with loading false means it can't be shown.
 */
export function useMedia(mediaId: string | null | undefined): MediaState {
  const resolve = useContext(MediaContext);
  const query = useQuery({
    queryKey: mediaQueryKey(mediaId ?? ""),
    queryFn: () => fetchMedia(mediaId!),
    enabled: Boolean(mediaId) && resolve === null,
    staleTime: linkReuseMs,
    // A fresh link would change the src and restart a playing clip, so links are only renewed
    // when a component asks (refresh) or mounts after they went stale.
    refetchOnWindowFocus: false,
    retry: false,
  });

  if (!mediaId) return { asset: null, loading: false, refresh: () => undefined };
  if (resolve) return { asset: resolve(mediaId), loading: false, refresh: () => undefined };
  return { asset: query.data ?? null, loading: query.isPending, refresh: () => void query.refetch() };
}
