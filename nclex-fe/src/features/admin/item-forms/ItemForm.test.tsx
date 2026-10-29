import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { ItemForm } from "./ItemForm";
import { authorableTypes, type AuthorableType, type ItemParts } from "./payloads";
import { emptyItem } from "./templates";

/** Renders one item type's form with real state, and exposes the latest value. */
function renderForm(itemType: AuthorableType) {
  const latest: { value: ItemParts } = { value: emptyItem(itemType) };
  function Harness() {
    const [value, setValue] = useState(latest.value);
    latest.value = value;
    return <ItemForm itemType={itemType} value={value} disabled={false} onChange={setValue} />;
  }
  const result = render(<Harness />);
  return { ...result, latest };
}

describe("ItemForm", () => {
  it.each(authorableTypes.map((t) => t.type))("renders an accessible form for %s", async (type) => {
    const { container } = renderForm(type);
    await expectNoAxeViolations(container);
  });

  it("multiple choice: picking Correct sets the key", async () => {
    const { latest } = renderForm("multiple_choice");

    await userEvent.click(screen.getAllByRole("radio", { name: "Correct" })[2]);
    await userEvent.type(screen.getByRole("textbox", { name: "Options: c text" }), "Assess breathing");

    expect(latest.value.answerKey).toEqual({ correct: "c" });
    expect((latest.value.content as { options: Array<{ text: string }> }).options[2].text).toBe("Assess breathing");
  });

  it("select all that apply: removing an option drops it from the key", async () => {
    const { latest } = renderForm("sata");
    await userEvent.click(screen.getAllByRole("checkbox", { name: "Correct" })[1]);
    expect(latest.value.answerKey).toEqual({ correct: ["a", "b"] });

    await userEvent.click(screen.getByRole("button", { name: "Remove a" }));

    expect(latest.value.answerKey).toEqual({ correct: ["b"] });
  });

  it("ordered response: moving an item changes the correct order", async () => {
    const { latest } = renderForm("ordered_response");

    await userEvent.click(screen.getByRole("button", { name: "Move c up" }));

    expect(latest.value.answerKey).toMatchObject({ order: ["a", "c", "b", "d"] });
  });

  it("matrix multiple response: ticked cells are grouped by the chosen axis", async () => {
    const { latest } = renderForm("matrix_mr");
    await userEvent.type(screen.getByRole("textbox", { name: "Row r2 text" }), "Furosemide");
    await userEvent.type(screen.getByRole("textbox", { name: "Column c2 text" }), "Monitor potassium");

    await userEvent.click(screen.getByRole("checkbox", { name: "Furosemide: Monitor potassium" }));
    expect(latest.value.answerKey).toEqual({ byGroup: { r1: ["r1:c1"], r2: ["r2:c1", "r2:c2"], r3: ["r3:c1"] } });

    await userEvent.selectOptions(screen.getByRole("combobox", { name: "Score per" }), "column");
    expect(latest.value.answerKey).toEqual({ byGroup: { c1: ["r1:c1", "r2:c1", "r3:c1"], c2: ["r2:c2"], c3: [] } });
  });

  it("drop-down cloze: adding a blank adds its placeholder and key entry", async () => {
    const { latest } = renderForm("dropdown_cloze");

    await userEvent.click(screen.getByRole("button", { name: "Add blank" }));

    const content = latest.value.content as { template: string; blanks: Array<{ id: string }> };
    expect(content.template).toContain("{{b3}}");
    expect(content.blanks.map((b) => b.id)).toEqual(["b1", "b2", "b3"]);
    expect(latest.value.answerKey).toEqual({ byBlank: { b1: "a", b2: "a", b3: "a" } });
  });

  it("drop-down table: making a cell a blank adds a blank to choose from", async () => {
    const { latest } = renderForm("dropdown_table");

    await userEvent.click(screen.getByRole("button", { name: "Make row 1, column 1 a blank" }));

    expect((latest.value.content as { blanks: unknown[] }).blanks).toHaveLength(3);
    expect(screen.getByRole("group", { name: "Blank b3 choices" })).toBeInTheDocument();
  });

  it("highlight text: a segment that is no longer selectable leaves the key", async () => {
    const { latest } = renderForm("highlight_text");

    await userEvent.click(screen.getAllByRole("checkbox", { name: "Selectable" })[0]);

    expect(latest.value.answerKey).toEqual({ correct: [] });
  });

  it("calculation: a tolerance replaces rounding", async () => {
    const { latest } = renderForm("fill_blank_calc");

    await userEvent.type(screen.getByRole("spinbutton", { name: "Tolerance (optional)" }), "0.5");

    expect(latest.value.answerKey).toMatchObject({ tolerance: 0.5 });
    expect(screen.getByRole("combobox", { name: "Rounding" })).toBeDisabled();
  });
});
