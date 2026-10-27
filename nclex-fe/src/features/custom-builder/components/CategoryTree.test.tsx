import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { describe, expect, it } from "vitest";
import { CategoryTree } from "./CategoryTree";
import { testTaxonomy } from "./testTaxonomy";

function Harness({ initial = [] as number[] }) {
  const [value, setValue] = useState(initial);
  return (
    <>
      <CategoryTree categories={testTaxonomy.clientNeeds} value={value} onChange={setValue} />
      <output data-testid="value">{value.join(",")}</output>
    </>
  );
}

describe("CategoryTree", () => {
  it("selects all of a category's subcategories from its own box", async () => {
    const user = userEvent.setup();
    render(<Harness />);

    await user.click(screen.getByRole("checkbox", { name: "Safe & Effective Care Environment" }));

    expect(screen.getByTestId("value")).toHaveTextContent("2,3");
    expect(screen.getByRole("checkbox", { name: "Management of Care" })).toBeChecked();
    expect(screen.getByRole("checkbox", { name: "Safety and Infection Control" })).toBeChecked();
  });

  it("shows a category as mixed when only some subcategories are chosen", async () => {
    const user = userEvent.setup();
    render(<Harness />);

    await user.click(screen.getByRole("checkbox", { name: "Management of Care" }));

    const parent = screen.getByRole("checkbox", { name: "Safe & Effective Care Environment" });
    expect(parent).not.toBeChecked();
    expect(parent).toBePartiallyChecked();
  });

  it("clears every subcategory when a full category is unchecked", async () => {
    const user = userEvent.setup();
    render(<Harness initial={[2, 3, 5]} />);

    await user.click(screen.getByRole("checkbox", { name: "Safe & Effective Care Environment" }));

    expect(screen.getByTestId("value")).toHaveTextContent(/^5$/);
  });

  it("treats a category without subcategories as a leaf", async () => {
    const user = userEvent.setup();
    render(<Harness />);

    await user.click(screen.getByRole("checkbox", { name: "Psychosocial Integrity" }));

    expect(screen.getByTestId("value")).toHaveTextContent(/^5$/);
  });

  it("works from the keyboard", async () => {
    const user = userEvent.setup();
    render(<Harness />);

    await user.tab();
    expect(screen.getByRole("checkbox", { name: "Safe & Effective Care Environment" })).toHaveFocus();
    await user.keyboard(" ");

    expect(screen.getByTestId("value")).toHaveTextContent("2,3");
  });
});
