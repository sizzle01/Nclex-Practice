import { toNumber } from "@/lib/format/toNumber";
import type { TrendBucket } from "./api/performanceApi";

const dateFormat = new Intl.DateTimeFormat(undefined, { day: "numeric", month: "short", year: "numeric" });
const shortDateFormat = new Intl.DateTimeFormat(undefined, { day: "numeric", month: "short" });

/**
 * A bucket's start date ("2026-09-14") is a calendar date in the user's time zone, not an instant,
 * so it is built from its parts: parsing it as an ISO string would read it as UTC midnight and
 * show the previous day west of Greenwich.
 */
function localDate(isoDate: string): Date {
  const [year, month, day] = isoDate.split("-").map(Number);
  return new Date(year, month - 1, day);
}

/** The full label of a bucket, e.g. "Week of 14 Sep 2026" or "14 Sep 2026". */
export function bucketLabel(isoDate: string, bucket: TrendBucket): string {
  const date = dateFormat.format(localDate(isoDate));
  return bucket === "week" ? `Week of ${date}` : date;
}

/** A short axis tick, e.g. "14 Sep". */
export function bucketTick(isoDate: string): string {
  return shortDateFormat.format(localDate(isoDate));
}

/** How a score reads at a glance: red needs work, amber is borderline, green is strong. */
export type PerformanceBand = "red" | "amber" | "green";

/** The screen-reader wording of each band, so the colour is never the only signal (interfaces.md 4). */
export const bandLabel: Record<PerformanceBand, string> = {
  red: "needs work",
  amber: "borderline",
  green: "strong",
};

/**
 * The band of a score share (0 to 1). It uses the whole percentage the user sees, so the colour
 * always agrees with the label: 70.6% shows as "71%" and is amber, not red.
 * Red is 70% and below, amber 71% to 85%, green 86% and above.
 */
export function performanceBand(share: number | string): PerformanceBand {
  const shown = Math.round(toNumber(share) * 100);
  if (shown <= 70) return "red";
  if (shown <= 85) return "amber";
  return "green";
}
