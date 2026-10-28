import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { renderItem, renderRevealed } from "./testRenderer";

// Sample: 1,000 mL over 8 hours, unit mL/hr, 0 decimals, input label "Infusion rate"; answer 125.
describe("fill in the blank (calculation)", () => {
  it("is a decimal text box with the unit and decimals in its description", () => {
    renderItem("fill_blank_calc");

    const input = screen.getByRole("textbox", { name: "Infusion rate" });
    expect(input).toHaveAttribute("inputmode", "decimal");
    expect(input).toHaveAccessibleDescription("Answer in mL/hr, as a whole number.");
    expect(screen.getByText("mL/hr")).toBeInTheDocument();
  });

  it("describes decimal places", () => {
    renderItem("fill_blank_calc", { content: { unit: "mg", decimals: 1 } });

    expect(screen.getByRole("textbox", { name: "Your answer" })).toHaveAccessibleDescription(
      "Answer in mg, to 1 decimal place.",
    );
  });

  it("saves the raw text typed, which the server parses", async () => {
    const { onChange } = renderItem("fill_blank_calc");

    await userEvent.tab();
    await userEvent.keyboard("7");

    expect(onChange).toHaveBeenLastCalledWith({ value: "7" });
  });

  it("clears the answer when the box is emptied", async () => {
    const { onChange } = renderItem("fill_blank_calc", { response: { value: "1" } });

    await userEvent.clear(screen.getByRole("textbox"));

    expect(onChange).toHaveBeenLastCalledWith(null);
  });

  it("gives the right answer with its unit after a wrong reveal", () => {
    renderRevealed("fill_blank_calc");

    expect(screen.getByText("✗ Incorrect")).toBeInTheDocument();
    expect(screen.getByText(/Correct answer: 125 mL\/hr/)).toBeInTheDocument();
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("fill_blank_calc", { response: { value: "125" } });

    await expectNoAxeViolations(container);
  });
});
