import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { ApiError } from "@/lib/api/errors";
import { hardNavigate } from "@/lib/browser/hardNavigate";
import { deleteAccount } from "../api/settingsApi";
import { DeleteAccountSection } from "./DeleteAccountSection";

vi.mock("../api/settingsApi", () => ({ deleteAccount: vi.fn() }));
vi.mock("@/lib/browser/hardNavigate", () => ({ hardNavigate: vi.fn() }));
const deleteMock = vi.mocked(deleteAccount);
const navigateMock = vi.mocked(hardNavigate);

async function openConfirmation() {
  const user = userEvent.setup();
  await user.click(screen.getByRole("button", { name: "Delete my account…" }));
  return user;
}

describe("DeleteAccountSection", () => {
  beforeEach(() => {
    deleteMock.mockReset();
    navigateMock.mockReset();
  });

  it("asks for confirmation first, and moves focus to the warning", async () => {
    renderWithAuth(<DeleteAccountSection />);

    await openConfirmation();

    expect(screen.getByRole("heading", { name: "Are you sure?" })).toHaveFocus();
    expect(screen.getByText(/cannot be undone/)).toBeInTheDocument();
    expect(deleteMock).not.toHaveBeenCalled();
  });

  it("can be cancelled, returning focus to the first button", async () => {
    renderWithAuth(<DeleteAccountSection />);
    const user = await openConfirmation();

    await user.click(screen.getByRole("button", { name: "Cancel" }));

    const openButton = screen.getByRole("button", { name: "Delete my account…" });
    await vi.waitFor(() => expect(openButton).toHaveFocus());
    expect(screen.queryByLabelText("Current password")).not.toBeInTheDocument();
  });

  it("needs the password", async () => {
    renderWithAuth(<DeleteAccountSection />);
    const user = await openConfirmation();

    await user.click(screen.getByRole("button", { name: "Delete my account permanently" }));

    expect(await screen.findByText("Enter your current password.")).toBeInTheDocument();
    expect(deleteMock).not.toHaveBeenCalled();
  });

  it("deletes with the password and loads the goodbye page from scratch", async () => {
    deleteMock.mockResolvedValue();
    renderWithAuth(<DeleteAccountSection />);
    const user = await openConfirmation();

    await user.type(screen.getByLabelText("Current password"), "my-password");
    await user.click(screen.getByRole("button", { name: "Delete my account permanently" }));

    await vi.waitFor(() => expect(navigateMock).toHaveBeenCalledWith("/account-deleted"));
    expect(deleteMock.mock.calls[0][0]).toEqual({ currentPassword: "my-password" });
  });

  it("shows a wrong password and stays on the page", async () => {
    deleteMock.mockRejectedValue(
      new ApiError(400, "VALIDATION_FAILED", [{ field: "currentPassword", code: "INCORRECT" }], "Invalid."),
    );
    renderWithAuth(<DeleteAccountSection />);
    const user = await openConfirmation();

    await user.type(screen.getByLabelText("Current password"), "wrong-password");
    await user.click(screen.getByRole("button", { name: "Delete my account permanently" }));

    expect(await screen.findByText("That password isn't right.")).toBeInTheDocument();
    expect(navigateMock).not.toHaveBeenCalled();
  });
});
