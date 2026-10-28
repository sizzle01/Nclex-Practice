import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { renderItem, renderRevealed } from "./testRenderer";

// Sample: hip arthroplasty, groups g1 Circulation (a–c), g2 Respiratory (d–f), g3 Operative hip (g–i).
describe("multiple response grouping", () => {
  it("shows each group as a named group of checkboxes", () => {
    renderItem("multiple_response_grouping");

    const circulation = screen.getByRole("group", { name: "Circulation" });
    expect(within(circulation).getAllByRole("checkbox")).toHaveLength(3);
  });

  it("answers per group from the keyboard", async () => {
    const { onChange } = renderItem("multiple_response_grouping", { response: { byGroup: { g1: ["b"] } } });
    screen.getByRole("checkbox", { name: "Sudden shortness of breath and chest pain" }).focus();

    await userEvent.keyboard(" ");

    expect(onChange).toHaveBeenCalledWith({ byGroup: { g1: ["b"], g2: ["d"] } });
  });

  it("drops a group whose last option is unticked", async () => {
    const { onChange } = renderItem("multiple_response_grouping", { response: { byGroup: { g1: ["b"], g2: ["d"] } } });

    await userEvent.click(screen.getByRole("checkbox", { name: "Cool, pale foot on the operative side" }));

    expect(onChange).toHaveBeenCalledWith({ byGroup: { g2: ["d"] } });
  });

  it("has a strike tool for each option", async () => {
    const { onToggleStrike } = renderItem("multiple_response_grouping");

    await userEvent.click(screen.getByRole("button", { name: "Strike out Clear breath sounds" }));

    expect(onToggleStrike).toHaveBeenCalledWith("f");
  });

  it("marks the revealed answer", () => {
    renderRevealed("multiple_response_grouping");

    expect(screen.getAllByText("✗ Incorrect")).toHaveLength(1);
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("multiple_response_grouping", { response: { byGroup: { g1: ["b"] } } });

    await expectNoAxeViolations(container);
  });
});
