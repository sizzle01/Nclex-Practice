import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { updateMe } from "../api/settingsApi";
import { ProfileForm } from "./ProfileForm";
import { testUser } from "./testUser";

vi.mock("../api/settingsApi", () => ({ updateMe: vi.fn() }));
const updateMock = vi.mocked(updateMe);

describe("ProfileForm", () => {
  beforeEach(() => {
    updateMock.mockReset();
  });

  it("starts from the current profile", () => {
    renderWithAuth(<ProfileForm user={{ ...testUser, timezone: "Europe/London" }} />);

    expect(screen.getByLabelText(/Display name/)).toHaveValue("Ada");
    expect(screen.getByRole("radio", { name: /NCLEX-RN/ })).toBeChecked();
    expect(screen.getByLabelText("Time zone")).toHaveValue("Europe/London");
  });

  it("offers UTC even when the browser's list leaves it out", () => {
    renderWithAuth(<ProfileForm user={testUser} />);

    expect(screen.getByLabelText("Time zone")).toHaveValue("UTC");
  });

  it("saves the changes and confirms", async () => {
    updateMock.mockImplementation(async (body) => ({ ...testUser, ...body, targetExamDate: body.targetExamDate ?? null }) as never);
    const user = userEvent.setup();
    renderWithAuth(<ProfileForm user={testUser} />);

    await user.clear(screen.getByLabelText(/Display name/));
    await user.type(screen.getByLabelText(/Display name/), "Ada L");
    await user.click(screen.getByRole("radio", { name: /NCLEX-PN/ }));
    await user.click(screen.getByRole("button", { name: "Save profile" }));

    expect(await screen.findByText("Your profile has been saved.")).toBeInTheDocument();
    expect(updateMock.mock.calls[0][0]).toEqual({
      displayName: "Ada L",
      track: "PN",
      timezone: "UTC",
      targetExamDate: undefined,
    });
  });

  it("checks the display name length before calling the API", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ProfileForm user={testUser} />);

    await user.type(screen.getByLabelText(/Display name/), "x".repeat(80));
    await user.click(screen.getByRole("button", { name: "Save profile" }));

    expect(await screen.findByText(/at most 80 characters/)).toBeInTheDocument();
    expect(updateMock).not.toHaveBeenCalled();
  });
});
