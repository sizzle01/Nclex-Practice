import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { PauseOverlay, pausesLeftText } from "./PauseOverlay";

describe("PauseOverlay", () => {
  it("is a dialog with focus on Resume, and says how many pauses are left", async () => {
    const onResume = vi.fn();
    render(
      <PauseOverlay pause={{ used: 1, max: 3, maxPauseSec: 600, startedAt: "2026-10-28T09:00:00Z" }} busy={false} error={null} onResume={onResume} />,
    );

    expect(screen.getByRole("dialog", { name: "Test paused" })).toHaveAccessibleDescription(/2 of 3 pauses left/);
    expect(screen.getByText(/longer than 10 minutes uses up test time/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Resume" })).toHaveFocus();

    await userEvent.keyboard("{Enter}");
    expect(onResume).toHaveBeenCalledOnce();
  });

  it("shows a resume error", () => {
    render(<PauseOverlay pause={{ used: 1, max: null, maxPauseSec: null, startedAt: null }} busy={false} error="Could not resume." onResume={vi.fn()} />);

    expect(screen.getByRole("alert")).toHaveTextContent("Could not resume.");
    expect(screen.queryByText(/uses up test time/)).not.toBeInTheDocument();
  });

  it.each([
    [{ used: 0, max: null }, "You can pause as often as you need."],
    [{ used: 0, max: 1 }, "1 of 1 pause left."],
    [{ used: 2, max: 2 }, "0 of 2 pauses left."],
  ])("describes the pause limit %o", (counts, text) => {
    expect(pausesLeftText({ ...counts, maxPauseSec: null, startedAt: null })).toBe(text);
  });
});
