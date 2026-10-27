import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { TestSetCard } from "./TestSetCard";
import { challengeSet, practiceSet, sampleSet } from "./testSets";

function renderCard(set = practiceSet, selected = false) {
  const onOpen = vi.fn();
  render(<TestSetCard set={set} selected={selected} panelId="panel" onOpen={onOpen} />);
  return { onOpen };
}

describe("TestSetCard", () => {
  it("shows the title, mode, question count and timing", () => {
    renderCard(challengeSet);

    expect(screen.getByRole("heading", { name: "Comprehensive Challenge 1" })).toBeInTheDocument();
    expect(screen.getByText("Challenge")).toBeInTheDocument();
    expect(screen.getByText("40 questions")).toBeInTheDocument();
    expect(screen.getByText("80 minutes")).toBeInTheDocument();
  });

  it("marks sample sets and says untimed sets are untimed", () => {
    renderCard(sampleSet);

    expect(screen.getByText("Sample")).toBeInTheDocument();
    expect(screen.getByText("Untimed")).toBeInTheDocument();
    expect(screen.queryByText(/Locked/)).not.toBeInTheDocument();
  });

  it("marks locked premium sets", () => {
    renderCard({ ...practiceSet, accessTier: "premium", locked: true });

    expect(screen.getByText(/Locked: premium/)).toBeInTheDocument();
    expect(screen.queryByText("Sample")).not.toBeInTheDocument();
  });

  it("opens the set's details from the keyboard", async () => {
    const user = userEvent.setup();
    const { onOpen } = renderCard();

    await user.tab();
    const button = screen.getByRole("button", { name: "View details" });
    expect(button).toHaveFocus();
    expect(button).toHaveAttribute("aria-expanded", "false");
    await user.keyboard("{Enter}");

    expect(onOpen).toHaveBeenCalledWith(practiceSet.id);
  });

  it("tells assistive technology which panel it controls while open", () => {
    renderCard(practiceSet, true);

    const button = screen.getByRole("button", { name: "View details" });
    expect(button).toHaveAttribute("aria-expanded", "true");
    expect(button).toHaveAttribute("aria-controls", "panel");
  });
});
