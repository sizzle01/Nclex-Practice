import { z } from "zod";

// The same limits as the API (api.md section 3); the API checks them again and is the authority.
export const minQuestions = 5;
export const maxQuestions = 150;

// An emptied number input reads as NaN; treat it as "not given".
const optionalWholeNumber = (message: string) =>
  z.preprocess(
    (value) => (typeof value === "number" && Number.isNaN(value) ? undefined : value),
    z.number().int(message).min(1, message).optional(),
  );

export const builderSchema = z
  .object({
    clientNeedsIds: z.array(z.number()),
    bodySystemIds: z.array(z.number()),
    itemTypes: z.array(z.string()),
    difficulties: z.array(z.string()),
    pool: z.enum(["all", "unused", "incorrect", "flagged"]),
    questionCount: z
      .number({ error: `Enter a number from ${minQuestions} to ${maxQuestions}.` })
      .int(`Enter a whole number from ${minQuestions} to ${maxQuestions}.`)
      .min(minQuestions, `Choose at least ${minQuestions} questions.`)
      .max(maxQuestions, `Choose at most ${maxQuestions} questions.`),
    mode: z.enum(["practice", "challenge"]),
    revealMode: z.enum(["tutor", "exam"]),
    timed: z.boolean(),
    timeBudgetMin: optionalWholeNumber("Enter whole minutes, 1 or more."),
    perQuestionSec: optionalWholeNumber("Enter whole seconds, 1 or more."),
  })
  // A timed test needs at least one timer (the same rule as the database).
  .refine((v) => !v.timed || v.timeBudgetMin !== undefined || v.perQuestionSec !== undefined, {
    path: ["timeBudgetMin"],
    message: "Set a time limit for the test, a time per question, or both.",
  });

export type BuilderInput = z.input<typeof builderSchema>;
export type BuilderValues = z.output<typeof builderSchema>;

export const defaultBuilderValues: BuilderInput = {
  clientNeedsIds: [],
  bodySystemIds: [],
  itemTypes: [],
  difficulties: [],
  pool: "all",
  questionCount: 20,
  mode: "practice",
  revealMode: "tutor",
  timed: false,
  timeBudgetMin: undefined,
  perQuestionSec: undefined,
};
