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
