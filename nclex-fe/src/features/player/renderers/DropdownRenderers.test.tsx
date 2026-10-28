import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { blankLabels } from "./DropdownClozeRenderer";
import { renderItem, renderRevealed } from "./testRenderer";

describe("drop-down cloze", () => {
  // Sample: "The client is most likely experiencing {{b1}}, as shown by {{b2}}."
  it("names each blank by its place in the sentence", () => {
    renderItem("dropdown_cloze");

    expect(
      screen.getByRole("combobox", {
        name: "Blank 1 of 2: The client is most likely experiencing blank, as shown by ….",
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("combobox", {
        name: "Blank 2 of 2: The client is most likely experiencing …, as shown by blank.",
      }),
    ).toBeInTheDocument();
  });

  it("labels blanks within their own sentence", () => {
    const labels = blankLabels("Give {{b1}} now. Then check {{b2}} in an hour.");

    expect(labels.get("b1")).toBe("Blank 1 of 2: Give blank now.");
    expect(labels.get("b2")).toBe("Blank 2 of 2: Then check blank in an hour.");
  });

  it("is reached with Tab and answers each blank by id", async () => {
    const { onChange } = renderItem("dropdown_cloze", { response: { byBlank: { b2: "a" } } });

    await userEvent.tab();
    expect(screen.getByRole("combobox", { name: /^Blank 1/ })).toHaveFocus();
    await userEvent.selectOptions(screen.getByRole("combobox", { name: /^Blank 1/ }), "hypoglycemia");

    expect(onChange).toHaveBeenCalledWith({ byBlank: { b2: "a", b1: "a" } });
  });

  it("clears the answer when the only chosen blank goes back to Select…", async () => {
    const { onChange } = renderItem("dropdown_cloze", { response: { byBlank: { b1: "a" } } });

    await userEvent.selectOptions(screen.getByRole("combobox", { name: /^Blank 1/ }), "Select…");

    expect(onChange).toHaveBeenCalledWith(null);
  });

  it("marks each blank and gives the right answer for a wrong one", () => {
    renderRevealed("dropdown_cloze");

    expect(screen.getByText("✗ Incorrect")).toBeInTheDocument();
    expect(screen.getByText(/Correct answer: hypoglycemia/)).toBeInTheDocument();
    expect(screen.getByText("✓ Correct")).toBeInTheDocument();
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("dropdown_cloze", { response: { byBlank: { b1: "a" } } });

    await expectNoAxeViolations(container);
  });
});

describe("drop-down table", () => {
  // Sample: kidney disease labs; rows Potassium, Sodium, Fasting glucose; blanks b1–b3 in "Interpretation".
  it("is a table whose blanks are named by column and row", () => {
    renderItem("dropdown_table");

    const table = screen.getByRole("table", { name: "Stem" });
    expect(within(table).getByRole("rowheader", { name: "Potassium" })).toBeInTheDocument();
    expect(within(table).getByRole("combobox", { name: "Interpretation for Potassium" })).toBeInTheDocument();
  });

  it("answers a blank", async () => {
    const { onChange } = renderItem("dropdown_table");

    await userEvent.selectOptions(
      screen.getByRole("combobox", { name: "Interpretation for Sodium" }),
      "Within normal range",
    );

    expect(onChange).toHaveBeenCalledWith({ byBlank: { b2: "b" } });
  });

  it("marks the revealed answer", () => {
    renderRevealed("dropdown_table");

    expect(screen.getAllByText("✓ Correct")).toHaveLength(2);
    expect(screen.getByText("✗ Incorrect")).toBeInTheDocument();
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("dropdown_table");

    await expectNoAxeViolations(container);
  });
});
