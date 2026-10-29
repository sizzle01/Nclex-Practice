import type { UserProfile } from "@/features/auth/api/authApi";
import type { AdminQuestionView, AdminVersionView } from "../api/adminApi";

/** A signed-in staff member with <paramref name="role"/>. */
export function staffUser(role: UserProfile["role"], id = "0192f0a0-0000-7000-8000-00000000000a"): UserProfile {
  return {
    id,
    email: `${role}@example.com`,
    emailVerified: true,
    role,
    track: "RN",
    planTier: "free",
    displayName: null,
    timezone: "UTC",
    targetExamDate: null,
  };
}

export const authorId = "0192f0a0-0000-7000-8000-0000000000a1";
export const reviewerId = "0192f0a0-0000-7000-8000-0000000000b1";

/** A multiple-choice version (options a to d, key b) in <paramref name="status"/>. */
export function mcVersion(status: AdminVersionView["status"], overrides: Partial<AdminVersionView> = {}): AdminVersionView {
  return {
    id: "0192f0a0-0000-7000-8000-0000000000c1",
    version: 1,
    status,
    itemType: "multiple_choice",
    stem: "Which client should the nurse see first?",
    content: { options: ["a", "b", "c", "d"].map((id) => ({ id, text: `Option ${id}` })) },
    answerKey: { correct: "b" },
    rationale: { correct: "Because b.", perOption: { a: "Why a", b: "Why b", c: "Why c", d: "Why d" } },
    educationalObjective: "Prioritise airway.",
    exhibits: [],
    clientNeedsId: 2,
    bodySystemId: 1,
    cjmmSkill: null,
    difficulty: "easy",
    timeAllotmentSec: 120,
    maxPoints: 1,
    caseStudyId: null,
    casePosition: null,
    authorId,
    authorName: "Avery Author",
    reviewerId: null,
    reviewerName: null,
    reviewNotes: null,
    reviewedAt: null,
    publishedAt: null,
    createdAt: "2026-10-28T09:00:00Z",
    ...overrides,
  };
}

export function question(versions: AdminVersionView[], overrides: Partial<AdminQuestionView> = {}): AdminQuestionView {
  return {
    id: "0192f0a0-0000-7000-8000-0000000000d1",
    track: "RN",
    status: "active",
    accessTier: "free",
    currentVersionId: versions.find((v) => v.status === "published")?.id ?? null,
    createdAt: "2026-10-28T09:00:00Z",
    retiredAt: null,
    versions,
    ...overrides,
  };
}

/** An openapi-fetch result, as the mocked client returns it. */
export function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}
