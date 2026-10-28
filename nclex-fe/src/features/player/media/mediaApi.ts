import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";
import type { MediaAsset } from "./MediaContext";

export type MediaView = ApiSchema<"MediaView">;

export function mediaQueryKey(mediaId: string) {
  return ["media", mediaId] as const;
}

/** GET /media/{id}: the asset's alt text and links to its file, captions and transcript, signed for a few minutes. */
export async function fetchMedia(mediaId: string): Promise<MediaAsset> {
  const { data, error, response } = await apiClient.GET("/api/v1/media/{id}", {
    params: { path: { id: mediaId } },
  });
  if (data) {
    return {
      url: data.url,
      altText: data.altText,
      captionsUrl: data.captionsUrl,
      transcriptUrl: data.transcriptUrl,
    };
  }
  throw ApiError.from(response, error);
}
