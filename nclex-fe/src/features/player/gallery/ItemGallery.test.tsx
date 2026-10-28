import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { expect, it } from "vitest";
import { ItemGallery } from "./ItemGallery";
import { sampleItems } from "./sampleItems";

it("shows every MVP item type with a working renderer", () => {
  renderGallery();

  for (const { itemType } of sampleItems) {
    expect(screen.getByRole("heading", { name: itemType, level: 2 }), itemType).toBeInTheDocument();
  }
  expect(sampleItems).toHaveLength(12);
  expect(screen.queryByText(/can't be answered in the player yet/)).not.toBeInTheDocument();
});

it("shows the response a renderer sends and the reveal on request", async () => {
  renderGallery();
  const sample = screen.getByRole("region", { name: "fill_blank_calc" });

  await userEvent.type(within(sample).getByRole("textbox", { name: "Infusion rate" }), "125");
  expect(within(sample).getByText(/"value": "125"/)).toBeInTheDocument();

  await userEvent.click(within(sample).getByRole("checkbox", { name: "Show the reveal" }));
  expect(within(sample).getByText(/Correct answer: 125 mL\/hr/)).toBeInTheDocument();
  expect(within(sample).getByRole("region", { name: "Rationale" })).toBeInTheDocument();
});

// The app wraps every page in a QueryClientProvider; media in the gallery come from local files.
function renderGallery() {
  render(
    <QueryClientProvider client={new QueryClient()}>
      <ItemGallery />
    </QueryClientProvider>,
  );
}
