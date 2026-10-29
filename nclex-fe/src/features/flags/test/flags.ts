import type { FlagQueueItem, MyFlag } from "../api/flagsApi";

export function aMyFlag(overrides: Partial<MyFlag> = {}): MyFlag {
  return {
    id: "f-1",
    questionVersionId: "v-1",
    stemPreview: "Which client should the nurse see first?",
    reason: "incorrect_answer_key",
    comment: null,
    status: "open",
    resolutionNote: null,
    attemptId: null,
    createdAt: "2026-10-28T09:00:00Z",
    resolvedAt: null,
    ...overrides,
  };
}

export function aQueueItem(overrides: Partial<FlagQueueItem> = {}): FlagQueueItem {
  return {
    id: "f-1",
    status: "open",
    reason: "rationale_unclear",
    comment: "The rationale skips a step.",
    question: { questionId: "q-1", versionId: "v-1", version: 2, itemType: "multiple_choice", stemPreview: "Which client first?" },
    reporter: { id: "u-1", displayName: "Ada" },
    assignee: null,
    resolutionNote: null,
    resolvedBy: null,
    resolvedAt: null,
    attemptId: null,
    createdAt: "2026-10-28T09:00:00Z",
    ...overrides,
  };
}

/** A GET /me profile with the given role. */
export function aProfile(role: "candidate" | "reviewer" | "admin", id = "me-1") {
  return {
    id,
    email: "me@example.com",
    emailVerified: true,
    role,
    track: "RN" as const,
    planTier: "free" as const,
    displayName: "Me",
    timezone: "UTC",
    targetExamDate: null,
  };
}
