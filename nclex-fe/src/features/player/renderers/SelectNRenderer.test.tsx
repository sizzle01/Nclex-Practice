import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { renderItem, renderRevealed } from "./testRenderer";

// Sample: thyroidectomy, select 3 of options a–f.
describe("select N", () => {
  it("says how many to select in the group's description", () => {
    renderItem("select_n", { response: { selected: ["a"] } });

    expect(screen.getByRole("group", { name: "Stem" })).toHaveAccessibleDescription("Select 3. 1 of 3 selected.");
  });

  it("answers from the keyboard in content order", async () => {
    const { onChange } = renderItem("select_n", { response: { selected: ["c"] } });
    screen.getByRole("checkbox", { name: "Tingling around the mouth and in the fingertips" }).focus();

    await userEvent.keyboard(" ");

    expect(onChange).toHaveBeenCalledWith({ selected: ["a", "c"] });
  });

  it("disables the other options once N are selected, and says why", () => {
    renderItem("select_n", { response: { selected: ["a", "c", "e"] } });

    expect(screen.getByRole("checkbox", { name: "Mild hoarseness when speaking" })).toBeDisabled();
    expect(screen.getByRole("checkbox", { name: "High-pitched sound on inspiration" })).toBeEnabled();
    expect(screen.getByText(/You have selected 3; untick one/)).toBeInTheDocument();
  });

  it("marks the revealed answer", () => {
    renderRevealed("select_n");

    // Answered a, c and b; the key is a, c and e.
    expect(screen.getAllByText("✓ Correct")).toHaveLength(2);
    expect(screen.getAllByText("✗ Incorrect")).toHaveLength(1);
    expect(screen.getAllByText("Correct answer, not selected")).toHaveLength(1);
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("select_n", { response: { selected: ["a"] } });

    await expectNoAxeViolations(container);
  });
});
