import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { resendVerification, type UserProfile } from "@/features/auth/api/authApi";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { ApiError } from "@/lib/api/errors";
import { VerifyEmailBanner } from "./VerifyEmailBanner";

vi.mock("@/features/auth/api/authApi", () => ({ resendVerification: vi.fn() }));
const resendMock = vi.mocked(resendVerification);

const user = { id: "user-1", email: "ada@example.com", emailVerified: false } as UserProfile;

describe("VerifyEmailBanner", () => {
  beforeEach(() => {
    resendMock.mockReset();
    window.sessionStorage.clear();
  });

  it("tells the user where the link went", () => {
    renderWithAuth(<VerifyEmailBanner user={user} />);

    expect(screen.getByRole("region", { name: "Verify your email address" })).toHaveTextContent("ada@example.com");
  });

  it("resends the link and announces it", async () => {
    resendMock.mockResolvedValue();
    const actor = userEvent.setup();
    renderWithAuth(<VerifyEmailBanner user={user} />);

    await actor.click(screen.getByRole("button", { name: "Resend verification email" }));

    expect(await screen.findByRole("status")).toHaveTextContent("We've sent a new link to ada@example.com.");
  });

  it("says when to try again after too many requests", async () => {
    resendMock.mockRejectedValue(new ApiError(429, "RATE_LIMITED", [], "Too many requests.", 1500));
    const actor = userEvent.setup();
    renderWithAuth(<VerifyEmailBanner user={user} />);

    await actor.click(screen.getByRole("button", { name: "Resend verification email" }));

    expect(await screen.findByRole("status")).toHaveTextContent("Try again in 25 minutes.");
  });

  it("can be dismissed for the rest of the browser session", async () => {
    const onDismiss = vi.fn();
    const actor = userEvent.setup();
    const { unmount } = renderWithAuth(<VerifyEmailBanner user={user} onDismiss={onDismiss} />);

    await actor.click(screen.getByRole("button", { name: "Dismiss the verify email reminder" }));

    expect(screen.queryByRole("region")).not.toBeInTheDocument();
    expect(onDismiss).toHaveBeenCalled();
    unmount();
    renderWithAuth(<VerifyEmailBanner user={user} />);
    expect(screen.queryByRole("region")).not.toBeInTheDocument();
  });

  it("still shows for a different user in the same tab", async () => {
    const actor = userEvent.setup();
    const { unmount } = renderWithAuth(<VerifyEmailBanner user={user} />);
    await actor.click(screen.getByRole("button", { name: "Dismiss the verify email reminder" }));
    unmount();

    renderWithAuth(<VerifyEmailBanner user={{ ...user, id: "user-2" }} />);

    expect(screen.getByRole("region", { name: "Verify your email address" })).toBeInTheDocument();
  });
});
