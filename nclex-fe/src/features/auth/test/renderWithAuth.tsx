import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render } from "@testing-library/react";
import type { ReactElement } from "react";
import { vi } from "vitest";
import { AuthContext, type AuthContextValue } from "../AuthProvider";

/** Renders `ui` with a fake auth context (no network) and a fresh query client. */
export function renderWithAuth(ui: ReactElement, auth: Partial<AuthContextValue> = {}) {
  const value: AuthContextValue = {
    status: "anonymous",
    signIn: vi.fn(),
    signOut: vi.fn(),
    signOutEverywhere: vi.fn(),
    ...auth,
  };
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  const result = render(
    <QueryClientProvider client={queryClient}>
      <AuthContext value={value}>{ui}</AuthContext>
    </QueryClientProvider>,
  );
  return { ...result, auth: value };
}
