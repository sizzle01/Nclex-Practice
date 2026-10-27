import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api/errors";
import { confirmEmailChange } from "../api/authApi";
import { renderWithAuth } from "../test/renderWithAuth";
import { ConfirmEmailChangeResult } from "./ConfirmEmailChangeResult";

vi.mock("../api/authApi", () => ({ confirmEmailChange: vi.fn() }));
let search = "";
vi.mock("next/navigation", () => ({ useSearchParams: () => new URLSearchParams(search) }));
const confirmMock = vi.mocked(confirmEmailChange);

describe("ConfirmEmailChangeResult", () => {
  beforeEach(() => {
    confirmMock.mockReset();
    search = "token=change-token";
  });

  it("sends the link's token once and confirms the change", async () => {
    confirmMock.mockResolvedValue();
    renderWithAuth(<ConfirmEmailChangeResult />, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: "Your email address has been changed" })).toBeInTheDocument();
    expect(confirmMock).toHaveBeenCalledTimes(1);
    expect(confirmMock.mock.calls[0][0]).toBe("change-token");
    expect(screen.getByRole("link", { name: "Back to account settings" })).toHaveAttribute("href", "/settings/account");
  });

  it("sends signed-out users to log in afterwards", async () => {
    confirmMock.mockResolvedValue();
    renderWithAuth(<ConfirmEmailChangeResult />);

    expect(await screen.findByRole("link", { name: "Log in" })).toHaveAttribute("href", "/login");
  });

  it("explains an expired or used link", async () => {
    confirmMock.mockRejectedValue(new ApiError(400, "INVALID_TOKEN", [], "Invalid."));
    renderWithAuth(<ConfirmEmailChangeResult />, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: "This link is invalid or has expired" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Go to account settings" })).toBeInTheDocument();
  });

  it("explains an address that another account took in the meantime", async () => {
    confirmMock.mockRejectedValue(new ApiError(409, "EMAIL_TAKEN", [], "Taken."));
    renderWithAuth(<ConfirmEmailChangeResult />);

    expect(await screen.findByRole("heading", { name: "This email address is already in use" })).toBeInTheDocument();
  });

  it("does not call the API without a token", () => {
    search = "";
    renderWithAuth(<ConfirmEmailChangeResult />);

    expect(screen.getByRole("heading", { name: "This link is incomplete" })).toBeInTheDocument();
    expect(confirmMock).not.toHaveBeenCalled();
  });
});
