import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { AttemptPage } from "@/features/attempt/api/attemptApi";

export const historyPageSize = 20;
export const recentResultsCount = 5;

export const historyQueryKeys = {
  all: ["attempts", "history"] as const,
  recent: ["attempts", "history", "recent"] as const,
};

/** GET /attempts?status=submitted: one page of finished attempts, most recent first. */
export async function fetchHistory(limit: number, cursor?: string): Promise<AttemptPage> {
  const { data, error, response } = await apiClient.GET("/api/v1/attempts", {
    params: { query: { status: "submitted", limit, cursor } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** The /history table: pages are added with "Show more", following nextCursor. */
export function useHistory() {
  return useInfiniteQuery({
    queryKey: historyQueryKeys.all,
    queryFn: ({ pageParam }) => fetchHistory(historyPageSize, pageParam),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  });
}

/** The dashboard's latest results. */
export function useRecentResults() {
  return useQuery({ queryKey: historyQueryKeys.recent, queryFn: () => fetchHistory(recentResultsCount) });
}
