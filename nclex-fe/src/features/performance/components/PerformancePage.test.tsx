import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { BreakdownRow, PerformanceSummary, TrendPoint } from "../api/performanceApi";
import { bucketLabel } from "../format";
import { PerformancePage } from "./PerformancePage";

// Only the network call is replaced; the queries and the page are real.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

type Query = Record<string, string | undefined>;

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function row(key: string, name: string, items: number, accuracy: number): BreakdownRow {
  return { key, name, itemsAnswered: items, itemsCorrect: Math.round(items * accuracy), accuracy, avgTimeMs: 75_000, attemptsCount: 3 };
}

const summary: PerformanceSummary = {
  track: "RN",
  attemptsCount: 4,
  itemsAnswered: 120,
  itemsCorrect: 80,
  accuracy: 0.6812,
  avgTimeMs: 81_000,
  weakest: [],
};

const breakdowns: Record<string, BreakdownRow[]> = {
  client_needs_parent: [row("SECE", "Safe & Effective Care Environment", 60, 0.7), row("PHI", "Physiological Integrity", 40, 0.55)],
  body_system: [row("cardiovascular", "Cardiovascular", 20, 0.5)],
};

function point(bucketStart: string, pct: number, items: number): TrendPoint {
  return { bucketStart, pct, items, avgTimeMs: 80_000 };
}

function serve(overrides: { summary?: PerformanceSummary } = {}) {
  getMock.mockImplementation(((path: string, init?: { params?: { query?: Query } }) => {
    const query = init?.params?.query ?? {};
    if (path === "/api/v1/me/performance/summary") return Promise.resolve(reply(200, overrides.summary ?? summary));
    if (path === "/api/v1/me/performance/breakdown") {
      return Promise.resolve(reply(200, { track: "RN", dimension: query.dimension, items: breakdowns[query.dimension!] ?? [] }));
    }
    const series =
      query.dimension === "overall"
        ? [point("2026-09-07", 0.62, 120), point("2026-09-14", 0.68, 210)]
        : [point("2026-09-14", query.key === "PHI" ? 0.4 : 0.75, 30)];
    return Promise.resolve(reply(200, { track: "RN", dimension: query.dimension, key: query.key ?? "all", bucket: query.bucket, series }));
  }) as never);
}

function trendQueries() {
  const calls = getMock.mock.calls as unknown as [string, { params: { query: Query } }][];
  return calls.filter(([path]) => path === "/api/v1/me/performance/trend").map(([, init]) => init.params.query);
}

/** The value of a stat tile, found by its label (a <dt>, role "term"). */
function stat(label: string) {
  return screen.getAllByRole("term").find((term) => term.textContent === label)?.nextElementSibling;
}

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <PerformancePage />
    </QueryClientProvider>,
  );
}

describe("PerformancePage", () => {
  beforeEach(() => getMock.mockReset());

  it("shows the totals across every finished test", async () => {
    serve();

    renderPage();

    expect(await screen.findByRole("heading", { name: "Performance" })).toBeInTheDocument();
    expect(stat("Overall score")).toHaveTextContent("68%");
    expect(stat("Questions answered")).toHaveTextContent("120");
    expect(stat("Tests finished")).toHaveTextContent("4");
    expect(stat("Avg time per question")).toHaveTextContent("1 min 21 s");
  });

  it("invites the candidate to practise before the first finished test", async () => {
    serve({ summary: { ...summary, attemptsCount: 0, itemsAnswered: 0, accuracy: 0 } });

    renderPage();

    expect(await screen.findByRole("heading", { name: "No finished tests yet" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Browse practice sets" })).toHaveAttribute("href", "/practice");
    expect(screen.queryByRole("combobox", { name: "Break down by" })).not.toBeInTheDocument();
  });

  it("lists the breakdown of the chosen dimension in a table", async () => {
    serve();
    renderPage();

    const table = await screen.findByRole("table", { name: "By Client Needs category" });
    expect(within(table).getByRole("row", { name: /Physiological Integrity 40 22 55% 1 min 15 s 3/ })).toBeInTheDocument();

    await userEvent.selectOptions(screen.getByRole("combobox", { name: "Break down by" }), "body_system");

    const bySystem = await screen.findByRole("table", { name: "By body system" });
    expect(within(bySystem).getByRole("rowheader", { name: "Cardiovascular" })).toBeInTheDocument();
  });

  it("puts every chart's numbers in a data table beside it", async () => {
    serve();
    renderPage();

    const overall = await screen.findByRole("table", { name: "Overall score: data" });
    const rows = within(overall).getAllByRole("row").slice(1);
    expect(rows.map((r) => r.textContent)).toEqual([
      `${bucketLabel("2026-09-07", "week")}62%1201 min 20 s`,
      `${bucketLabel("2026-09-14", "week")}68%2101 min 20 s`,
    ]);
    expect(screen.getByRole("img", { name: /Overall score: line chart of 2 weeks, latest 68%/ })).toBeInTheDocument();
    expect(await screen.findByRole("table", { name: "Safe & Effective Care Environment score: data" })).toHaveTextContent("75%");
  });

  it("follows the chosen category and regroups the trends by day", async () => {
    serve();
    renderPage();
    await screen.findByRole("table", { name: "Safe & Effective Care Environment score: data" });

    await userEvent.selectOptions(screen.getByRole("combobox", { name: "Category to follow" }), "PHI");
    expect(await screen.findByRole("table", { name: "Physiological Integrity score: data" })).toHaveTextContent("40%");

    await userEvent.click(screen.getByRole("radio", { name: "Day" }));

    await waitFor(() =>
      expect(trendQueries()).toEqual(
        expect.arrayContaining([
          { dimension: "overall", bucket: "day" },
          { dimension: "client_needs_parent", key: "PHI", bucket: "day" },
        ]),
      ),
    );
    expect(await screen.findAllByRole("columnheader", { name: "Day" })).toHaveLength(2);
  });

  it("says so when the performance cannot be loaded", async () => {
    getMock.mockResolvedValue(reply(500, { code: "INTERNAL" }));

    renderPage();

    expect(await screen.findByRole("alert")).toHaveTextContent("Your performance could not be loaded.");
  });
});
