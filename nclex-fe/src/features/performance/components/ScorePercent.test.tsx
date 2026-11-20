import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ScorePercent } from "./ScorePercent";

describe("ScorePercent", () => {
  it.each([
    [0.62, "62%", "red", "needs work"],
    [0.78, "78%", "amber", "borderline"],
    [0.91, "91%", "green", "strong"],
  ])("shows %s as %s in the %s band and names the band for screen readers", (value, shown, band, label) => {
    render(<ScorePercent value={value} />);

    const score = screen.getByText(shown, { exact: false });
    expect(score).toHaveAttribute("data-band", band);
    expect(score).toHaveTextContent(`${shown} (${label})`);
  });
});
