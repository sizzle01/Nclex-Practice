import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type TestSetSummary = ApiSchema<"TestSetSummary">;
export type TestSetDetail = ApiSchema<"TestSetDetail">;
export type TestSetPage = ApiSchema<"TestSetPage">;

/** Sets per page: the API's maximum, so the whole RN catalog usually arrives in one request. */
const pageSize = 50;

/**
 * GET /test-sets: one page of the curated sets on the user's own track. With no kind, the API
 * returns prebuilt sets and adaptive exams (F19) together, as the Practice page shows both.
 */
export async function fetchPrebuiltSets(cursor: string | null): Promise<TestSetPage> {
  const { data, error, response } = await apiClient.GET("/api/v1/test-sets", {
    params: { query: { limit: pageSize, cursor: cursor ?? undefined } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/**
 * Every curated set, following nextCursor until the last page. The Practice page groups sets into
 * sections and tabs, which only makes sense once it has all of them.
 */
export async function fetchAllPrebuiltSets(): Promise<TestSetSummary[]> {
  const sets: TestSetSummary[] = [];
  let cursor: string | null = null;
  do {
    const page: TestSetPage = await fetchPrebuiltSets(cursor);
    sets.push(...page.items);
    cursor = page.nextCursor ?? null;
  } while (cursor !== null);
  return sets;
}

/** GET /test-sets/{id}: one set's settings for the detail panel. */
export async function fetchTestSet(id: string): Promise<TestSetDetail> {
  const { data, error, response } = await apiClient.GET("/api/v1/test-sets/{id}", { params: { path: { id } } });
  if (data) return data;
  throw ApiError.from(response, error);
}

export const testSetsQueryKey = ["test-sets", "prebuilt"] as const;

export function testSetQueryKey(id: string) {
  return ["test-sets", "detail", id] as const;
}

/** All the curated sets of the user's track, in the API's order. */
export function usePrebuiltSets() {
  return useQuery({ queryKey: testSetsQueryKey, queryFn: fetchAllPrebuiltSets });
}

/** One set's details; idle while no set is selected. */
export function useTestSet(id: string | null) {
  return useQuery({
    queryKey: testSetQueryKey(id ?? ""),
    queryFn: () => fetchTestSet(id!),
    enabled: id !== null,
  });
}
