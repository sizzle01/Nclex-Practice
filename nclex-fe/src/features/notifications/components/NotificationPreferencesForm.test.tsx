import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { fetchPreferences, type NotificationPreferences, replacePreferences } from "../api/notificationsApi";
import { NotificationPreferencesForm } from "./NotificationPreferencesForm";

vi.mock("../api/notificationsApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/notificationsApi")>()),
  fetchPreferences: vi.fn(),
  replacePreferences: vi.fn(),
}));
const fetchMock = vi.mocked(fetchPreferences);
const replaceMock = vi.mocked(replacePreferences);

const defaults: NotificationPreferences = {
  preferences: [
    { category: "results", inApp: true, email: true, locked: false },
    { category: "reminders", inApp: true, email: false, locked: false },
    { category: "engagement", inApp: true, email: false, locked: false },
    { category: "content", inApp: true, email: false, locked: false },
    { category: "security", inApp: true, email: true, locked: true },
  ],
};

describe("NotificationPreferencesForm", () => {
  beforeEach(() => {
    fetchMock.mockReset().mockResolvedValue(defaults);
    replaceMock.mockReset().mockImplementation(async (body) => ({
      preferences: body.preferences!.map((p) => ({ ...p, locked: p.category === "security" })) as never,
    }));
  });

  it("shows each category's channels, with security locked on", async () => {
    renderWithAuth(<NotificationPreferencesForm />, { status: "authenticated" });

    const results = await screen.findByRole("group", { name: "Results" });
    expect(within(results).getByRole("checkbox", { name: "By email" })).toBeChecked();
    const security = screen.getByRole("group", { name: "Security" });
    expect(within(security).getByRole("checkbox", { name: "In the app" })).toBeDisabled();
    expect(within(security).getByRole("checkbox", { name: "By email" })).toBeDisabled();
    expect(within(security).getByRole("checkbox", { name: "By email" })).toBeChecked();
  });

  it("saves every category, keeping security on", async () => {
    const user = userEvent.setup();
    renderWithAuth(<NotificationPreferencesForm />, { status: "authenticated" });

    await user.click(within(await screen.findByRole("group", { name: "Results" })).getByRole("checkbox", { name: "By email" }));
    await user.click(within(screen.getByRole("group", { name: "Reminders" })).getByRole("checkbox", { name: "By email" }));
    await user.click(screen.getByRole("button", { name: "Save settings" }));

    expect(await screen.findByText("Your notification settings have been saved.")).toBeInTheDocument();
    expect(replaceMock.mock.calls[0][0]).toEqual({
      preferences: [
        { category: "results", inApp: true, email: false },
        { category: "reminders", inApp: true, email: true },
        { category: "engagement", inApp: true, email: false },
        { category: "content", inApp: true, email: false },
        { category: "security", inApp: true, email: true },
      ],
    });
  });

  it("says so when saving fails", async () => {
    replaceMock.mockRejectedValue(new Error("network"));
    const user = userEvent.setup();
    renderWithAuth(<NotificationPreferencesForm />, { status: "authenticated" });

    await user.click(within(await screen.findByRole("group", { name: "Your reports" })).getByRole("checkbox", { name: "In the app" }));
    await user.click(screen.getByRole("button", { name: "Save settings" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("We couldn't save your settings.");
  });
});
