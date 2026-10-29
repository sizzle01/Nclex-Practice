import { z } from "zod";
import type { Exhibit } from "@/features/player/exhibits/exhibitTypes";
import { parseExhibits } from "@/features/player/exhibits/exhibitTypes";
import { toNumber } from "@/lib/format/toNumber";
import type { AdminVersionView, DraftInput } from "../api/adminApi";
import { type AuthorableType, type ItemParts } from "../item-forms/payloads";
import { emptyItem } from "../item-forms/templates";

const difficulties = ["easy", "moderate", "difficult"] as const;

/**
 * The draft editor's fields (react-hook-form + Zod). Simple checks run here for quick feedback; the
 * server checks everything else on save (database.md 2.4) and its problems are shown by field.
 */
export const draftSchema = z.object({
  itemType: z.custom<AuthorableType>(),
  stem: z.string().trim().min(1, "Write the question stem."),
  educationalObjective: z.string().trim().min(1, "Write the educational objective."),
  rationaleCorrect: z.string().trim().min(1, "Explain why the correct answer is correct."),
  /** One reference per line. */
  references: z.string(),
  clientNeedsId: z.coerce.number<string>().int().positive("Choose a Client Needs subcategory."),
  bodySystemId: z.coerce.number<string>().int().positive("Choose a body system."),
  /** "" for none. */
  cjmmSkill: z.string(),
  difficulty: z.enum(difficulties, { message: "Choose a difficulty." }),
  timeAllotmentSec: z.coerce.number<string>().int().positive("Use a whole number of seconds above 0."),
  item: z.custom<ItemParts>(),
  perOption: z.record(z.string(), z.string()),
  exhibits: z.custom<Exhibit[]>(),
});

export type DraftFormInput = z.input<typeof draftSchema>;
export type DraftFormValues = z.output<typeof draftSchema>;

/** The form's fields for a stored version. */
export function formValuesFrom(version: AdminVersionView): DraftFormInput {
  const rationale = (version.rationale ?? {}) as { correct?: string; perOption?: Record<string, string>; references?: string[] };
  return {
    itemType: version.itemType as AuthorableType,
    stem: version.stem,
    educationalObjective: version.educationalObjective,
    rationaleCorrect: rationale.correct ?? "",
    references: (rationale.references ?? []).join("\n"),
    clientNeedsId: String(version.clientNeedsId),
    bodySystemId: String(version.bodySystemId),
    cjmmSkill: version.cjmmSkill ?? "",
    difficulty: version.difficulty,
    timeAllotmentSec: String(toNumber(version.timeAllotmentSec)),
    item: { content: version.content, answerKey: version.answerKey } as ItemParts,
    perOption: rationale.perOption ?? {},
    exhibits: parseExhibits(version.exhibits),
  };
}

/** Empty fields for a new question of <paramref name="itemType"/>. */
export function emptyFormValues(itemType: AuthorableType): DraftFormInput {
  return {
    itemType,
    stem: "",
    educationalObjective: "",
    rationaleCorrect: "",
    references: "",
    clientNeedsId: "",
    bodySystemId: "",
    cjmmSkill: "",
    difficulty: "moderate",
    timeAllotmentSec: "120",
    item: emptyItem(itemType),
    perOption: {},
    exhibits: [],
  };
}

/** The body of PUT /admin/question-versions/{id} (and the draft of POST /admin/questions). */
export function toDraftInput(values: DraftFormValues): DraftInput {
  const references = values.references.split("\n").map((r) => r.trim()).filter(Boolean);
  return {
    itemType: values.itemType,
    stem: values.stem.trim(),
    content: values.item.content as DraftInput["content"],
    answerKey: values.item.answerKey as DraftInput["answerKey"],
    rationale: {
      correct: values.rationaleCorrect.trim(),
      perOption: values.perOption,
      ...(references.length > 0 ? { references } : {}),
    } as DraftInput["rationale"],
    educationalObjective: values.educationalObjective.trim(),
    exhibits: values.exhibits as DraftInput["exhibits"],
    clientNeedsId: values.clientNeedsId,
    bodySystemId: values.bodySystemId,
    cjmmSkill: (values.cjmmSkill || null) as DraftInput["cjmmSkill"],
    difficulty: values.difficulty,
    timeAllotmentSec: values.timeAllotmentSec,
  };
}

/** Which form field a server error path belongs to, for the fields shown as plain inputs. */
export const serverFieldMap: Record<string, keyof DraftFormInput> = {
  stem: "stem",
  educationalObjective: "educationalObjective",
  "rationale.correct": "rationaleCorrect",
  clientNeedsId: "clientNeedsId",
  bodySystemId: "bodySystemId",
  difficulty: "difficulty",
  timeAllotmentSec: "timeAllotmentSec",
};
