import { toNumber } from "@/lib/format/toNumber";

/** A score share as a whole percentage, e.g. 0.739 becomes "74%". */
export function percent(share: number | string): string {
  return `${Math.round(toNumber(share) * 100)}%`;
}

/** Points without needless decimals: 68 stays "68", partial credit shows "1.5". */
export function points(value: number | string): string {
  const n = toNumber(value);
  return Number.isInteger(n) ? String(n) : String(Math.round(n * 100) / 100);
}

/** Time spent, e.g. "45 s", "1 min 21 s", "1 h 42 min". */
export function elapsed(ms: number | string): string {
  const totalSeconds = Math.round(toNumber(ms) / 1000);
  if (totalSeconds < 60) return `${totalSeconds} s`;
  const minutes = Math.floor(totalSeconds / 60);
  if (minutes < 60) {
    const seconds = totalSeconds % 60;
    return seconds === 0 ? `${minutes} min` : `${minutes} min ${seconds} s`;
  }
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  return rest === 0 ? `${hours} h` : `${hours} h ${rest} min`;
}
