import { useMutation, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type WeakAreasView = ApiSchema<"WeakAreasView">;
export type WeakArea = ApiSchema<"WeakAreaView">;
export type StartedPractice = ApiSchema<"CreateCustomSetResponse">;

export const weakAreasQueryKey = ["me", "weak-areas"] as const;

/** GET /me/weak-areas: the weak areas in the last 10 tests and the size of the test they would give. */
export async function fetchWeakAreas(): Promise<WeakAreasView> {
  const { data, error, response } = await apiClient.GET("/api/v1/me/weak-areas");
  if (data) return data;
  throw ApiError.from(response, error);
}

/**
 * POST /test-sets/weak-areas: builds the practice test and starts it. Fails with NO_WEAK_AREAS or
 * INSUFFICIENT_QUESTIONS (422) when the areas changed since the card loaded.
 */
export async function startWeakAreaPractice(): Promise<StartedPractice> {
  const { data, error, response } = await apiClient.POST("/api/v1/test-sets/weak-areas", { body: {} });
  if (data) return data;
  throw ApiError.from(response, error);
}

export function useWeakAreas() {
  return useQuery({ queryKey: weakAreasQueryKey, queryFn: fetchWeakAreas });
}

export function useStartWeakAreaPractice() {
  return useMutation({ mutationFn: startWeakAreaPractice });
}
