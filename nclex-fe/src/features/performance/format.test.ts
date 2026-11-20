import { describe, expect, it } from "vitest";
import { bucketLabel, bucketTick, performanceBand } from "./format";

describe("bucket labels", () => {
  it("names the week by its local start date, never the day before", () => {
    // Read as an ISO instant this would be UTC midnight, i.e. 13 September west of Greenwich.
    const expected = new Date(2026, 8, 14).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });

    expect(bucketLabel("2026-09-14", "week")).toBe(`Week of ${expected}`);
    expect(bucketLabel("2026-09-14", "day")).toBe(expected);
  });

  it("keeps axis ticks short", () => {
    expect(bucketTick("2026-09-14")).toBe(new Date(2026, 8, 14).toLocaleDateString(undefined, { day: "numeric", month: "short" }));
  });
});

describe("performanceBand", () => {
  it.each([
    [0, "red"],
    [0.69, "red"],
    [0.694, "red"],
    [0.695, "amber"],
    [0.7, "amber"],
    [0.85, "amber"],
    [0.854, "amber"],
    [0.855, "green"],
    [0.86, "green"],
    [1, "green"],
    ["0.9", "green"],
  ] as const)("puts %s in the %s band", (share, band) => {
    expect(performanceBand(share)).toBe(band);
  });
});
