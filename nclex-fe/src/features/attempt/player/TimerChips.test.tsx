import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { TimerChips } from "./TimerChips";

function liveRegion(container: HTMLElement) {
  return container.querySelector('[aria-live="polite"]')!;
}

describe("TimerChips", () => {
  it("shows the exam and question timers as timers with spoken labels", () => {
    render(<TimerChips examMs={3_725_000} itemMs={45_000} paused={false} />);

    expect(screen.getByRole("timer", { name: "Test time remaining: 1 hour 3 minutes" })).toHaveTextContent("1:02:05");
    expect(screen.getByRole("timer", { name: /Time remaining for this question/ })).toHaveTextContent("00:45");
  });

  it("shows nothing for an untimed test", () => {
    const { container } = render(<TimerChips examMs={null} itemMs={null} paused={false} />);

    expect(container).toBeEmptyDOMElement();
  });

  it("says when time is low or the timer is paused, not only with colour", () => {
    const { rerender } = render(<TimerChips examMs={50_000} itemMs={null} paused={false} />);
    expect(screen.getByRole("timer")).toHaveTextContent("time low");

    rerender(<TimerChips examMs={50_000} itemMs={null} paused />);
    expect(screen.getByRole("timer", { name: /paused/ })).toHaveTextContent("paused");
  });

  it("announces 5 minutes and 1 minute left, and nothing in between", () => {
    const { container, rerender } = render(<TimerChips examMs={301_000} itemMs={null} paused={false} />);
    expect(liveRegion(container)).toHaveTextContent("");

    rerender(<TimerChips examMs={299_000} itemMs={null} paused={false} />);
    expect(liveRegion(container)).toHaveTextContent("5 minutes remaining in the test");

    rerender(<TimerChips examMs={200_000} itemMs={null} paused={false} />);
    expect(liveRegion(container)).toHaveTextContent("5 minutes remaining in the test");

    rerender(<TimerChips examMs={59_000} itemMs={null} paused={false} />);
    expect(liveRegion(container)).toHaveTextContent("1 minute remaining in the test");
  });

  it("does not announce on first render, even with little time left", () => {
    const { container } = render(<TimerChips examMs={30_000} itemMs={null} paused={false} />);

    expect(liveRegion(container)).toHaveTextContent("");
  });

  it("announces the question timer's last minute", () => {
    const { container, rerender } = render(<TimerChips examMs={null} itemMs={61_000} paused={false} />);

    rerender(<TimerChips examMs={null} itemMs={60_000} paused={false} />);

    expect(liveRegion(container)).toHaveTextContent("1 minute remaining for this question");
  });
});
