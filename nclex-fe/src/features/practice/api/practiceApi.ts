import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type TestSetSummary = ApiSchema<"TestSetSummary">;
export type TestSetDetail = ApiSchema<"TestSetDetail">;
export type TestSetPage = ApiSchema<"TestSetPage">;

/** Sets per page; the API allows up to 50. */
const pageSize = 20;

/** GET /test-sets?kind=prebuilt: one page of the curated sets on the user's own track. */
export async function fetchPrebuiltSets(cursor: string | null): Promise<TestSetPage> {
  const { data, error, response } = await apiClient.GET("/api/v1/test-sets", {
    params: { query: { kind: "prebuilt", limit: pageSize, cursor: cursor ?? undefined } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
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

/** The curated sets, a page at a time ("Show more sets" loads the next one). */
export function usePrebuiltSets() {
  return useInfiniteQuery({
    queryKey: testSetsQueryKey,
    queryFn: ({ pageParam }) => fetchPrebuiltSets(pageParam),
    initialPageParam: null as string | null,
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  });
}

/** One set's details; idle while no set is selected. */
export function useTestSet(id: string | null) {
  return useQuery({
    queryKey: testSetQueryKey(id ?? ""),
    queryFn: () => fetchTestSet(id!),
    enabled: id !== null,
  });
}
