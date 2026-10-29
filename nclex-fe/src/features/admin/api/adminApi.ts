import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type AdminQuestionView = ApiSchema<"AdminQuestionView">;
export type AdminVersionView = ApiSchema<"AdminVersionView">;
export type QuestionSummary = ApiSchema<"QuestionSummary">;
export type QuestionSearchPage = ApiSchema<"QuestionSearchPage">;
export type DraftInput = ApiSchema<"DraftInput">;
export type VersionStatus = ApiSchema<"VersionStatus">;
export type ItemGrade = ApiSchema<"ItemGrade">;
export type AdminMediaView = ApiSchema<"AdminMediaView">;
export type UpdateMediaRequest = ApiSchema<"UpdateMediaRequest">;
export type AdminTestSetPage = ApiSchema<"AdminTestSetPage">;
export type AdminTestSetView = ApiSchema<"AdminTestSetView">;
export type TestSetSettings = ApiSchema<"TestSetSettings">;
export type CreateTestSetRequest = ApiSchema<"CreateTestSetRequest">;
export type UpdateTestSetRequest = ApiSchema<"UpdateTestSetRequest">;
export type AdminBankPage = ApiSchema<"AdminBankPage">;
export type AdminBankView = ApiSchema<"AdminBankView">;
export type CreateBankRequest = ApiSchema<"CreateBankRequest">;
export type UpdateBankRequest = ApiSchema<"UpdateBankRequest">;
export type MemberQuestion = ApiSchema<"MemberQuestion">;
export type AdminUserPage = ApiSchema<"AdminUserPage">;
export type AdminUserView = ApiSchema<"AdminUserView">;
export type UpdateAdminUserRequest = ApiSchema<"UpdateAdminUserRequest">;
export type UserRole = ApiSchema<"UserRole">;

/** Query string of GET /admin/questions. Leave a field out to match anything. */
export interface QuestionSearch {
  track?: ApiSchema<"ExamTrack">;
  status?: VersionStatus;
  itemType?: ApiSchema<"ItemType">;
  clientNeedsId?: number;
  q?: string;
  questionStatus?: ApiSchema<"QuestionStatus">;
  cursor?: string;
}

export const adminKeys = {
  questions: (search: QuestionSearch) => ["admin", "questions", search] as const,
  question: (id: string) => ["admin", "question", id] as const,
  testSets: ["admin", "test-sets"] as const,
  testSet: (id: string) => ["admin", "test-set", id] as const,
  banks: ["admin", "banks"] as const,
  bank: (id: string) => ["admin", "bank", id] as const,
  users: (q: string) => ["admin", "users", q] as const,
};

function unwrap<T>(result: { data?: T; error?: unknown; response: Response }): T {
  if (result.data !== undefined) return result.data;
  throw ApiError.from(result.response, result.error);
}

// Questions and the review workflow (api.md section 8)

export async function searchQuestions(search: QuestionSearch): Promise<QuestionSearchPage> {
  return unwrap(await apiClient.GET("/api/v1/admin/questions", { params: { query: search } }));
}

export async function fetchQuestion(id: string): Promise<AdminQuestionView> {
  return unwrap(await apiClient.GET("/api/v1/admin/questions/{id}", { params: { path: { id } } }));
}

export async function createQuestion(body: ApiSchema<"CreateQuestionRequest">): Promise<AdminQuestionView> {
  return unwrap(await apiClient.POST("/api/v1/admin/questions", { body }));
}

export async function createVersion(questionId: string): Promise<AdminQuestionView> {
  return unwrap(await apiClient.POST("/api/v1/admin/questions/{id}/versions", { params: { path: { id: questionId } } }));
}

export async function retireQuestion(questionId: string): Promise<AdminQuestionView> {
  return unwrap(await apiClient.POST("/api/v1/admin/questions/{id}/retire", { params: { path: { id: questionId } } }));
}

export async function updateDraft(versionId: string, body: DraftInput): Promise<AdminQuestionView> {
  return unwrap(await apiClient.PUT("/api/v1/admin/question-versions/{id}", { params: { path: { id: versionId } }, body }));
}

/** The workflow steps that take only the version id, or notes for a review decision. */
export type WorkflowStep = "submit-for-review" | "approve" | "reject" | "publish";

export async function runWorkflowStep(versionId: string, step: WorkflowStep, notes?: string): Promise<AdminQuestionView> {
  const params = { path: { id: versionId } };
  switch (step) {
    case "submit-for-review":
      return unwrap(await apiClient.POST("/api/v1/admin/question-versions/{id}/submit-for-review", { params }));
    case "approve":
      return unwrap(await apiClient.POST("/api/v1/admin/question-versions/{id}/approve", { params, body: { notes: notes ?? null } }));
    case "reject":
      return unwrap(await apiClient.POST("/api/v1/admin/question-versions/{id}/reject", { params, body: { notes: notes ?? null } }));
    case "publish":
      return unwrap(await apiClient.POST("/api/v1/admin/question-versions/{id}/publish", { params }));
  }
}

/** "Test the key": grades a sample answer against a saved version. Nothing is stored. */
export async function previewGrade(versionId: string, response: unknown): Promise<ItemGrade> {
  return unwrap(
    await apiClient.POST("/api/v1/admin/question-versions/{id}/preview-grade", {
      params: { path: { id: versionId } },
      body: { response: response as ApiSchema<"JsonElement"> },
    }),
  );
}

// Media

/**
 * Uploads a file the way a presigned URL works: ask for a signed link, then PUT the bytes there
 * without a token. Returns the new asset's id, to use as mediaId.
 */
export async function uploadMedia(file: File): Promise<string> {
  const link = unwrap(
    await apiClient.POST("/api/v1/admin/media/upload-url", { body: { mimeType: file.type, bytes: file.size } }),
  );
  const response = await fetch(link.uploadUrl, { method: "PUT", body: file, headers: { "Content-Type": file.type } });
  if (!response.ok) throw ApiError.from(response, await response.json().catch(() => null));
  return link.mediaId;
}

export async function updateMedia(id: string, body: UpdateMediaRequest): Promise<AdminMediaView> {
  return unwrap(await apiClient.PATCH("/api/v1/admin/media/{id}", { params: { path: { id } }, body }));
}

// Curated test sets and banks (admin only)

export async function listTestSets(): Promise<AdminTestSetPage> {
  return unwrap(await apiClient.GET("/api/v1/admin/test-sets", { params: { query: { limit: 100 } } }));
}

export async function fetchTestSet(id: string): Promise<AdminTestSetView> {
  return unwrap(await apiClient.GET("/api/v1/admin/test-sets/{id}", { params: { path: { id } } }));
}

export async function createTestSet(body: CreateTestSetRequest): Promise<AdminTestSetView> {
  return unwrap(await apiClient.POST("/api/v1/admin/test-sets", { body }));
}

export async function updateTestSet(id: string, body: UpdateTestSetRequest): Promise<AdminTestSetView> {
  return unwrap(await apiClient.PATCH("/api/v1/admin/test-sets/{id}", { params: { path: { id } }, body }));
}

export async function listBanks(): Promise<AdminBankPage> {
  return unwrap(await apiClient.GET("/api/v1/admin/question-banks", { params: { query: { limit: 100 } } }));
}

export async function fetchBank(id: string): Promise<AdminBankView> {
  return unwrap(await apiClient.GET("/api/v1/admin/question-banks/{id}", { params: { path: { id } } }));
}

export async function createBank(body: CreateBankRequest): Promise<AdminBankView> {
  return unwrap(await apiClient.POST("/api/v1/admin/question-banks", { body }));
}

export async function updateBank(id: string, body: UpdateBankRequest): Promise<AdminBankView> {
  return unwrap(await apiClient.PATCH("/api/v1/admin/question-banks/{id}", { params: { path: { id } }, body }));
}

// Users (admin only)

export async function listUsers(q: string): Promise<AdminUserPage> {
  return unwrap(await apiClient.GET("/api/v1/admin/users", { params: { query: { q: q || undefined, limit: 50 } } }));
}

export async function updateUser(id: string, body: UpdateAdminUserRequest): Promise<AdminUserView> {
  return unwrap(await apiClient.PATCH("/api/v1/admin/users/{id}", { params: { path: { id } }, body }));
}
