import type { TestSetDetail, TestSetSummary } from "./api/practiceApi";

type Mode = TestSetSummary["mode"];
type RevealMode = TestSetSummary["revealMode"];

// The generated schema allows numbers as strings too (ASP.NET Core's OpenAPI output), so values
// are converted before doing maths with them.
function toNumber(value: number | string): number {
  return typeof value === "number" ? value : Number(value);
}

export function modeLabel(mode: Mode): string {
  switch (mode) {
    case "practice":
      return "Practice";
    case "challenge":
      return "Challenge";
    case "cat":
      return "Adaptive";
  }
}

export function revealModeLabel(revealMode: RevealMode): string {
  return revealMode === "tutor" ? "Answers after each question" : "Answers at the end";
}

export function questionCountLabel(count: number | string): string {
  const n = toNumber(count);
  return n === 1 ? "1 question" : `${n} questions`;
}

/**
 * A duration as people say it: "45 seconds", "80 minutes" (up to two hours, as exam timers are
 * usually quoted), then "2 h 30 min".
 */
export function formatDuration(seconds: number | string): string {
  const totalSeconds = toNumber(seconds);
  if (totalSeconds < 60) return `${totalSeconds} seconds`;
  const minutes = Math.round(totalSeconds / 60);
  if (minutes < 120) return minutes === 1 ? "1 minute" : `${minutes} minutes`;
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  return rest === 0 ? `${hours} h` : `${hours} h ${rest} min`;
}

/** What the card says about time: "Untimed", "80 minutes", or "90 s per question". */
export function timingLabel(set: Pick<TestSetSummary, "timed" | "timeBudgetSec" | "perQuestionSec">): string {
  if (!set.timed) return "Untimed";
  const parts: string[] = [];
  if (set.timeBudgetSec != null) parts.push(formatDuration(set.timeBudgetSec));
  if (set.perQuestionSec != null) parts.push(`${toNumber(set.perQuestionSec)} s per question`);
  return parts.join(", ");
}

/** The pass threshold as a whole percentage, e.g. 0.65 becomes "65%". */
export function percentLabel(share: number | string): string {
  return `${Math.round(toNumber(share) * 100)}%`;
}

/** The pause limits in words, e.g. "Unlimited" or "Up to 2 pauses of 5 minutes each". */
export function pausePolicyLabel(policy: TestSetDetail["pausePolicy"]): string {
  const { maxPauses, maxPauseSec } = policy;
  if (maxPauses == null && maxPauseSec == null) return "Unlimited";
  const count = maxPauses == null ? "Any number of pauses" : `Up to ${toNumber(maxPauses)} pause${toNumber(maxPauses) === 1 ? "" : "s"}`;
  return maxPauseSec == null ? count : `${count} of ${formatDuration(maxPauseSec)} each`;
}
