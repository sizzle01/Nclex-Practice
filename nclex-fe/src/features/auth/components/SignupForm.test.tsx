import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api/errors";
import { type AuthResponse, signup } from "../api/authApi";
import { renderWithAuth } from "../test/renderWithAuth";
import { SignupForm } from "./SignupForm";

vi.mock("../api/authApi", () => ({ signup: vi.fn() }));
const replace = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ replace }) }));
const signupMock = vi.mocked(signup);

const authResponse: AuthResponse = {
  user: {
    id: "0192f0c4-0000-7000-8000-000000000001",
    email: "ada@example.com",
    emailVerified: false,
    role: "candidate",
    track: "RN",
    planTier: "free",
    displayName: "Ada",
    timezone: "UTC",
    targetExamDate: null,
  },
  accessToken: "access-token",
  accessTokenExpiresAt: "2026-10-24T15:15:00Z",
};

async function fillValidForm(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText("Email"), "  ada@example.com ");
  await user.type(screen.getByLabelText("Password"), "correct-horse-battery");
  await user.click(screen.getByLabelText(/NCLEX-RN/));
  await user.type(screen.getByLabelText(/Display name/), "Ada");
}

describe("SignupForm", () => {
  beforeEach(() => {
    signupMock.mockReset();
    replace.mockReset();
  });

  it("shows a message for each invalid field and does not call the API", async () => {
    const user = userEvent.setup();
    renderWithAuth(<SignupForm />);

    await user.click(screen.getByRole("button", { name: "Create account" }));

    expect(await screen.findByText("Enter your email address.")).toBeInTheDocument();
    expect(screen.getByText("Use at least 10 characters.")).toBeInTheDocument();
    expect(screen.getByText("Choose the exam you are preparing for.")).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toHaveAttribute("aria-invalid", "true");
    expect(screen.getByLabelText("Email")).toHaveAccessibleDescription("Enter your email address.");
    expect(signupMock).not.toHaveBeenCalled();
  });

  it("rejects a malformed email and a password that is too short", async () => {
    const user = userEvent.setup();
    renderWithAuth(<SignupForm />);

    await user.type(screen.getByLabelText("Email"), "ada@");
    await user.type(screen.getByLabelText("Password"), "short");
    await user.click(screen.getByRole("button", { name: "Create account" }));

    expect(await screen.findByText(/Enter a valid email address/)).toBeInTheDocument();
    expect(screen.getByText("Use at least 10 characters.")).toBeInTheDocument();
    expect(signupMock).not.toHaveBeenCalled();
  });

  it("signs up with trimmed values, signs in and goes to the dashboard", async () => {
    signupMock.mockResolvedValue(authResponse);
    const user = userEvent.setup();
    const { auth } = renderWithAuth(<SignupForm />);

    await fillValidForm(user);
    await user.click(screen.getByRole("button", { name: "Create account" }));

    await waitFor(() => expect(replace).toHaveBeenCalledWith("/dashboard"));
    expect(signupMock).toHaveBeenCalledWith({
      email: "ada@example.com",
      password: "correct-horse-battery",
      track: "RN",
      displayName: "Ada",
    });
    expect(auth.signIn).toHaveBeenCalledWith(authResponse);
  });

  it("shows 'email taken' next to the email field", async () => {
    signupMock.mockRejectedValue(new ApiError(409, "EMAIL_TAKEN", [], "An account with this email already exists."));
    const user = userEvent.setup();
    renderWithAuth(<SignupForm />);

    await fillValidForm(user);
    await user.click(screen.getByRole("button", { name: "Create account" }));

    expect(await screen.findByText("An account with this email already exists.")).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toHaveAttribute("aria-invalid", "true");
    expect(replace).not.toHaveBeenCalled();
  });

  it("places the API's field errors next to their fields", async () => {
    signupMock.mockRejectedValue(
      new ApiError(400, "VALIDATION_FAILED", [{ field: "password", code: "TOO_LONG" }], "Some fields are missing or invalid."),
    );
    const user = userEvent.setup();
    renderWithAuth(<SignupForm />);

    await fillValidForm(user);
    await user.click(screen.getByRole("button", { name: "Create account" }));

    expect(await screen.findByText("This is too long.")).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });
});
