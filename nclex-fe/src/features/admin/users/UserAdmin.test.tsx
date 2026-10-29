import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { reply, staffUser } from "../test/adminFixtures";
import { UserAdmin } from "./UserAdmin";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), PATCH: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const patchMock = vi.mocked(apiClient.PATCH);

const me = staffUser("admin", "admin-1");
const users = [
  { id: "admin-1", email: "admin@example.com", displayName: null, role: "admin", planTier: "free", track: "RN", emailVerified: true, lockedUntil: null, createdAt: "2026-10-01T00:00:00Z" },
  { id: "user-2", email: "sam@example.com", displayName: "Sam", role: "candidate", planTier: "free", track: "RN", emailVerified: true, lockedUntil: "2999-01-01T00:00:00Z", createdAt: "2026-10-02T00:00:00Z" },
];

describe("UserAdmin", () => {
  beforeEach(() => {
    getMock.mockReset();
    patchMock.mockReset();
    getMock.mockImplementation(((path: string) =>
      Promise.resolve(path === "/api/v1/me" ? reply(200, me) : reply(200, { items: users, nextCursor: null }))) as never);
    patchMock.mockResolvedValue(reply(200, users[1]));
  });

  it("changes another user's role", async () => {
    renderWithAuth(<UserAdmin />, { status: "authenticated" });

    await userEvent.selectOptions(await screen.findByRole("combobox", { name: "Role of sam@example.com" }), "reviewer");

    expect(patchMock).toHaveBeenCalledWith("/api/v1/admin/users/{id}", {
      params: { path: { id: "user-2" } },
      body: { role: "reviewer", planTier: null, unlock: null },
    });
  });

  it("does not let an admin change their own role", async () => {
    renderWithAuth(<UserAdmin />, { status: "authenticated" });

    await vi.waitFor(() => expect(screen.getByRole("combobox", { name: "Role of admin@example.com" })).toBeDisabled());
  });

  it("unlocks a locked account", async () => {
    renderWithAuth(<UserAdmin />, { status: "authenticated" });

    await userEvent.click(await screen.findByRole("button", { name: "Unlock sam@example.com" }));

    expect(patchMock).toHaveBeenCalledWith("/api/v1/admin/users/{id}", expect.objectContaining({ body: { role: null, planTier: null, unlock: true } }));
  });
});
