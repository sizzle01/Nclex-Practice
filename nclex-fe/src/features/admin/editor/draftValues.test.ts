import { describe, expect, it } from "vitest";
import { mcVersion } from "../test/adminFixtures";
import { draftSchema, formValuesFrom, toDraftInput } from "./draftValues";

describe("draft values", () => {
  it("round-trips a stored version into the PUT body", () => {
    const version = mcVersion("draft", { rationale: { correct: "Because b.", perOption: { a: "A", b: "B", c: "C", d: "D" }, references: ["Guideline 2024"] } });

    const values = draftSchema.parse(formValuesFrom(version));
    const body = toDraftInput(values);

    expect(body).toMatchObject({
      itemType: "multiple_choice",
      stem: version.stem,
      content: version.content,
      answerKey: { correct: "b" },
      rationale: { correct: "Because b.", perOption: { a: "A", b: "B", c: "C", d: "D" }, references: ["Guideline 2024"] },
      clientNeedsId: 2,
      bodySystemId: 1,
      cjmmSkill: null,
      difficulty: "easy",
      timeAllotmentSec: 120,
    });
  });

  it("reports missing required fields with readable messages", () => {
    const values = { ...formValuesFrom(mcVersion("draft")), stem: " ", clientNeedsId: "" };

    const result = draftSchema.safeParse(values);

    expect(result.success).toBe(false);
    const messages = result.error!.issues.map((i) => i.message);
    expect(messages).toContain("Write the question stem.");
    expect(messages).toContain("Choose a Client Needs subcategory.");
  });
});
