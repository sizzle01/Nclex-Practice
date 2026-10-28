import { describe, expect, it } from "vitest";
import type { GradeDetail, ItemReveal } from "@/features/attempt/api/attemptApi";
import { choiceLabels } from "./choiceLabels";
import { choiceMarks } from "./revealMarks";

function revealWith(detail: Partial<GradeDetail>[]): ItemReveal {
  return {
    answerKey: {},
    grade: {
      points: 0,
      maxPoints: 1,
      isCorrect: false,
      detail: detail.map((d) => ({ missed: false, delta: 0, target: "", ...d })),
    },
    rationale: { correct: "", perOption: {} },
    educationalObjective: "",
    tags: { clientNeeds: "", bodySystem: "", cjmmSkill: null, difficulty: "easy" },
  };
}

describe("choiceMarks", () => {
  it("marks the chosen and the expected choice inside a graded slot", () => {
    const reveal = revealWith([
      { target: "b1", delta: 0, selected: "b", expected: "a", missed: true },
      { target: "b2", delta: 1, selected: "c", expected: "c" },
      { target: "b3", delta: 0, expected: "a", missed: true },
    ]);

    expect(choiceMarks("dropdown_cloze", reveal)).toEqual({
      "b1:b": "incorrect",
      "b1:a": "missed",
      "b2:c": "correct",
      "b3:a": "missed",
    });
  });

  it("marks matrix multiple choice cells by row", () => {
    const reveal = revealWith([{ target: "r1", delta: 0, selected: "c2", expected: "c1", missed: true }]);

    expect(choiceMarks("matrix_mc", reveal)).toEqual({ "r1:c2": "incorrect", "r1:c1": "missed" });
  });

  it("marks ordered items right or wrong for their position", () => {
    const reveal = revealWith([
      { target: "a", delta: 0, selected: "b", expected: "a", missed: true },
      { target: "b", delta: 0, selected: "a", expected: "b", missed: true },
      { target: "c", delta: 1, selected: "c", expected: "c" },
    ]);

    expect(choiceMarks("ordered_response", reveal)).toEqual({ a: "incorrect", b: "incorrect", c: "correct" });
  });

  it("uses the target marks for types graded per choice", () => {
    const reveal = revealWith([
      { target: "r1:c1", delta: 1, selected: "r1:c1" },
      { target: "r1:c2", delta: -1, selected: "r1:c2" },
      { target: "r2:c1", delta: 0, missed: true },
    ]);

    expect(choiceMarks("matrix_mr", reveal)).toEqual({ "r1:c1": "correct", "r1:c2": "incorrect", "r2:c1": "missed" });
  });
});

describe("choiceLabels", () => {
  it("names matrix cells by row and column", () => {
    const labels = choiceLabels("matrix_mr", {
      rows: [{ id: "r1", text: "Warfarin" }],
      columns: [{ id: "c1", text: "Bleeding" }],
      groupAxis: "row",
    });

    expect(labels.get("r1:c1")).toBe("Warfarin: Bleeding");
  });

  it("names drop-down choices by blank number", () => {
    const labels = choiceLabels("dropdown_cloze", {
      template: "{{b1}}",
      blanks: [{ id: "b1", options: [{ id: "a", text: "hypoglycemia" }] }],
    });

    expect(labels.get("b1:a")).toBe("Blank 1: hypoglycemia");
  });

  it("names grouping options with their group and highlight rows by their cells", () => {
    expect(
      choiceLabels("multiple_response_grouping", {
        groups: [{ id: "g1", label: "Circulation", options: [{ id: "a", text: "Cool foot" }] }],
      }).get("a"),
    ).toBe("Circulation: Cool foot");
    expect(
      choiceLabels("highlight_table", {
        columns: [],
        rows: [{ id: "r1", cells: ["30 min", "Chills"], selectable: true }],
      }).get("r1"),
    ).toBe("30 min, Chills");
  });
});
