import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";
import type { PendingWrite } from "../player/saveQueue";

export type AttemptState = ApiSchema<"AttemptState">;
export type AttemptSummary = ApiSchema<"AttemptSummary">;
export type AttemptPage = ApiSchema<"AttemptPage">;
export type CandidateItem = ApiSchema<"CandidateItemView">;
export type ItemState = ApiSchema<"ItemState">;

export function attemptQueryKey(attemptId: string) {
  return ["attempts", attemptId] as const;
}

export function itemQueryKey(attemptId: string, position: number) {
  return ["attempts", attemptId, "items", position] as const;
}

export const draftsQueryKey = ["attempts", "drafts"] as const;

/** POST /attempts: starts a set, or returns the existing draft of it. */
export async function startAttempt(testSetId: string): Promise<AttemptState> {
  const { data, error, response } = await apiClient.POST("/api/v1/attempts", { body: { testSetId } });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** POST /attempts/{id}/open: claims the device lease for this page load. */
export async function openAttempt(attemptId: string, clientId: string): Promise<AttemptState> {
  const { data, error, response } = await apiClient.POST("/api/v1/attempts/{id}/open", {
    params: { path: { id: attemptId }, header: { "X-Attempt-Client": clientId } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

export async function fetchAttempt(attemptId: string): Promise<AttemptState> {
  const { data, error, response } = await apiClient.GET("/api/v1/attempts/{id}", { params: { path: { id: attemptId } } });
  if (data) return data;
  throw ApiError.from(response, error);
}

export async function fetchItem(attemptId: string, position: number): Promise<CandidateItem> {
  const { data, error, response } = await apiClient.GET("/api/v1/attempts/{id}/items/{position}", {
    params: { path: { id: attemptId, position } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** PUT …/response: one autosave. Used only by the save queue. */
export async function saveResponse(
  attemptId: string,
  clientId: string,
  position: number,
  write: PendingWrite,
  keepalive: boolean,
): Promise<void> {
  const { error, response } = await apiClient.PUT("/api/v1/attempts/{id}/items/{position}/response", {
    params: { path: { id: attemptId, position }, header: { "X-Attempt-Client": clientId } },
    body: { response: write.response ?? null, clientSeq: write.clientSeq },
    // Lets the last write finish while the tab is closing (pagehide).
    keepalive,
  });
  if (!response.ok) throw ApiError.from(response, error);
}

export async function navigateTo(attemptId: string, clientId: string, toPosition: number) {
  const { data, error, response } = await apiClient.POST("/api/v1/attempts/{id}/navigate", {
    params: { path: { id: attemptId }, header: { "X-Attempt-Client": clientId } },
    body: { toPosition },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

export async function setFlagged(attemptId: string, clientId: string, position: number, flagged: boolean) {
  const { data, error, response } = await apiClient.PATCH("/api/v1/attempts/{id}/items/{position}", {
    params: { path: { id: attemptId, position }, header: { "X-Attempt-Client": clientId } },
    body: { flagged },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** GET /attempts?status=in_progress,paused: the dashboard's drafts. */
export async function fetchDrafts(): Promise<AttemptPage> {
  const { data, error, response } = await apiClient.GET("/api/v1/attempts", {
    params: { query: { status: "in_progress,paused" } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

export async function discardAttempt(attemptId: string): Promise<void> {
  const { error, response } = await apiClient.DELETE("/api/v1/attempts/{id}", { params: { path: { id: attemptId } } });
  if (!response.ok) throw ApiError.from(response, error);
}

export function useDrafts() {
  return useQuery({ queryKey: draftsQueryKey, queryFn: fetchDrafts });
}

export function useDiscardAttempt() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: discardAttempt,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: draftsQueryKey }),
  });
}

export function useStartAttempt() {
  return useMutation({ mutationFn: startAttempt });
}
