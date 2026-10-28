import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type PerformanceSummary = ApiSchema<"PerformanceSummary">;
export type PerformanceBreakdown = ApiSchema<"PerformanceBreakdown">;
export type PerformanceTrend = ApiSchema<"PerformanceTrend">;
export type BreakdownRow = ApiSchema<"BreakdownRow">;
export type TrendPoint = ApiSchema<"TrendPoint">;
export type PerformanceDimension = ApiSchema<"PerformanceDimension">;
export type BreakdownDimension = Exclude<PerformanceDimension, "overall">;
export type TrendBucket = ApiSchema<"TrendBucket">;

/** What a trend chart follows: overall, or one category of a dimension. */
export type TrendQuery = { dimension: PerformanceDimension; key?: string; bucket: TrendBucket };

export const performanceQueryKeys = {
  summary: ["performance", "summary"] as const,
  breakdown: (dimension: BreakdownDimension) => ["performance", "breakdown", dimension] as const,
  trend: (query: TrendQuery) => ["performance", "trend", query] as const,
};

export async function fetchSummary(): Promise<PerformanceSummary> {
  const { data, error, response } = await apiClient.GET("/api/v1/me/performance/summary");
  if (data) return data;
  throw ApiError.from(response, error);
}

export async function fetchBreakdown(dimension: BreakdownDimension): Promise<PerformanceBreakdown> {
  const { data, error, response } = await apiClient.GET("/api/v1/me/performance/breakdown", {
    params: { query: { dimension } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

export async function fetchTrend(query: TrendQuery): Promise<PerformanceTrend> {
  const { data, error, response } = await apiClient.GET("/api/v1/me/performance/trend", { params: { query } });
  if (data) return data;
  throw ApiError.from(response, error);
}

// Snapshots are refreshed by a background job a few seconds after a submit, so these are always
// refetched when a page mounts rather than cached for long.

export function usePerformanceSummary() {
  return useQuery({ queryKey: performanceQueryKeys.summary, queryFn: fetchSummary });
}

export function useBreakdown(dimension: BreakdownDimension) {
  return useQuery({ queryKey: performanceQueryKeys.breakdown(dimension), queryFn: () => fetchBreakdown(dimension) });
}

/** Pass undefined to wait, e.g. until a category has been picked. */
export function useTrend(query: TrendQuery | undefined) {
  return useQuery({
    queryKey: performanceQueryKeys.trend(query ?? { dimension: "overall", bucket: "week" }),
    queryFn: () => fetchTrend(query!),
    enabled: query !== undefined,
  });
}
