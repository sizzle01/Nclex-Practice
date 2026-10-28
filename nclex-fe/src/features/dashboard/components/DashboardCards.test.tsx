import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import type { ReactNode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { PerformanceSummary } from "@/features/performance/api/performanceApi";
import { apiClient } from "@/lib/api/client";
import { PerformanceOverview } from "./PerformanceOverview";
import { RecentResults } from "./RecentResults";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function renderWithQueries(ui: ReactNode) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

const summary: PerformanceSummary = {
  track: "RN",
  attemptsCount: 3,
  itemsAnswered: 90,
  itemsCorrect: 55,
  accuracy: 0.64,
  avgTimeMs: 70_000,
  weakest: [
    { key: "PHI.PPT", name: "Pharmacological and Parenteral Therapies", itemsAnswered: 12, itemsCorrect: 4, accuracy: 0.41, avgTimeMs: 90_000, attemptsCount: 3 },
    { key: "SECE.SIC", name: "Safety and Infection Control", itemsAnswered: 9, itemsCorrect: 5, accuracy: 0.56, avgTimeMs: 60_000, attemptsCount: 2 },
  ],
};

describe("dashboard performance card", () => {
  beforeEach(() => getMock.mockReset());

  it("shows the overall score and the weakest categories, worst first", async () => {
    getMock.mockResolvedValue(reply(200, summary));

    renderWithQueries(<PerformanceOverview />);

    expect(await screen.findByText(/overall on 90 questions from 3 tests/)).toHaveTextContent("64%");
    const items = within(screen.getByRole("list")).getAllByRole("listitem");
    expect(items.map((i) => i.textContent)).toEqual([
      "Pharmacological and Parenteral Therapies41%12 questions",
      "Safety and Infection Control56%9 questions",
    ]);
  });

  it("explains what will appear before the first finished test", async () => {
    getMock.mockResolvedValue(reply(200, { ...summary, attemptsCount: 0, itemsAnswered: 0, weakest: [] }));

    renderWithQueries(<PerformanceOverview />);

    expect(await screen.findByText(/Once you finish a test/)).toBeInTheDocument();
  });

  it("asks for more answers when no category has enough to call weak", async () => {
    getMock.mockResolvedValue(reply(200, { ...summary, weakest: [] }));

    renderWithQueries(<PerformanceOverview />);

    expect(await screen.findByText(/Answer a few more questions per category/)).toBeInTheDocument();
  });
});

describe("dashboard recent results", () => {
  beforeEach(() => getMock.mockReset());

  it("links each of the latest finished tests to its results with the score", async () => {
    getMock.mockResolvedValue(
      reply(200, {
        items: [
          {
            id: "a-9",
            testSet: { id: "s-1", title: "Comprehensive Practice 1" },
            status: "submitted",
            mode: "practice",
            totalItems: 75,
            answeredItems: 75,
            startedAt: "2026-10-20T10:00:00Z",
            lastActivityAt: "2026-10-20T11:00:00Z",
            submittedAt: "2026-10-20T11:00:00Z",
            staleDraft: false,
            timeRemainingMs: null,
            scorePct: 0.8,
            passed: null,
          },
        ],
        nextCursor: "next",
      }),
    );

    renderWithQueries(<RecentResults />);

    const list = await screen.findByRole("list", { name: "Recent results" });
    expect(within(list).getByRole("link", { name: "Comprehensive Practice 1" })).toHaveAttribute("href", "/attempts/a-9/results");
    expect(list).toHaveTextContent("80%");
    expect(getMock).toHaveBeenCalledWith("/api/v1/attempts", { params: { query: { status: "submitted", limit: 5, cursor: undefined } } });
  });

  it("says when there are no results yet", async () => {
    getMock.mockResolvedValue(reply(200, { items: [], nextCursor: null }));

    renderWithQueries(<RecentResults />);

    expect(await screen.findByText(/Your scores show up here once you finish a test/)).toBeInTheDocument();
  });
});
