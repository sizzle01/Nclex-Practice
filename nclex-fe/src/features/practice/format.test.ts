import { describe, expect, it } from "vitest";
import { formatDuration, pausePolicyLabel, percentLabel, questionCountLabel, timingLabel } from "./format";

describe("practice formatting", () => {
  it.each([
    [45, "45 seconds"],
    [60, "1 minute"],
    [4800, "80 minutes"],
    [7200, "2 h"],
    [9000, "2 h 30 min"],
    ["4800", "80 minutes"],
  ])("formats %s seconds as %s", (seconds, label) => {
    expect(formatDuration(seconds)).toBe(label);
  });

  it("describes untimed, exam-timed and item-timed sets", () => {
    expect(timingLabel({ timed: false, timeBudgetSec: null, perQuestionSec: null })).toBe("Untimed");
    expect(timingLabel({ timed: true, timeBudgetSec: 4800, perQuestionSec: null })).toBe("80 minutes");
    expect(timingLabel({ timed: true, timeBudgetSec: null, perQuestionSec: 90 })).toBe("90 s per question");
    expect(timingLabel({ timed: true, timeBudgetSec: 4800, perQuestionSec: 90 })).toBe("80 minutes, 90 s per question");
  });

  it("shows counts, percentages and pause limits in words", () => {
    expect(questionCountLabel(1)).toBe("1 question");
    expect(questionCountLabel(40)).toBe("40 questions");
    expect(percentLabel(0.65)).toBe("65%");
    expect(pausePolicyLabel({ maxPauses: null, maxPauseSec: null })).toBe("Unlimited");
    expect(pausePolicyLabel({ maxPauses: 1, maxPauseSec: null })).toBe("Up to 1 pause");
    expect(pausePolicyLabel({ maxPauses: 2, maxPauseSec: 300 })).toBe("Up to 2 pauses of 5 minutes each");
    expect(pausePolicyLabel({ maxPauses: null, maxPauseSec: 600 })).toBe("Any number of pauses of 10 minutes each");
  });
});
