import { describe, expect, it } from "vitest";
import { describeWait } from "./retryAfter";

describe("describeWait", () => {
  it("rounds up to whole minutes", () => {
    expect(describeWait(900)).toBe("15 minutes");
    expect(describeWait(61)).toBe("2 minutes");
  });

  it("never says less than a minute", () => {
    expect(describeWait(1)).toBe("1 minute");
    expect(describeWait()).toBe("1 minute");
  });
});
