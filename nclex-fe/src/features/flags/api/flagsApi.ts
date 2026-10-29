import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type FlagReason = ApiSchema<"FlagReason">;
export type FlagStatus = ApiSchema<"FlagStatus">;
export type MyFlag = ApiSchema<"MyFlagView">;
export type MyFlagPage = ApiSchema<"MyFlagPage">;
export type FlagQueueItem = ApiSchema<"FlagQueueItem">;
export type FlagQueuePage = ApiSchema<"FlagQueuePage">;
export type UpdateFlagRequest = ApiSchema<"UpdateFlagRequest">;

export interface ReportIssueInput {
  questionVersionId: string;
  reason: FlagReason;
  comment?: string;
  attemptId?: string;
}

/** POST /question-versions/{id}/flags. A second report with the same reason fails with FLAG_DUPLICATE. */
export async function reportIssue({ questionVersionId, ...body }: ReportIssueInput): Promise<MyFlag> {
  const { data, error, response } = await apiClient.POST("/api/v1/question-versions/{id}/flags", {
    params: { path: { id: questionVersionId } },
    body,
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** GET /me/flags: my reports, newest first. */
export async function fetchMyFlags(cursor: string | null): Promise<MyFlagPage> {
  const { data, error, response } = await apiClient.GET("/api/v1/me/flags", {
    params: { query: { cursor: cursor ?? undefined } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** GET /admin/flags: one status of the moderation queue, oldest first. Reviewers and admins only. */
export async function fetchFlagQueue(status: FlagStatus, cursor: string | null): Promise<FlagQueuePage> {
  const { data, error, response } = await apiClient.GET("/api/v1/admin/flags", {
    params: { query: { status, cursor: cursor ?? undefined } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** PATCH /admin/flags/{id}: assign, triage, resolve or reject. Resolving notifies the reporter. */
export async function updateFlag(id: string, body: UpdateFlagRequest): Promise<FlagQueueItem> {
  const { data, error, response } = await apiClient.PATCH("/api/v1/admin/flags/{id}", {
    params: { path: { id } },
    body,
  });
  if (data) return data;
  throw ApiError.from(response, error);
}
