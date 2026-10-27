import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type Taxonomy = ApiSchema<"TaxonomyResponse">;
export type ClientNeedsNode = ApiSchema<"ClientNeedsNode">;
export type Track = ApiSchema<"ExamTrack">;
export type ItemType = ApiSchema<"ItemType">;
export type Difficulty = ApiSchema<"DifficultyTier">;
export type PoolMode = ApiSchema<"PoolMode">;
export type CreateCustomSetRequest = ApiSchema<"CreateCustomSetRequest">;
export type CreateCustomSetResponse = ApiSchema<"CreateCustomSetResponse">;

/** The builder's filters, as both endpoints take them. Empty lists mean "any". */
export interface PoolFilters {
  track: Track;
  clientNeedsIds: number[];
  bodySystemIds: number[];
  itemTypes: ItemType[];
  difficulties: Difficulty[];
  pool: PoolMode;
}

/** GET /taxonomy: the Client Needs tree, body systems and item types to choose from. */
export async function fetchTaxonomy(track: Track): Promise<Taxonomy> {
  const { data, error, response } = await apiClient.GET("/api/v1/taxonomy", { params: { query: { track } } });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** GET /test-sets/custom/available-count: how many questions match the filters right now. */
export async function fetchAvailableCount(filters: PoolFilters): Promise<number> {
  const { data, error, response } = await apiClient.GET("/api/v1/test-sets/custom/available-count", {
    params: { query: filters },
  });
  if (data) return Number(data.available);
  throw ApiError.from(response, error);
}

/** POST /test-sets/custom. Fails with INSUFFICIENT_QUESTIONS (422, with `available`) when too few match. */
export async function createCustomSet(body: CreateCustomSetRequest): Promise<CreateCustomSetResponse> {
  const { data, error, response } = await apiClient.POST("/api/v1/test-sets/custom", { body });
  if (data) return data;
  throw ApiError.from(response, error);
}

export function useTaxonomy(track: Track | undefined) {
  return useQuery({
    queryKey: ["taxonomy", track],
    queryFn: () => fetchTaxonomy(track!),
    enabled: track !== undefined,
    // Seed data: it only changes with a deployment.
    staleTime: Infinity,
  });
}

/** The live count. Keeps showing the previous number while a new one loads, so it doesn't flicker. */
export function useAvailableCount(filters: PoolFilters | undefined) {
  return useQuery({
    queryKey: ["test-sets", "custom", "available-count", filters],
    queryFn: () => fetchAvailableCount(filters!),
    enabled: filters !== undefined,
    placeholderData: keepPreviousData,
  });
}
