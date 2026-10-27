/**
 * Turns a `retryAfterSec` from a 423 or 429 response into words, e.g. "15 minutes".
 * Rounded up to whole minutes, because a countdown in seconds would be stale as soon as it is read.
 */
export function describeWait(seconds = 60): string {
  const minutes = Math.max(1, Math.ceil(seconds / 60));
  return minutes === 1 ? "1 minute" : `${minutes} minutes`;
}
