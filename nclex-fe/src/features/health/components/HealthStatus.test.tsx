import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import { HealthStatus } from "./HealthStatus";

// Replace only the network call; everything above it (fetchReadiness, the query hook, the component) is real.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function readyzReply(status: number, body: unknown) {
  const ok = status < 400;
  // Shape of an openapi-fetch result: body in `data` on success, in `error` otherwise.
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) };
}

function renderWithQueryClient() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <HealthStatus />
    </QueryClientProvider>,
  );
}

describe("HealthStatus", () => {
  beforeEach(() => {
    // Block body on purpose: a function returned from beforeEach is run by Vitest as a teardown.
    getMock.mockReset();
  });

  it("shows each dependency's state from /readyz", async () => {
    getMock.mockResolvedValue(
      readyzReply(200, { status: "ok", checks: { postgres: "ok", redis: "ok" } }) as never,
    );

    renderWithQueryClient();

    expect(await screen.findByText(/Postgres: OK/)).toBeInTheDocument();
    expect(screen.getByText(/Redis: OK/)).toBeInTheDocument();
    expect(getMock).toHaveBeenCalledWith("/readyz");
  });

  it("shows Postgres as unavailable when /readyz answers 503", async () => {
    getMock.mockResolvedValue(
      readyzReply(503, { status: "unhealthy", checks: { postgres: "unhealthy", redis: "degraded" } }) as never,
    );

    renderWithQueryClient();

    expect(await screen.findByText(/Postgres: Unavailable/)).toBeInTheDocument();
    expect(screen.getByText(/Redis: Degraded/)).toBeInTheDocument();
  });

  it("says the API is unreachable when the request fails", async () => {
    getMock.mockRejectedValue(new TypeError("Failed to fetch"));

    renderWithQueryClient();

    expect(await screen.findByText(/Cannot reach the API/)).toBeInTheDocument();
  });
});
