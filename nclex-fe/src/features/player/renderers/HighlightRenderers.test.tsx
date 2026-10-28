import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { renderItem, renderRevealed } from "./testRenderer";

describe("highlight text", () => {
  // Sample: postpartum note; selectable s2, s4, s6, s8; correct s2 and s4.
  it("makes only the selectable segments toggle buttons", () => {
    renderItem("highlight_text");

    expect(screen.getAllByRole("button")).toHaveLength(4);
    expect(screen.getByRole("button", { name: "Fundus is boggy and 2 cm above the umbilicus" })).toHaveAttribute(
      "aria-pressed",
      "false",
    );
  });

  it("Tabs to each segment and toggles it with Space", async () => {
    const { onChange } = renderItem("highlight_text", { response: { selected: ["s6"] } });

    await userEvent.tab();
    await userEvent.tab();
    expect(screen.getByRole("button", { name: /Lochia rubra/ })).toHaveFocus();
    await userEvent.keyboard(" ");

    expect(onChange).toHaveBeenCalledWith({ selected: ["s4", "s6"] });
  });

  it("shows highlighted segments as pressed", () => {
    renderItem("highlight_text", { response: { selected: ["s6"] } });

    expect(screen.getByRole("button", { name: "Blood pressure 118/72 mm Hg" })).toHaveAttribute("aria-pressed", "true");
  });

  it("marks the revealed answer in highlight terms", () => {
    renderRevealed("highlight_text");

    expect(screen.getByText("✓ Correct highlight")).toBeInTheDocument();
    expect(screen.getByText("✗ Should not be highlighted")).toBeInTheDocument();
    expect(screen.getByText("Should be highlighted")).toBeInTheDocument();
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("highlight_text", { response: { selected: ["s2"] } });

    await expectNoAxeViolations(container);
  });
});

describe("highlight table", () => {
  // Sample: transfusion; rows r1–r4 selectable, r5 not; correct r3 and r4.
  it("gives each selectable row a toggle named after its cells", () => {
    renderItem("highlight_table");

    expect(screen.getAllByRole("button", { name: /^Highlight row:/ })).toHaveLength(4);
    expect(
      screen.getByRole("button", { name: "Highlight row: 30 minutes, Client reports chills and low back pain" }),
    ).toBeInTheDocument();
  });

  it("toggles a row from the keyboard", async () => {
    const { onChange } = renderItem("highlight_table", { response: { selected: ["r4"] } });
    screen.getByRole("button", { name: /^Highlight row: 30 minutes, T 38.4/ }).focus();

    await userEvent.keyboard("{Enter}");

    expect(onChange).toHaveBeenCalledWith({ selected: ["r3", "r4"] });
  });

  it("marks the revealed answer", () => {
    renderRevealed("highlight_table");

    expect(screen.getByText("✓ Correct highlight")).toBeInTheDocument();
    expect(screen.getByText("✗ Should not be highlighted")).toBeInTheDocument();
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("highlight_table", { response: { selected: ["r3"] } });

    await expectNoAxeViolations(container);
  });
});
