import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "../test/renderWithAuth";
import { RequireAuth } from "./RequireAuth";

const replace = vi.fn();
vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace }),
  usePathname: () => "/dashboard",
}));

describe("RequireAuth", () => {
  beforeEach(() => {
    replace.mockReset();
  });

  it("sends signed-out users to /login with a way back", () => {
    renderWithAuth(<RequireAuth>Secret dashboard</RequireAuth>, { status: "anonymous" });

    expect(replace).toHaveBeenCalledWith("/login?next=%2Fdashboard");
    expect(screen.queryByText("Secret dashboard")).not.toBeInTheDocument();
  });

  it("waits, without redirecting, while the session is being checked", () => {
    renderWithAuth(<RequireAuth>Secret dashboard</RequireAuth>, { status: "unknown" });

    expect(screen.getByRole("status")).toHaveTextContent("Checking your session");
    expect(screen.queryByText("Secret dashboard")).not.toBeInTheDocument();
    expect(replace).not.toHaveBeenCalled();
  });

  it("shows the page to signed-in users", () => {
    renderWithAuth(<RequireAuth>Secret dashboard</RequireAuth>, { status: "authenticated" });

    expect(screen.getByText("Secret dashboard")).toBeInTheDocument();
    expect(replace).not.toHaveBeenCalled();
  });
});
