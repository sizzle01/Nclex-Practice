import { describe, expect, it } from "vitest";
import { builderSchema, defaultBuilderValues } from "./schema";

describe("builderSchema", () => {
  it("accepts the defaults", () => {
    expect(builderSchema.safeParse(defaultBuilderValues).success).toBe(true);
  });

  it.each([4, 151, Number.NaN, 7.5])("rejects %s questions", (questionCount) => {
    expect(builderSchema.safeParse({ ...defaultBuilderValues, questionCount }).success).toBe(false);
  });

  it("needs a timer when the test is timed", () => {
    const result = builderSchema.safeParse({ ...defaultBuilderValues, timed: true, timeBudgetMin: Number.NaN });

    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(["timeBudgetMin"]);
  });

  it("accepts a time per question on its own", () => {
    expect(builderSchema.safeParse({ ...defaultBuilderValues, timed: true, perQuestionSec: 90 }).success).toBe(true);
  });
});
