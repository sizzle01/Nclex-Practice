import { createStore, type StoreApi } from "zustand/vanilla";
import { toNumber } from "@/lib/format/toNumber";
import type { ClockState } from "../api/attemptApi";

/**
 * The server-synced clock (interfaces.md 5.4, AttemptClock). The server owns the time limit
 * (architecture.md 5.1): every attempt response carries `serverNow`, the client remembers
 * `offset = serverNow - Date.now()`, and shows `deadline - (Date.now() + offset)`. A wrong local
 * clock therefore changes nothing, and the display keeps counting while offline.
 */
export interface AttemptClockState {
  /** serverNow − Date.now() at the last response. */
  offsetMs: number;
  /** Call with the `serverNow` of every attempt response. */
  sync(serverNow: string): void;
  /** Time left on a countdown now: from its deadline while running, else its frozen remainder. */
  remaining(deadlineAt: string | null, fallbackMs: number): number;
}

export function createAttemptClock(now: () => number = Date.now): StoreApi<AttemptClockState> {
  return createStore<AttemptClockState>()((set, get) => ({
    offsetMs: 0,
    sync(serverNow) {
      set({ offsetMs: clockOffsetMs(serverNow, now()) });
    },
    remaining(deadlineAt, fallbackMs) {
      return remainingMs(deadlineAt, fallbackMs, get().offsetMs, now());
    },
  }));
}

export function clockOffsetMs(serverNow: string, localNow: number) {
  return Date.parse(serverNow) - localNow;
}

/** Never below zero. A stopped countdown (no deadline) shows its frozen remainder. */
export function remainingMs(deadlineAt: string | null, fallbackMs: number, offsetMs: number, localNow: number) {
  if (!deadlineAt) return Math.max(0, fallbackMs);
  return Math.max(0, Date.parse(deadlineAt) - (localNow + offsetMs));
}

/** Time left on a timer block from the API, or null when that timer does not exist. */
export function clockRemaining(clock: AttemptClockState, timer: ClockState | null | undefined) {
  return timer ? clock.remaining(timer.deadlineAt ?? null, toNumber(timer.remainingMs)) : null;
}

/** "1:02:05" or "04:59": what the timer chips show. Rounded up, so it reads 00:00 only at zero. */
export function formatCountdown(ms: number) {
  const totalSec = Math.ceil(Math.max(0, ms) / 1000);
  const hours = Math.floor(totalSec / 3600);
  const minutes = Math.floor((totalSec % 3600) / 60);
  const seconds = totalSec % 60;
  const mm = String(minutes).padStart(2, "0");
  const ss = String(seconds).padStart(2, "0");
  return hours > 0 ? `${hours}:${mm}:${ss}` : `${mm}:${ss}`;
}

/** Spoken form of a countdown, e.g. "1 hour 5 minutes", for the timer's accessible label. */
export function spokenCountdown(ms: number) {
  const totalMin = Math.ceil(Math.max(0, ms) / 60_000);
  if (totalMin <= 1) return ms <= 0 ? "no time" : "less than 1 minute";
  const hours = Math.floor(totalMin / 60);
  const minutes = totalMin % 60;
  const parts = [hours > 0 && plural(hours, "hour"), minutes > 0 && plural(minutes, "minute")];
  return parts.filter(Boolean).join(" ");
}

function plural(count: number, unit: string) {
  return `${count} ${unit}${count === 1 ? "" : "s"}`;
}

/** The screen-reader announcements: at 5 and 1 minutes left, never a running count (interfaces.md 4). */
const announcedAtMs = [5 * 60_000, 60_000] as const;

/**
 * The announcement due when a countdown moves from `previousMs` to `currentMs`, if it crossed
 * 5 or 1 minutes left; otherwise null. Opening the player with less time left than a threshold
 * does not announce it, because nothing was crossed.
 */
export function crossedAnnouncement(previousMs: number, currentMs: number): string | null {
  const crossed = announcedAtMs.filter((at) => previousMs > at && currentMs <= at);
  if (crossed.length === 0) return null;
  const at = Math.min(...crossed);
  return at === 60_000 ? "1 minute remaining" : "5 minutes remaining";
}
