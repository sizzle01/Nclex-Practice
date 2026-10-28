import { describe, expect, it } from "vitest";
import {
  clockOffsetMs,
  createAttemptClock,
  crossedAnnouncement,
  formatCountdown,
  remainingMs,
  spokenCountdown,
} from "./attemptClock";

const serverNow = "2026-10-28T09:00:00.000Z";
const deadline = "2026-10-28T09:10:00.000Z";

describe("the attempt clock", () => {
  it("counts from the server's time even when the local clock is hours off", () => {
    const localNow = Date.parse("2026-10-28T13:30:00Z"); // 4.5 hours fast
    const clock = createAttemptClock(() => localNow);

    clock.getState().sync(serverNow);

    expect(clock.getState().offsetMs).toBe(-4.5 * 3_600_000);
    expect(clock.getState().remaining(deadline, 0)).toBe(10 * 60_000);
  });

  it("keeps counting down as local time passes, with no further server response", () => {
    let localNow = Date.parse(serverNow);
    const clock = createAttemptClock(() => localNow);
    clock.getState().sync(serverNow);

    localNow += 4 * 60_000;

    expect(clock.getState().remaining(deadline, 0)).toBe(6 * 60_000);
  });

  it("shows a stopped timer's frozen time, and never goes below zero", () => {
    expect(remainingMs(null, 123_000, 0, Date.now())).toBe(123_000);
    expect(remainingMs(deadline, 0, 0, Date.parse("2026-10-28T10:00:00Z"))).toBe(0);
    expect(clockOffsetMs(serverNow, Date.parse(serverNow) + 500)).toBe(-500);
  });
});

describe("countdown text", () => {
  it.each([
    [0, "00:00"],
    [400, "00:01"], // rounds up, so 00:00 means the time is really up
    [59_000, "00:59"],
    [5 * 60_000, "05:00"],
    [3_725_000, "1:02:05"],
  ])("formats %i ms as %s", (ms, text) => {
    expect(formatCountdown(ms)).toBe(text);
  });

  it.each([
    [0, "no time"],
    [30_000, "less than 1 minute"],
    [5 * 60_000, "5 minutes"],
    [61 * 60_000, "1 hour 1 minute"],
    [120 * 60_000, "2 hours"],
  ])("speaks %i ms as %s", (ms, text) => {
    expect(spokenCountdown(ms)).toBe(text);
  });
});

describe("screen-reader announcements", () => {
  it("speaks when crossing 5 minutes and 1 minute left", () => {
    expect(crossedAnnouncement(300_400, 299_400)).toBe("5 minutes remaining");
    expect(crossedAnnouncement(60_900, 59_900)).toBe("1 minute remaining");
  });

  it("stays quiet between thresholds, so the count never floods the reader", () => {
    expect(crossedAnnouncement(250_000, 249_000)).toBeNull();
    expect(crossedAnnouncement(299_000, 298_000)).toBeNull();
  });

  it("names only the smaller threshold when both are crossed at once (after sleep or a slow tab)", () => {
    expect(crossedAnnouncement(400_000, 30_000)).toBe("1 minute remaining");
  });
});
