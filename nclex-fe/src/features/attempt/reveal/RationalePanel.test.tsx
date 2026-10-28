import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { ItemReveal } from "../api/attemptApi";
import { outcomeText, RationalePanel } from "./RationalePanel";

function reveal(points: number, maxPoints: number, isCorrect: boolean): ItemReveal {
  return {
    answerKey: { correct: "b" },
    grade: {
      points,
      maxPoints,
      isCorrect,
      detail: [
        { target: "a", delta: 0, selected: "a", missed: false },
        { target: "b", delta: 0, missed: true },
      ],
    },
    rationale: { correct: "B protects the airway first.", perOption: { a: "A can wait.", b: "B is the priority." } },
    educationalObjective: "Airway comes first.",
    tags: { clientNeeds: "SECE.MOC", bodySystem: "fundamentals", cjmmSkill: null, difficulty: "easy" },
  };
}

describe("RationalePanel", () => {
  it.each([
    [1, 1, true, "Correct: 1 of 1 point"],
    [1, 3, false, "Partially correct: 1 of 3 points"],
    [0, 1, false, "Incorrect: 0 of 1 point"],
    [1.5, 4, false, "Partially correct: 1.50 of 4 points"],
  ])("describes %s of %s points as the outcome", (points, max, isCorrect, text) => {
    expect(outcomeText(reveal(points, max, isCorrect))).toBe(text);
  });

  it("explains the answer, every option with its text and mark, and the objective", () => {
    const content = { options: [{ id: "a", text: "Reposition the client" }, { id: "b", text: "Open the airway" }] };

    render(<RationalePanel reveal={reveal(0, 1, false)} content={content} />);

    expect(screen.getByRole("heading", { name: "Incorrect: 0 of 1 point" })).toBeInTheDocument();
    expect(screen.getByText("B protects the airway first.")).toBeInTheDocument();
    // A wrong multiple-choice pick scores 0 rather than -1, and is still marked incorrect.
    expect(screen.getByText("Reposition the client")).toHaveTextContent("Reposition the client (✗ Incorrect)");
    expect(screen.getByText("Open the airway")).toHaveTextContent("(Correct answer, not selected)");
    expect(screen.getByText("B is the priority.")).toBeInTheDocument();
    expect(screen.getByText(/Airway comes first\./)).toBeInTheDocument();
  });

  it("falls back to option ids when the content has no options list", () => {
    render(<RationalePanel reveal={reveal(1, 1, true)} content={{ blanks: [] }} />);

    expect(screen.getByText("A can wait.").previousSibling).toHaveTextContent("a");
  });
});
