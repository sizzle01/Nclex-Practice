import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { ApiError } from "@/lib/api/errors";
import { requestEmailChange } from "../api/settingsApi";
import { ChangeEmailForm } from "./ChangeEmailForm";

vi.mock("../api/settingsApi", () => ({ requestEmailChange: vi.fn() }));
const requestMock = vi.mocked(requestEmailChange);

async function submit(newEmail: string, password = "my-password") {
  const user = userEvent.setup();
  if (newEmail) await user.type(screen.getByLabelText("New email address"), newEmail);
  if (password) await user.type(screen.getByLabelText("Current password"), password);
  await user.click(screen.getByRole("button", { name: "Send confirmation link" }));
}

describe("ChangeEmailForm", () => {
  beforeEach(() => {
    requestMock.mockReset();
  });

  it("shows the address the user signs in with", () => {
    renderWithAuth(<ChangeEmailForm currentEmail="ada@example.com" />);

    expect(screen.getByText("ada@example.com")).toBeInTheDocument();
  });

  it("validates before calling the API", async () => {
    renderWithAuth(<ChangeEmailForm currentEmail="ada@example.com" />);

    await submit("not-an-email", "");

    expect(await screen.findByText(/Enter a valid email address/)).toBeInTheDocument();
    expect(screen.getByText("Enter your current password.")).toBeInTheDocument();
    expect(requestMock).not.toHaveBeenCalled();
  });

  it("says where the link went and that nothing has changed yet", async () => {
    requestMock.mockResolvedValue();
    renderWithAuth(<ChangeEmailForm currentEmail="ada@example.com" />);

    await submit("new@example.com");

    expect(await screen.findByRole("status")).toHaveTextContent(
      "Check new@example.com for a confirmation link. You'll keep signing in with ada@example.com until you follow it.",
    );
    expect(requestMock.mock.calls[0][0]).toEqual({ newEmail: "new@example.com", currentPassword: "my-password" });
    expect(screen.getByLabelText("Current password")).toHaveValue("");
  });

  it("shows an address another account uses next to the field", async () => {
    requestMock.mockRejectedValue(new ApiError(409, "EMAIL_TAKEN", [], "Taken."));
    renderWithAuth(<ChangeEmailForm currentEmail="ada@example.com" />);

    await submit("taken@example.com");

    expect(await screen.findByText("Another account already uses this email address.")).toBeInTheDocument();
  });

  it("shows a wrong current password next to its field", async () => {
    requestMock.mockRejectedValue(
      new ApiError(400, "VALIDATION_FAILED", [{ field: "currentPassword", code: "INCORRECT" }], "Invalid."),
    );
    renderWithAuth(<ChangeEmailForm currentEmail="ada@example.com" />);

    await submit("new@example.com", "wrong-password");

    expect(await screen.findByText("That password isn't right.")).toBeInTheDocument();
  });
});
