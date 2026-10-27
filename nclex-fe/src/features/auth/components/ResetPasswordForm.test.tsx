import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api/errors";
import { resetPassword } from "../api/authApi";
import { setSession, useSessionStore } from "../session/sessionStore";
import { renderWithAuth } from "../test/renderWithAuth";
import { ResetPasswordForm } from "./ResetPasswordForm";

vi.mock("../api/authApi", () => ({ resetPassword: vi.fn() }));
let search = "";
vi.mock("next/navigation", () => ({ useSearchParams: () => new URLSearchParams(search) }));
const resetMock = vi.mocked(resetPassword);

async function fillIn(password: string, confirmation = password) {
  const user = userEvent.setup();
  await user.type(screen.getByLabelText("New password"), password);
  await user.type(screen.getByLabelText("Confirm new password"), confirmation);
  return user;
}

describe("ResetPasswordForm", () => {
  beforeEach(() => {
    resetMock.mockReset();
    search = "token=reset-token";
  });

  it("checks the password rules and that both entries match before calling the API", async () => {
    renderWithAuth(<ResetPasswordForm />);
    const user = await fillIn("short", "different");

    await user.click(screen.getByRole("button", { name: "Save new password" }));

    expect(await screen.findByText(/Use at least 10 characters/)).toBeInTheDocument();
    expect(screen.getByText(/The passwords don't match/)).toBeInTheDocument();
    expect(resetMock).not.toHaveBeenCalled();
  });

  it("sends the token, signs out everywhere by default, and confirms", async () => {
    resetMock.mockResolvedValue();
    setSession("access-token", new Date(Date.now() + 60_000).toISOString());
    renderWithAuth(<ResetPasswordForm />);
    const user = await fillIn("a-brand-new-password");

    expect(screen.getByRole("checkbox", { name: /Sign out on every device/ })).toBeChecked();
    await user.click(screen.getByRole("button", { name: "Save new password" }));

    expect(await screen.findByRole("heading", { name: "Your password has been changed" })).toHaveFocus();
    expect(resetMock.mock.calls[0][0]).toEqual({
      token: "reset-token",
      newPassword: "a-brand-new-password",
      signOutEverywhere: true,
    });
    expect(useSessionStore.getState().status).toBe("anonymous");
  });

  it("can keep other sessions signed in", async () => {
    resetMock.mockResolvedValue();
    renderWithAuth(<ResetPasswordForm />);
    const user = await fillIn("a-brand-new-password");

    await user.click(screen.getByRole("checkbox", { name: /Sign out on every device/ }));
    await user.click(screen.getByRole("button", { name: "Save new password" }));

    await screen.findByRole("heading", { name: "Your password has been changed" });
    expect(resetMock.mock.calls[0][0]).toMatchObject({ signOutEverywhere: false });
  });

  it("explains an expired link and links to a new one", async () => {
    resetMock.mockRejectedValue(new ApiError(400, "INVALID_TOKEN", [], "This link is invalid or has expired."));
    renderWithAuth(<ResetPasswordForm />);
    const user = await fillIn("a-brand-new-password");

    await user.click(screen.getByRole("button", { name: "Save new password" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("This link is invalid or has expired.");
    expect(screen.getByRole("link", { name: "Get a new link" })).toHaveAttribute("href", "/forgot-password");
  });

  it("asks for a new link when the URL has no token", () => {
    search = "";
    renderWithAuth(<ResetPasswordForm />);

    expect(screen.getByRole("heading", { name: "This link is incomplete" })).toBeInTheDocument();
    expect(screen.queryByLabelText("New password")).not.toBeInTheDocument();
  });
});
