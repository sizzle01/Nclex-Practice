import { z } from "zod";

/** The limit of question_flags.comment (database.md 2.9), which also caps a moderator's note. */
export const maxTextLength = 2000;

const reasons = ["rationale_unclear", "incorrect_answer_key", "typo", "outdated_guideline", "media_problem", "other"] as const;

export const reportIssueSchema = z.object({
  reason: z.enum(reasons, { error: "Choose what is wrong." }),
  comment: z.string().trim().max(maxTextLength, `Keep it to ${maxTextLength} characters.`),
});

export type ReportIssueValues = z.infer<typeof reportIssueSchema>;

export const closeFlagSchema = z.object({
  note: z
    .string()
    .trim()
    .min(1, "Write a note. For a resolved report the reporter sees it.")
    .max(maxTextLength, `Keep it to ${maxTextLength} characters.`),
});

export type CloseFlagValues = z.infer<typeof closeFlagSchema>;
