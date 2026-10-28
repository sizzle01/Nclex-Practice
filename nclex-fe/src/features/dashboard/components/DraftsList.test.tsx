import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { AttemptSummary } from "@/features/attempt/api/attemptApi";
import { apiClient } from "@/lib/api/client";
import { DraftsList, percentComplete } from "./DraftsList";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), DELETE: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const deleteMock = vi.mocked(apiClient.DELETE);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function draft(overrides: Partial<AttemptSummary>): AttemptSummary {
  return {
    id: "a-1",
    testSet: { id: "s-1", title: "Comprehensive Practice 1" },
    status: "in_progress",
    mode: "practice",
    totalItems: 75,
    answeredItems: 30,
    startedAt: "2026-10-01T10:00:00Z",
    lastActivityAt: "2026-10-27T10:00:00Z",
    submittedAt: null,
    staleDraft: false,
    timeRemainingMs: null,
    ...overrides,
  };
}

function renderList() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <DraftsList />
    </QueryClientProvider>,
  );
}

describe("DraftsList", () => {
  beforeEach(() => {
    getMock.mockReset();
    deleteMock.mockReset();
  });

  it("rounds the share answered down", () => {
    expect(percentComplete({ answeredItems: 2, totalItems: 3 })).toBe(66);
    expect(percentComplete({ answeredItems: 0, totalItems: 0 })).toBe(0);
  });

  it("lists drafts with % complete and a Resume link", async () => {
    getMock.mockResolvedValue(reply(200, { items: [draft({})], nextCursor: null }));

    renderList();

    const row = await screen.findByRole("article", { name: "Comprehensive Practice 1" });
    expect(within(row).getByText("40% complete")).toBeInTheDocument();
    expect(within(row).getByRole("link", { name: "Resume" })).toHaveAttribute("href", "/attempts/a-1");
    expect(within(row).queryByRole("button", { name: "Discard" })).not.toBeInTheDocument();
    expect(getMock).toHaveBeenCalledWith("/api/v1/attempts", { params: { query: { status: "in_progress,paused" } } });
  });

  it("says so when nothing is in progress", async () => {
    getMock.mockResolvedValue(reply(200, { items: [], nextCursor: null }));

    renderList();

    expect(await screen.findByText("You have no tests in progress.")).toBeInTheDocument();
  });

  it("asks resume or discard for an old draft, and discards only after confirming", async () => {
    getMock.mockResolvedValue(reply(200, { items: [draft({ staleDraft: true })], nextCursor: null }));
    deleteMock.mockResolvedValue(reply(204, undefined));
    renderList();

    expect(await screen.findByText(/over 30 days\. Resume it or discard it\?/)).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Discard" }));
    expect(deleteMock).not.toHaveBeenCalled();
    await userEvent.click(screen.getByRole("button", { name: "Keep it" }));
    await userEvent.click(screen.getByRole("button", { name: "Discard" }));
    await userEvent.click(screen.getByRole("button", { name: "Yes, discard" }));

    expect(deleteMock).toHaveBeenCalledWith("/api/v1/attempts/{id}", { params: { path: { id: "a-1" } } });
  });
});
