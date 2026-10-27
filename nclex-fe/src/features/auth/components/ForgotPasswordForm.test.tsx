import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { forgotPassword } from "../api/authApi";
import { renderWithAuth } from "../test/renderWithAuth";
import { ForgotPasswordForm } from "./ForgotPasswordForm";

vi.mock("../api/authApi", () => ({ forgotPassword: vi.fn() }));
const forgotMock = vi.mocked(forgotPassword);

describe("ForgotPasswordForm", () => {
  beforeEach(() => forgotMock.mockReset());

  it("checks the email before calling the API", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ForgotPasswordForm />);

    await user.type(screen.getByLabelText("Email"), "not-an-email");
    await user.click(screen.getByRole("button", { name: "Send reset link" }));

    expect(await screen.findByText(/Enter a valid email address/)).toBeInTheDocument();
    expect(forgotMock).not.toHaveBeenCalled();
  });

  it("shows the same confirmation whether or not the account exists, and focuses it", async () => {
    forgotMock.mockResolvedValue();
    const user = userEvent.setup();
    renderWithAuth(<ForgotPasswordForm />);

    await user.type(screen.getByLabelText("Email"), "  ada@example.com ");
    await user.click(screen.getByRole("button", { name: "Send reset link" }));

    const heading = await screen.findByRole("heading", { name: "Check your email" });
    expect(heading).toHaveFocus();
    expect(screen.getByText(/If an account exists for ada@example.com/)).toBeInTheDocument();
    expect(forgotMock.mock.calls[0][0]).toBe("ada@example.com");
  });
});
