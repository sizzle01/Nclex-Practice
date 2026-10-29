import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { fetchMyFlags } from "../api/flagsApi";
import { aMyFlag } from "../test/flags";
import { MyReports } from "./MyReports";

vi.mock("../api/flagsApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/flagsApi")>()),
  fetchMyFlags: vi.fn(),
}));
const mineMock = vi.mocked(fetchMyFlags);

describe("MyReports", () => {
  beforeEach(() => mineMock.mockReset());

  it("shows each report's status and, once resolved, the moderator's note", async () => {
    mineMock.mockResolvedValue({
      items: [
        aMyFlag({
          id: "r",
          status: "resolved",
          resolutionNote: "You were right; the key is fixed.",
          resolvedAt: "2026-10-29T09:00:00Z",
        }),
        aMyFlag({ id: "o", reason: "typo", comment: "Spelling of 'dyspnea'." }),
      ],
      nextCursor: null,
    });
    renderWithAuth(<MyReports />, { status: "authenticated" });

    expect(await screen.findByText("You were right; the key is fixed.")).toBeInTheDocument();
    expect(screen.getByText("Resolved")).toBeInTheDocument();
    expect(screen.getByText("Received")).toBeInTheDocument();
    expect(screen.getByText("You wrote: Spelling of 'dyspnea'.")).toBeInTheDocument();
  });

  it("says when there are no reports", async () => {
    mineMock.mockResolvedValue({ items: [], nextCursor: null });
    renderWithAuth(<MyReports />, { status: "authenticated" });

    expect(await screen.findByText(/haven't reported any questions/)).toBeInTheDocument();
  });
});
