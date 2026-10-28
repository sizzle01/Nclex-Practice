import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { stubMatchMedia } from "@/test/matchMedia";
import { renderItem, renderRevealed } from "./testRenderer";

// matrix_mc sample: heart failure, rows r1–r4, columns c1 (Left-sided) and c2 (Right-sided).
// matrix_mr sample: medications r1–r3, adverse effects c1–c4, grouped by row.

afterEach(() => {
  // Back to jsdom's default (no matchMedia), which is the wide layout.
  stubMatchMedia(false);
});

describe("matrix multiple choice", () => {
  it("is a table with row and column headers and a labelled radio in each cell", () => {
    renderItem("matrix_mc");

    const table = screen.getByRole("table", { name: "Stem" });
    expect(within(table).getByRole("columnheader", { name: "Left-sided" })).toBeInTheDocument();
    expect(within(table).getByRole("rowheader", { name: "Orthopnea" })).toBeInTheDocument();
    expect(within(table).getAllByRole("radio")).toHaveLength(8);
    expect(screen.getByRole("radio", { name: "Orthopnea: Right-sided" })).toBeInTheDocument();
  });

  it("moves between cells with the arrow keys and selects with Space", async () => {
    const { onChange } = renderItem("matrix_mc");
    screen.getByRole("radio", { name: "Crackles in both lung bases: Left-sided" }).focus();

    await userEvent.keyboard("{ArrowDown}{ArrowRight}");
    expect(screen.getByRole("radio", { name: "Jugular venous distension: Right-sided" })).toHaveFocus();
    expect(onChange).not.toHaveBeenCalled();

    await userEvent.keyboard(" ");
    expect(onChange).toHaveBeenCalledWith({ byRow: { r2: "c2" } });
  });

  it("keeps the other rows' answers when one row changes", async () => {
    const { onChange } = renderItem("matrix_mc", { response: { byRow: { r1: "c1" } } });

    await userEvent.click(screen.getByRole("radio", { name: "Orthopnea: Left-sided" }));

    expect(onChange).toHaveBeenCalledWith({ byRow: { r1: "c1", r4: "c1" } });
  });

  it("marks the chosen and the expected cell of a wrong row", () => {
    renderRevealed("matrix_mc");

    // Row 1 was answered Right-sided; the answer is Left-sided.
    const row = screen.getByRole("rowheader", { name: "Crackles in both lung bases" }).closest("tr")!;
    expect(within(row).getByText("✗ Incorrect")).toBeInTheDocument();
    expect(within(row).getByText("Correct answer, not selected")).toBeInTheDocument();
    expect(screen.getAllByText("✓ Correct")).toHaveLength(3);
  });

  it("stacks into one group per row on narrow screens", () => {
    stubMatchMedia(true);
    renderItem("matrix_mc");

    expect(screen.queryByRole("table")).not.toBeInTheDocument();
    const row = screen.getByRole("group", { name: "Orthopnea" });
    expect(within(row).getByRole("radio", { name: "Left-sided" })).toBeInTheDocument();
    expect(within(row).getByRole("radio", { name: "Right-sided" })).toBeInTheDocument();
  });

  it("answers the same way when stacked", async () => {
    stubMatchMedia(true);
    const { onChange } = renderItem("matrix_mc");

    await userEvent.click(
      within(screen.getByRole("group", { name: "Orthopnea" })).getByRole("radio", { name: "Left-sided" }),
    );

    expect(onChange).toHaveBeenCalledWith({ byRow: { r4: "c1" } });
  });

  it.each([false, true])("has no axe violations (stacked: %s)", async (stacked) => {
    stubMatchMedia(stacked);
    const { container } = renderItem("matrix_mc", { response: { byRow: { r1: "c1" } } });

    await expectNoAxeViolations(container);
  });
});

describe("matrix multiple response", () => {
  it("toggles cells with Space and groups them by row", async () => {
    const { onChange } = renderItem("matrix_mr", { response: { byGroup: { r1: ["r1:c4"] } } });
    screen.getByRole("checkbox", { name: "Warfarin: Black, tarry stools" }).focus();

    await userEvent.keyboard("{ArrowLeft}{ArrowLeft}{ArrowLeft} ");

    expect(onChange).toHaveBeenCalledWith({ byGroup: { r1: ["r1:c1", "r1:c4"] } });
  });

  it("groups by column when the content says so", async () => {
    const content = {
      rows: [
        { id: "r1", text: "Day 1" },
        { id: "r2", text: "Day 2" },
      ],
      columns: [
        { id: "c1", text: "Fever" },
        { id: "c2", text: "Rash" },
      ],
      groupAxis: "column",
    };
    const { onChange } = renderItem("matrix_mr", { content, response: { byGroup: { c1: ["r2:c1"] } } });

    await userEvent.click(screen.getByRole("checkbox", { name: "Day 1: Fever" }));

    expect(onChange).toHaveBeenCalledWith({ byGroup: { c1: ["r1:c1", "r2:c1"] } });
  });

  it("clears the answer when the last cell is unticked", async () => {
    const { onChange } = renderItem("matrix_mr", { response: { byGroup: { r2: ["r2:c2"] } } });

    await userEvent.click(screen.getByRole("checkbox", { name: "Lisinopril: Swelling of the face, lips or tongue" }));

    expect(onChange).toHaveBeenCalledWith(null);
  });

  it("marks right, wrong and missed cells", () => {
    renderRevealed("matrix_mr");

    expect(screen.getAllByText("✗ Incorrect")).toHaveLength(1);
    expect(screen.getAllByText("Correct answer, not selected")).toHaveLength(1);
  });

  it("has no axe violations", async () => {
    const { container } = renderItem("matrix_mr", { response: { byGroup: { r1: ["r1:c1"] } } });

    await expectNoAxeViolations(container);
  });
});
