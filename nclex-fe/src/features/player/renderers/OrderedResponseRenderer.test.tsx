import { render, screen, within } from "@testing-library/react";
import { useState } from "react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { ItemAnswer } from "./ItemAnswer";
import { renderItem, renderRevealed, sampleOf } from "./testRenderer";

// Sample: eye drops, shown as c, a, e, b, d; the key is a, b, c, d, e.
const shown = ["c", "a", "e", "b", "d"];

/** The renderer with its answer held in state, as the player's save queue would. */
function StatefulOrdered() {
  const [response, setResponse] = useState<unknown>(null);
  return (
    <ItemAnswer
      itemType="ordered_response"
      content={sampleOf("ordered_response").content}
      response={response}
      onChange={setResponse}
      disabled={false}
      struckOptions={[]}
      onToggleStrike={() => undefined}
      labelledBy="stem"
      reveal={null}
    />
  );
}

function itemTexts() {
  return within(screen.getByRole("list"))
    .getAllByRole("listitem")
    .map((item) => item.textContent);
}

describe("ordered response", () => {
  it("shows the items in the order given, with no answer yet", () => {
    renderItem("ordered_response");

    expect(itemTexts()[0]).toContain("Pull the lower lid down to form a pocket");
    expect(screen.getByRole("button", { name: "Keep this order" })).toBeInTheDocument();
  });

  it("answers with the order shown when the candidate keeps it", async () => {
    const { onChange } = renderItem("ordered_response");

    await userEvent.click(screen.getByRole("button", { name: "Keep this order" }));

    expect(onChange).toHaveBeenCalledWith({ order: shown });
  });

  it("moves an item down from the keyboard and announces its new position", async () => {
    const { onChange } = renderItem("ordered_response");
    screen.getByRole("button", { name: "Move down: Pull the lower lid down to form a pocket" }).focus();

    await userEvent.keyboard("{Enter}");

    expect(onChange).toHaveBeenCalledWith({ order: ["a", "c", "e", "b", "d"] });
    // dnd-kit keeps a live region of its own for drags, so find ours by its text.
    const announcement = screen.getByText("Pull the lower lid down to form a pocket moved to position 2 of 5.");
    expect(announcement).toHaveAttribute("role", "status");
  });

  it("shows a saved order without offering Keep this order", () => {
    renderItem("ordered_response", { response: { order: ["a", "c", "e", "b", "d"] } });

    expect(itemTexts()[0]).toContain("Perform hand hygiene and put on gloves");
    expect(screen.queryByRole("button", { name: "Keep this order" })).not.toBeInTheDocument();
  });

  it("keeps focus on the moved item, switching to Move down when it reaches the top", async () => {
    render(<StatefulOrdered />);
    screen.getByRole("button", { name: "Move up: Perform hand hygiene and put on gloves" }).focus();

    await userEvent.keyboard("{Enter}");

    expect(itemTexts()[0]).toContain("Perform hand hygiene and put on gloves");
    expect(screen.getByRole("button", { name: "Move down: Perform hand hygiene and put on gloves" })).toHaveFocus();
  });

  it("disables Move up for the first item and Move down for the last", () => {
    renderItem("ordered_response");

    expect(screen.getByRole("button", { name: "Move up: Pull the lower lid down to form a pocket" })).toBeDisabled();
    expect(
      screen.getByRole("button", { name: "Move down: Place the drops in the pocket of the lower lid" }),
    ).toBeDisabled();
  });

  it("offers a drag handle that the keyboard can pick up", () => {
    renderItem("ordered_response");

    const handle = screen.getByRole("button", {
      name: "Drag Perform hand hygiene and put on gloves, now at position 2 of 5",
    });
    expect(handle).toHaveAttribute("aria-roledescription", "sortable");
  });

  it("marks each position and says where a misplaced item belongs", () => {
    renderRevealed("ordered_response");

    // The answer swapped the first two steps.
    expect(screen.getAllByText("✓ Correct position")).toHaveLength(3);
    expect(screen.getByText("✗ Wrong position; belongs at position 1")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Move/ })).not.toBeInTheDocument();
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("ordered_response");

    await expectNoAxeViolations(container);
  });
});
