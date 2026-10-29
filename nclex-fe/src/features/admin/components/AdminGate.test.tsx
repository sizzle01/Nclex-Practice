import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { isAdmin } from "../roles";
import { reply, staffUser } from "../test/adminFixtures";
import { AdminGate } from "./AdminGate";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function signedInAs(role: Parameters<typeof staffUser>[0]) {
  getMock.mockResolvedValue(reply(200, staffUser(role)));
}

describe("AdminGate", () => {
  beforeEach(() => getMock.mockReset());

  it("keeps candidates out of the console", async () => {
    signedInAs("candidate");
    renderWithAuth(<AdminGate>Console</AdminGate>, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: "You don't have access to this page" })).toBeInTheDocument();
    expect(screen.queryByText("Console")).not.toBeInTheDocument();
  });

  it("lets content staff in", async () => {
    signedInAs("reviewer");
    renderWithAuth(<AdminGate>Console</AdminGate>, { status: "authenticated" });

    expect(await screen.findByText("Console")).toBeInTheDocument();
  });

  it("keeps admin-only pages from authors", async () => {
    signedInAs("author");
    renderWithAuth(<AdminGate allow={isAdmin}>Users</AdminGate>, { status: "authenticated" });

    expect(await screen.findByRole("heading", { name: "You don't have access to this page" })).toBeInTheDocument();
  });
});
