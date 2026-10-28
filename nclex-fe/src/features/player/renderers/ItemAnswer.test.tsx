import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { ItemAnswer, renderers } from "./ItemAnswer";
import { renderRevealed } from "./testRenderer";
import { UnsupportedRenderer } from "./UnsupportedRenderer";

const content = {
  options: [
    { id: "a", text: "Check the airway" },
    { id: "b", text: "Call the provider" },
    { id: "c", text: "Raise the head of the bed" },
  ],
};

function renderItem(itemType: "multiple_choice" | "sata" | "bowtie", response: unknown = null, disabled = false) {
  const onChange = vi.fn();
  const onToggleStrike = vi.fn();
  const { container } = render(
    <>
      <p id="stem">Which action should the nurse take first?</p>
      <ItemAnswer
        itemType={itemType}
        content={content}
        response={response}
        onChange={onChange}
        disabled={disabled}
        struckOptions={["c"]}
        onToggleStrike={onToggleStrike}
        labelledBy="stem"
        reveal={null}
      />
    </>,
  );
  return { onChange, onToggleStrike, container };
}

describe("multiple choice", () => {
  it("is a radio group labelled by the stem", () => {
    renderItem("multiple_choice", { selected: "b" });

    expect(screen.getByRole("radiogroup", { name: "Which action should the nurse take first?" })).toBeInTheDocument();
    expect(screen.getByRole("radio", { name: "Call the provider" })).toBeChecked();
  });

  it("answers with the chosen option id", async () => {
    const { onChange } = renderItem("multiple_choice");

    await userEvent.click(screen.getByRole("radio", { name: "Check the airway" }));

    expect(onChange).toHaveBeenCalledWith({ selected: "a" });
  });

  it("moves the answer with the arrow keys", async () => {
    const { onChange } = renderItem("multiple_choice", { selected: "a" });
    screen.getByRole("radio", { name: "Check the airway" }).focus();

    await userEvent.keyboard("{ArrowDown}");

    expect(onChange).toHaveBeenLastCalledWith({ selected: "b" });
  });

  it("cannot be changed when locked, and hides the strike tool", () => {
    renderItem("multiple_choice", { selected: "a" }, true);

    for (const radio of screen.getAllByRole("radio")) expect(radio).toBeDisabled();
    expect(screen.queryByRole("button", { name: /Strike out/ })).not.toBeInTheDocument();
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("multiple_choice", { selected: "a" });

    await expectNoAxeViolations(container);
  });

  it("marks the revealed answer in words, not colour alone", () => {
    renderRevealed("multiple_choice");

    expect(screen.getByText("✗ Incorrect")).toBeInTheDocument();
    expect(screen.getByText("Correct answer, not selected")).toBeInTheDocument();
  });
});

describe("strike-through", () => {
  it("shows struck options crossed out, with a pressed toggle", () => {
    renderItem("multiple_choice");

    expect(screen.getByText("Raise the head of the bed")).toHaveClass(/struck/);
    expect(screen.getByRole("button", { name: "Strike out Raise the head of the bed" })).toHaveAttribute(
      "aria-pressed",
      "true",
    );
    expect(screen.getByRole("button", { name: "Strike out Check the airway" })).toHaveAttribute(
      "aria-pressed",
      "false",
    );
  });

  it("toggles from the keyboard without changing the answer", async () => {
    const { onChange, onToggleStrike } = renderItem("sata");
    screen.getByRole("checkbox", { name: "Check the airway" }).focus();

    await userEvent.tab();
    await userEvent.keyboard("{Enter}");

    expect(onToggleStrike).toHaveBeenCalledWith("a");
    expect(onChange).not.toHaveBeenCalled();
  });
});

describe("select all that apply", () => {
  it("is a group of checkboxes that says to select all that apply", () => {
    renderItem("sata", { selected: ["a"] });

    expect(
      screen.getByRole("group", { name: "Which action should the nurse take first?" }),
    ).toHaveAccessibleDescription("Select all that apply.");
    expect(screen.getByRole("checkbox", { name: "Check the airway" })).toBeChecked();
  });

  it("keeps the options in content order and toggles with Space", async () => {
    const { onChange } = renderItem("sata", { selected: ["c"] });
    screen.getByRole("checkbox", { name: "Check the airway" }).focus();

    await userEvent.keyboard(" ");

    expect(onChange).toHaveBeenCalledWith({ selected: ["a", "c"] });
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("sata", { selected: ["a"] });

    await expectNoAxeViolations(container);
  });

  it("clears the answer when the last option is unticked", async () => {
    const { onChange } = renderItem("sata", { selected: ["a"] });

    await userEvent.click(screen.getByRole("checkbox", { name: "Check the airway" }));

    expect(onChange).toHaveBeenCalledWith(null);
  });
});

it("explains phase 2 item types, which have no renderer yet", () => {
  renderItem("bowtie");

  expect(screen.getByRole("note")).toHaveTextContent("can't be answered in the player yet");
});

it("has a real renderer for every MVP item type", () => {
  const mvp = [
    "multiple_choice",
    "sata",
    "select_n",
    "matrix_mc",
    "matrix_mr",
    "multiple_response_grouping",
    "dropdown_cloze",
    "dropdown_table",
    "highlight_text",
    "highlight_table",
    "fill_blank_calc",
    "ordered_response",
  ] as const;

  for (const itemType of mvp) expect(renderers[itemType], itemType).not.toBe(UnsupportedRenderer);
});
