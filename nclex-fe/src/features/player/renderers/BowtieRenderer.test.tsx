import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { ItemAnswer } from "./ItemAnswer";
import { renderItem, renderRevealed, sampleOf } from "./testRenderer";

// Sample: a client in DKA. Conditions c1 to c4 (c1 is DKA), actions a1 to a5, parameters p1 to p5.
const fluids = "Start an infusion of 0.9% sodium chloride";
const insulin = "Start a continuous IV regular insulin infusion as prescribed";
const bicarbonate = "Give sodium bicarbonate by IV push";

/** The renderer with its answer held in state, as the player's save queue would. */
function StatefulBowtie() {
  const [response, setResponse] = useState<unknown>(null);
  return (
    <>
      <p id="stem">Stem</p>
      <ItemAnswer
        itemType="bowtie"
        content={sampleOf("bowtie").content}
        response={response}
        onChange={setResponse}
        disabled={false}
        struckOptions={[]}
        onToggleStrike={() => undefined}
        labelledBy="stem"
        reveal={null}
      />
      <output data-testid="response">{JSON.stringify(response)}</output>
    </>
  );
}

const slot = (name: string) => screen.getByRole("button", { name: new RegExp(`^${escapeRegExp(name)}:`) });
const option = (text: string) => screen.getByRole("button", { name: text });
const savedResponse = () => JSON.parse(screen.getByTestId("response").textContent ?? "null");
// The live region is ours; dnd-kit keeps one of its own for drags.
const announced = (text: string) => expect(screen.getByText(text)).toHaveAttribute("role", "status");

function escapeRegExp(text: string) {
  return text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

describe("bowtie", () => {
  it("shows five empty slots and the three lists of options", () => {
    renderItem("bowtie");

    expect(slot("Condition most likely experiencing")).toHaveAccessibleName("Condition most likely experiencing: empty");
    expect(slot("Action to take, slot 1 of 2")).toBeInTheDocument();
    expect(slot("Action to take, slot 2 of 2")).toBeInTheDocument();
    expect(slot("Parameter to monitor, slot 1 of 2")).toBeInTheDocument();
    expect(slot("Parameter to monitor, slot 2 of 2")).toBeInTheDocument();
    for (const list of ["Potential conditions", "Actions to take", "Parameters to monitor"]) {
      expect(screen.getByRole("group", { name: list })).toBeInTheDocument();
    }
  });

  it("places an option from the keyboard: pick it, and its slot gets focus", async () => {
    const { onChange } = renderItem("bowtie");
    option(fluids).focus();

    await userEvent.keyboard("{Enter}");

    expect(option(fluids)).toHaveAttribute("aria-pressed", "true");
    expect(slot("Action to take, slot 1 of 2")).toHaveFocus();
    announced(`Picked ${fluids}. Choose a slot for it: Action to take. Press Escape to cancel.`);

    await userEvent.keyboard("{Enter}");

    expect(onChange).toHaveBeenCalledWith({ condition: null, actions: ["a1"], parameters: [] });
    announced(`${fluids} placed in Action to take, slot 1 of 2.`);
  });

  it("fills a whole bowtie with taps and saves one response", async () => {
    render(<StatefulBowtie />);

    await userEvent.click(option("Diabetic ketoacidosis"));
    await userEvent.click(slot("Condition most likely experiencing"));
    await userEvent.click(option(fluids));
    await userEvent.click(slot("Action to take, slot 1 of 2"));
    await userEvent.click(option(insulin));
    await userEvent.click(slot("Action to take, slot 2 of 2"));
    await userEvent.click(option("Serum potassium"));
    await userEvent.click(slot("Parameter to monitor, slot 1 of 2"));
    await userEvent.click(option("Blood glucose every hour"));
    await userEvent.click(slot("Parameter to monitor, slot 2 of 2"));

    expect(savedResponse()).toEqual({ condition: "c1", actions: ["a1", "a2"], parameters: ["p1", "p2"] });
    expect(slot("Condition most likely experiencing")).toHaveAccessibleName(
      "Condition most likely experiencing: Diabetic ketoacidosis",
    );
    // A placed option stays in its list, marked and out of reach until it is removed.
    expect(option(`${fluids} (in the diagram)`)).toBeDisabled();
  });

  it("replaces the option in a full wing's slot", async () => {
    render(<StatefulBowtie />);
    await userEvent.click(option(fluids));
    await userEvent.click(slot("Action to take, slot 1 of 2"));
    await userEvent.click(option(insulin));
    await userEvent.click(slot("Action to take, slot 2 of 2"));

    await userEvent.click(option(bicarbonate));
    expect(slot("Action to take, slot 2 of 2")).toHaveFocus();
    await userEvent.click(slot("Action to take, slot 1 of 2"));

    expect(savedResponse()).toEqual({ condition: null, actions: ["a4", "a2"], parameters: [] });
  });

  it("removes an option, and clears the answer when the bowtie is empty again", async () => {
    render(<StatefulBowtie />);
    await userEvent.click(option(fluids));
    await userEvent.click(slot("Action to take, slot 1 of 2"));

    await userEvent.click(screen.getByRole("button", { name: `Remove ${fluids} from Action to take, slot 1 of 2` }));

    expect(savedResponse()).toBeNull();
    expect(option(fluids)).toBeEnabled();
    announced(`${fluids} removed from the diagram.`);
  });

  it("only places an option in a slot of its own kind", async () => {
    const { onChange } = renderItem("bowtie");

    await userEvent.click(option(fluids));
    await userEvent.click(slot("Condition most likely experiencing"));

    expect(onChange).not.toHaveBeenCalled();
    announced("Pick an option from Potential conditions first, then choose this slot.");
  });

  it("cancels a pick with Escape", async () => {
    const { onChange } = renderItem("bowtie");
    await userEvent.click(option(fluids));

    await userEvent.keyboard("{Escape}");
    await userEvent.click(slot("Action to take, slot 1 of 2"));

    expect(option(fluids)).toHaveAttribute("aria-pressed", "false");
    expect(onChange).not.toHaveBeenCalled();
  });

  it("shows a saved answer in its slots", () => {
    renderItem("bowtie", { response: { condition: "c2", actions: ["a3"], parameters: [] } });

    expect(slot("Condition most likely experiencing")).toHaveAccessibleName(
      "Condition most likely experiencing: Hyperosmolar hyperglycemic state",
    );
    expect(slot("Action to take, slot 1 of 2")).toHaveAccessibleName(
      "Action to take, slot 1 of 2: Give insulin glargine subcutaneously instead of IV insulin",
    );
    expect(slot("Action to take, slot 2 of 2")).toHaveAccessibleName("Action to take, slot 2 of 2: empty");
  });

  it("marks each slot right or wrong after the reveal, and the correct options left out", () => {
    // The gallery answer: condition c2 (wrong), actions a1 (right) and a4 (wrong), parameters p1 and p2 (right).
    renderRevealed("bowtie");

    // Nothing can be changed: the slots are plain text and every option is disabled.
    expect(screen.queryByRole("button", { name: /slot|Condition most likely/ })).not.toBeInTheDocument();
    for (const button of screen.getAllByRole("button")) expect(button).toBeDisabled();
    expect(screen.getAllByText("✓ Correct")).toHaveLength(3);
    expect(screen.getAllByText("✗ Incorrect")).toHaveLength(2);
    // Diabetic ketoacidosis and the insulin infusion were left out.
    expect(screen.getAllByText("Correct answer, not selected")).toHaveLength(2);
  });

  it("has no accessibility violations before an answer", async () => {
    const { container } = renderItem("bowtie");
    await expectNoAxeViolations(container);
  });

  it("has no accessibility violations once revealed", async () => {
    const { container } = renderRevealed("bowtie");
    await expectNoAxeViolations(container);
  });
});
