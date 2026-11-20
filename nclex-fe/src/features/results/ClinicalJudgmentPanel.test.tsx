import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { AttemptResultView } from "../attempt/api/attemptApi";
import { ClinicalJudgmentPanel } from "./ClinicalJudgmentPanel";

type ClinicalJudgment = NonNullable<AttemptResultView["clinicalJudgment"]>;

function step(key: string, name: string, pct: number, avgTimeMs = 60_000) {
  return { key, name, items: 10, pointsEarned: pct * 10, maxPoints: 10, pct, avgTimeMs };
}

const judgment: ClinicalJudgment = {
  steps: [step("recognize_cues", "Recognize cues", 0.9), step("generate_solutions", "Generate solutions", 0.6), step("evaluate_outcomes", "Evaluate outcomes", 0.75)],
  firstHalf: { items: 15, pct: 0.84, avgTimeMs: 95_000, steps: [step("recognize_cues", "Recognize cues", 0.9), step("generate_solutions", "Generate solutions", 0.8)] },
  secondHalf: { items: 15, pct: 0.6, avgTimeMs: 48_000, steps: [step("generate_solutions", "Generate solutions", 0.4), step("evaluate_outcomes", "Evaluate outcomes", 0.75)] },
  secondHalfDrop: true,
};

function rowOf(name: string) {
  return screen.getByRole("row", { name: new RegExp(`^${name}`) });
}

describe("ClinicalJudgmentPanel", () => {
  it("lists the steps in the order the server sends, which is model order", () => {
    render(<ClinicalJudgmentPanel judgment={judgment} />);

    const names = within(screen.getByRole("table")).getAllByRole("rowheader").map((cell) => cell.textContent);
    expect(names).toEqual(["Recognize cues", "Generate solutions", "Evaluate outcomes"]);
  });

  it("names each step's standard in words, not colour alone", () => {
    render(<ClinicalJudgmentPanel judgment={judgment} />);

    expect(within(rowOf("Recognize cues")).getByText("Above passing standard")).toBeInTheDocument();
    expect(within(rowOf("Generate solutions")).getByText("Below passing standard")).toBeInTheDocument();
    expect(within(rowOf("Evaluate outcomes")).getByText("Near passing standard")).toBeInTheDocument();
  });

  it("shows each step's score in each half, and says when a half had no questions on it", () => {
    render(<ClinicalJudgmentPanel judgment={judgment} />);

    const generate = within(rowOf("Generate solutions")).getAllByRole("cell").map((cell) => cell.textContent);
    expect(generate).toEqual(["10", "60% (needs work)", "Below passing standard", "80% (borderline)", "40% (needs work)", "1 min"]);
    expect(within(rowOf("Recognize cues")).getByText("No questions")).toBeInTheDocument();
  });

  it("explains a clear drop in the second half, with the time per question in each half", () => {
    render(<ClinicalJudgmentPanel judgment={judgment} />);

    expect(screen.getByRole("note")).toHaveTextContent("Your score fell from 84% in the first half to 60% in the second half.");
    expect(screen.getByText(/First half: 15 questions/)).toHaveTextContent("1 min 35 s a question");
    expect(screen.getByText(/Second half: 15 questions/)).toHaveTextContent("48 s a question");
  });

  it("says nothing about a drop when there is none", () => {
    render(<ClinicalJudgmentPanel judgment={{ ...judgment, secondHalfDrop: false }} />);

    expect(screen.queryByRole("note")).not.toBeInTheDocument();
  });
});
