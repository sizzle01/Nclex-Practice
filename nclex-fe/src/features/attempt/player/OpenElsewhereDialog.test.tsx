import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { expect, it, vi } from "vitest";
import { OpenElsewhereDialog } from "./OpenElsewhereDialog";

it("blocks the player, focuses Continue here and calls back when pressed", async () => {
  const onContinueHere = vi.fn();
  render(<OpenElsewhereDialog busy={false} onContinueHere={onContinueHere} />);

  const dialog = screen.getByRole("alertdialog", { name: "This attempt is open on another device" });
  const button = screen.getByRole("button", { name: "Continue here" });
  expect(dialog).toHaveAttribute("aria-modal", "true");
  expect(button).toHaveFocus();

  await userEvent.keyboard("{Enter}");

  expect(onContinueHere).toHaveBeenCalledOnce();
});

it("disables the button while taking over", () => {
  render(<OpenElsewhereDialog busy onContinueHere={vi.fn()} />);

  expect(screen.getByRole("button", { name: "Taking over…" })).toBeDisabled();
});
