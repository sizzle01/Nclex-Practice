import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StrictMode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api/errors";
import { resendVerification, verifyEmail } from "../api/authApi";
import { renderWithAuth } from "../test/renderWithAuth";
import { VerifyEmailResult } from "./VerifyEmailResult";

vi.mock("../api/authApi", () => ({ verifyEmail: vi.fn(), resendVerification: vi.fn() }));
let search = "";
vi.mock("next/navigation", () => ({ useSearchParams: () => new URLSearchParams(search) }));
const verifyMock = vi.mocked(verifyEmail);
const resendMock = vi.mocked(resendVerification);

const invalidToken = new ApiError(400, "INVALID_TOKEN", [], "This link is invalid or has expired.");

describe("VerifyEmailResult", () => {
  beforeEach(() => {
    verifyMock.mockReset();
    resendMock.mockReset();
    search = "token=abc123";
  });

  it("sends the token from the link once and shows success", async () => {
    verifyMock.mockResolvedValue();
    renderWithAuth(
      <StrictMode>
        <VerifyEmailResult />
      </StrictMode>,
    );

    expect(await screen.findByRole("heading", { name: "Your email is verified" })).toBeInTheDocument();
    expect(verifyMock).toHaveBeenCalledTimes(1);
    expect(verifyMock.mock.calls[0][0]).toBe("abc123");
  });

  it("offers the dashboard to a signed-in user and log in to everyone else", async () => {
    verifyMock.mockResolvedValue();
    const { unmount } = renderWithAuth(<VerifyEmailResult />, { status: "authenticated" });
    expect(await screen.findByRole("link", { name: "Go to your dashboard" })).toHaveAttribute("href", "/dashboard");
    unmount();

    renderWithAuth(<VerifyEmailResult />, { status: "anonymous" });
    expect(await screen.findByRole("link", { name: "Log in" })).toHaveAttribute("href", "/login");
  });

  it("explains an invalid link and lets a signed-in user resend", async () => {
    verifyMock.mockRejectedValue(invalidToken);
    resendMock.mockResolvedValue();
    const user = userEvent.setup();
    renderWithAuth(<VerifyEmailResult />, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: "This link is invalid or has expired" })).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Resend verification email" }));

    expect(await screen.findByRole("status")).toHaveTextContent("We've sent a new link.");
  });

  it("asks a signed-out user to log in to get a new link", async () => {
    verifyMock.mockRejectedValue(invalidToken);
    renderWithAuth(<VerifyEmailResult />, { status: "anonymous" });

    expect(await screen.findByRole("link", { name: "Log in" })).toHaveAttribute("href", "/login?next=%2Fdashboard");
    expect(screen.queryByRole("button", { name: "Resend verification email" })).not.toBeInTheDocument();
  });

  it("says the link is incomplete when there is no token, without calling the API", () => {
    search = "";
    renderWithAuth(<VerifyEmailResult />);

    expect(screen.getByRole("heading", { name: "This link is incomplete" })).toBeInTheDocument();
    expect(verifyMock).not.toHaveBeenCalled();
  });
});
