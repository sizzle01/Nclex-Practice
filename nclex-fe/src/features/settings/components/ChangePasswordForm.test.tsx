import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { ApiError } from "@/lib/api/errors";
import { changePassword } from "../api/settingsApi";
import { ChangePasswordForm } from "./ChangePasswordForm";

vi.mock("../api/settingsApi", () => ({ changePassword: vi.fn() }));
const changeMock = vi.mocked(changePassword);

async function fillIn(current: string, next: string, confirmation = next) {
  const user = userEvent.setup();
  if (current) await user.type(screen.getByLabelText("Current password"), current);
  await user.type(screen.getByLabelText("New password"), next);
  await user.type(screen.getByLabelText("Confirm new password"), confirmation);
  await user.click(screen.getByRole("button", { name: "Change password" }));
}

describe("ChangePasswordForm", () => {
  beforeEach(() => {
    changeMock.mockReset();
  });

  it("checks the rules and the confirmation before calling the API", async () => {
    renderWithAuth(<ChangePasswordForm />);

    await fillIn("", "short", "different");

    expect(await screen.findByText("Enter your current password.")).toBeInTheDocument();
    expect(screen.getByText(/Use at least 10 characters/)).toBeInTheDocument();
    expect(screen.getByText(/The passwords don't match/)).toBeInTheDocument();
    expect(changeMock).not.toHaveBeenCalled();
  });

  it("sends both passwords, confirms, and clears the form", async () => {
    changeMock.mockResolvedValue();
    renderWithAuth(<ChangePasswordForm />);

    await fillIn("old-password", "a-brand-new-password");

    expect(await screen.findByRole("status")).toHaveTextContent("Your password has been changed. Your other devices have been signed out.");
    expect(changeMock.mock.calls[0][0]).toEqual({ currentPassword: "old-password", newPassword: "a-brand-new-password" });
    expect(screen.getByLabelText("Current password")).toHaveValue("");
    expect(screen.getByLabelText("New password")).toHaveValue("");
  });

  it("shows a wrong current password next to its field", async () => {
    changeMock.mockRejectedValue(
      new ApiError(400, "VALIDATION_FAILED", [{ field: "currentPassword", code: "INCORRECT" }], "Invalid."),
    );
    renderWithAuth(<ChangePasswordForm />);

    await fillIn("wrong-password", "a-brand-new-password");

    expect(await screen.findByText("That password isn't right.")).toBeInTheDocument();
    expect(screen.getByLabelText("Current password")).toHaveAttribute("aria-invalid", "true");
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("explains a lock with how long to wait", async () => {
    changeMock.mockRejectedValue(new ApiError(423, "ACCOUNT_LOCKED", [], "Locked.", 900));
    renderWithAuth(<ChangePasswordForm />);

    await fillIn("wrong-password", "a-brand-new-password");

    expect(await screen.findByRole("alert")).toHaveTextContent("Try again in 15 minutes");
  });
});
