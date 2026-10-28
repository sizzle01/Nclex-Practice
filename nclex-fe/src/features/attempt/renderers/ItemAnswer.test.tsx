import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ItemAnswer } from "./ItemAnswer";

const content = {
  options: [
    { id: "a", text: "Check the airway" },
    { id: "b", text: "Call the provider" },
    { id: "c", text: "Raise the head of the bed" },
  ],
};

function renderItem(itemType: "multiple_choice" | "sata" | "bowtie", response: unknown = null, disabled = false) {
  const onChange = vi.fn();
  render(
    <>
      <p id="stem">Which action should the nurse take first?</p>
      <ItemAnswer
        itemType={itemType}
        content={content}
        response={response}
        onChange={onChange}
        disabled={disabled}
        struckOptions={["c"]}
        labelledBy="stem"
      />
    </>,
  );
  return { onChange };
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

  it("cannot be changed when locked", () => {
    renderItem("multiple_choice", { selected: "a" }, true);

    for (const radio of screen.getAllByRole("radio")) expect(radio).toBeDisabled();
  });
});

describe("select all that apply", () => {
  it("is a group of checkboxes that says to select all that apply", () => {
    renderItem("sata", { selected: ["a"] });

    expect(screen.getByRole("group", { name: "Which action should the nurse take first?" })).toHaveAccessibleDescription(
      "Select all that apply.",
    );
    expect(screen.getByRole("checkbox", { name: "Check the airway" })).toBeChecked();
  });

  it("keeps the options in content order and toggles with Space", async () => {
    const { onChange } = renderItem("sata", { selected: ["c"] });
    screen.getByRole("checkbox", { name: "Check the airway" }).focus();

    await userEvent.keyboard(" ");

    expect(onChange).toHaveBeenCalledWith({ selected: ["a", "c"] });
  });

  it("clears the answer when the last option is unticked", async () => {
    const { onChange } = renderItem("sata", { selected: ["a"] });

    await userEvent.click(screen.getByRole("checkbox", { name: "Check the airway" }));

    expect(onChange).toHaveBeenCalledWith(null);
  });
});

it("explains item types that have no renderer yet", () => {
  renderItem("bowtie");

  expect(screen.getByRole("note")).toHaveTextContent("can't be answered in the player yet");
});
