import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { expect, it } from "vitest";
import { ItemGallery } from "./ItemGallery";
import { sampleItems } from "./sampleItems";

it("shows every MVP item type with a working renderer", () => {
  render(<ItemGallery />);

  for (const { itemType } of sampleItems) {
    expect(screen.getByRole("heading", { name: itemType, level: 2 }), itemType).toBeInTheDocument();
  }
  expect(sampleItems).toHaveLength(12);
  expect(screen.queryByText(/can't be answered in the player yet/)).not.toBeInTheDocument();
});

it("shows the response a renderer sends and the reveal on request", async () => {
  render(<ItemGallery />);
  const sample = screen.getByRole("region", { name: "fill_blank_calc" });

  await userEvent.type(within(sample).getByRole("textbox", { name: "Infusion rate" }), "125");
  expect(within(sample).getByText(/"value": "125"/)).toBeInTheDocument();

  await userEvent.click(within(sample).getByRole("checkbox", { name: "Show the reveal" }));
  expect(within(sample).getByText(/Correct answer: 125 mL\/hr/)).toBeInTheDocument();
  expect(within(sample).getByRole("region", { name: "Rationale" })).toBeInTheDocument();
});
