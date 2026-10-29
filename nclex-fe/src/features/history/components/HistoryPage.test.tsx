import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { AttemptSummary } from "@/features/attempt/api/attemptApi";
import { apiClient } from "@/lib/api/client";
import { HistoryPage } from "./HistoryPage";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function finished(id: string, title: string, overrides: Partial<AttemptSummary> = {}): AttemptSummary {
  return {
    id,
    testSet: { id: `set-${id}`, title },
    status: "submitted",
    mode: "practice",
    totalItems: 75,
    answeredItems: 75,
    startedAt: "2026-10-01T10:00:00Z",
    lastActivityAt: "2026-10-01T11:00:00Z",
    submittedAt: "2026-10-01T11:00:00Z",
    staleDraft: false,
    timeRemainingMs: null,
    scorePct: 0.7123,
    passed: null,
    ...overrides,
  };
}

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <HistoryPage />
    </QueryClientProvider>,
  );
}

describe("HistoryPage", () => {
  beforeEach(() => getMock.mockReset());

  it("lists finished tests with their score, estimate and links to results and review", async () => {
    getMock.mockResolvedValue(
      reply(200, {
        items: [finished("a-1", "Challenge Exam 1", { mode: "challenge", passed: true }), finished("a-2", "Practice 3")],
        nextCursor: null,
      }),
    );

    renderPage();

    const table = await screen.findByRole("table", { name: "Finished tests" });
    const challenge = within(table).getByRole("row", { name: /Challenge Exam 1/ });
    expect(challenge).toHaveTextContent("Challenge");
    expect(challenge).toHaveTextContent("71%");
    expect(challenge).toHaveTextContent("Likely ready");
    expect(within(challenge).getByRole("link", { name: "Results" })).toHaveAttribute("href", "/attempts/a-1/results");
    expect(within(challenge).getByRole("link", { name: "Review" })).toHaveAttribute("href", "/attempts/a-1/review");
    expect(within(table).getByRole("row", { name: /Practice 3/ })).toHaveTextContent("—");
    expect(getMock).toHaveBeenCalledWith("/api/v1/attempts", { params: { query: { status: "submitted", limit: 20, cursor: undefined } } });
    expect(screen.queryByRole("button", { name: "Show more" })).not.toBeInTheDocument();
  });

  it("loads the next page with the cursor when asked", async () => {
    getMock
      .mockResolvedValueOnce(reply(200, { items: [finished("a-1", "Newest")], nextCursor: "page-2" }))
      .mockResolvedValueOnce(reply(200, { items: [finished("a-2", "Oldest")], nextCursor: null }));
    renderPage();

    await userEvent.click(await screen.findByRole("button", { name: "Show more" }));

    expect(await screen.findByRole("rowheader", { name: "Oldest" })).toBeInTheDocument();
    expect(screen.getByRole("rowheader", { name: "Newest" })).toBeInTheDocument();
    expect(getMock).toHaveBeenLastCalledWith("/api/v1/attempts", { params: { query: { status: "submitted", limit: 20, cursor: "page-2" } } });
    expect(screen.queryByRole("button", { name: "Show more" })).not.toBeInTheDocument();
  });

  it("points to the practice sets before the first finished test", async () => {
    getMock.mockResolvedValue(reply(200, { items: [], nextCursor: null }));

    renderPage();

    expect(await screen.findByRole("heading", { name: "No finished tests yet" })).toBeInTheDocument();
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
  });
});
