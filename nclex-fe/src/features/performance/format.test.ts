import { describe, expect, it } from "vitest";
import { bucketLabel, bucketTick } from "./format";

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
