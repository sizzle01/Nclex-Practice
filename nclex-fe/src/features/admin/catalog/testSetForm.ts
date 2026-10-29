import { z } from "zod";
import { toNumber } from "@/lib/format/toNumber";
import type { AdminTestSetView, TestSetSettings } from "../api/adminApi";

const optionalWhole = z
  .string()
  .trim()
  .refine((v) => v === "" || (/^\d+$/.test(v) && Number(v) > 0), "Use a whole number above 0, or leave it empty.");

/**
 * A curated set's settings (database.md 2.5). The timing rule matches the server's: a timed set
 * needs a per-question or a whole-exam timer, and an untimed set has neither.
 */
export const testSetSchema = z
  .object({
    track: z.enum(["RN", "PN"]),
    title: z.string().trim().min(1, "Give the set a title.").max(200, "Keep the title under 200 characters."),
    description: z.string(),
    mode: z.enum(["practice", "challenge"]),
    revealMode: z.enum(["tutor", "exam"]),
    timed: z.boolean(),
    perQuestionSec: optionalWhole,
    timeBudgetSec: optionalWhole,
    /** A percentage, e.g. "65"; empty for no pass/fail estimate. */
    passPercent: z.string().trim().refine((v) => v === "" || (Number(v) > 0 && Number(v) <= 100), "Use a percentage from 1 to 100, or leave it empty."),
    maxPauses: optionalWhole,
    maxPauseSec: optionalWhole,
    allowBackNav: z.boolean(),
    isSample: z.boolean(),
    accessTier: z.enum(["free", "premium"]),
    isPublished: z.boolean(),
  })
  .refine((s) => (s.timed ? s.perQuestionSec !== "" || s.timeBudgetSec !== "" : s.perQuestionSec === "" && s.timeBudgetSec === ""), {
    path: ["timed"],
    message: "A timed set needs a per-question or whole-set time; an untimed set has neither.",
  });

export type TestSetFormValues = z.infer<typeof testSetSchema>;

export function emptyTestSet(track: "RN" | "PN"): TestSetFormValues {
  return {
    track,
    title: "",
    description: "",
    mode: "practice",
    revealMode: "tutor",
    timed: false,
    perQuestionSec: "",
    timeBudgetSec: "",
    passPercent: "",
    maxPauses: "",
    maxPauseSec: "",
    allowBackNav: true,
    isSample: false,
    accessTier: "free",
    isPublished: false,
  };
}

export function testSetValuesFrom(set: AdminTestSetView): TestSetFormValues {
  const s = set.settings;
  const text = (v: number | string | null | undefined) => (v == null ? "" : String(toNumber(v)));
  return {
    track: set.track as "RN" | "PN",
    title: s.title,
    description: s.description ?? "",
    mode: s.mode === "challenge" ? "challenge" : "practice",
    revealMode: s.revealMode === "exam" ? "exam" : "tutor",
    timed: s.timed,
    perQuestionSec: text(s.perQuestionSec),
    timeBudgetSec: text(s.timeBudgetSec),
    passPercent: s.passThreshold == null ? "" : String(Math.round(toNumber(s.passThreshold) * 10000) / 100),
    maxPauses: text(s.pausePolicy.maxPauses),
    maxPauseSec: text(s.pausePolicy.maxPauseSec),
    allowBackNav: s.allowBackNav,
    isSample: s.isSample,
    accessTier: s.accessTier === "premium" ? "premium" : "free",
    isPublished: set.isPublished,
  };
}

export function toSettings(values: TestSetFormValues): TestSetSettings {
  const whole = (v: string) => (v === "" ? null : Number(v));
  return {
    title: values.title.trim(),
    description: values.description.trim() || null,
    mode: values.mode,
    revealMode: values.revealMode,
    timed: values.timed,
    perQuestionSec: whole(values.perQuestionSec),
    timeBudgetSec: whole(values.timeBudgetSec),
    passThreshold: values.passPercent === "" ? null : Number(values.passPercent) / 100,
    pausePolicy: { maxPauses: whole(values.maxPauses), maxPauseSec: whole(values.maxPauseSec) },
    allowBackNav: values.allowBackNav,
    isSample: values.isSample,
    accessTier: values.accessTier,
  };
}
