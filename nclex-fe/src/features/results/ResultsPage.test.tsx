import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { AttemptResultView } from "../attempt/api/attemptApi";
import { elapsed } from "./format";
import { ResultsPage } from "./ResultsPage";

// Only the network call is replaced; the query and the page are real.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

const disclaimer = "This is an estimate based on your practice score, not a prediction of your NCLEX result.";

function row(key: string, name: string, items: number, correct: number, earned: number, max: number) {
  return { key, name, items, correct, pointsEarned: earned, maxPoints: max, pct: earned / max, avgTimeMs: 81_000 };
}

const result: AttemptResultView = {
  attemptId: "attempt-1",
  title: "Comprehensive Challenge Exam 1",
  submittedAt: "2026-10-28T10:00:00Z",
  submitReason: "user",
  score: { pointsEarned: 68, maxPoints: 92, pct: 0.739, itemsCorrect: 52, itemsAnswered: 74, itemsTotal: 75 },
  time: { totalMs: 6_120_000, avgPerItemMs: 81_600 },
  readiness: { passThreshold: 0.65, passed: true, label: "likely_ready", disclaimer },
  breakdown: {
    client_needs: [row("SECE.MOC", "Management of Care", 12, 8, 8.5, 12), row("PHI.PPT", "Pharmacological and Parenteral Therapies", 10, 4, 4.8, 10)],
    client_needs_parent: [row("SECE", "Safe & Effective Care Environment", 12, 8, 8.5, 12)],
    body_system: [],
    item_type: [row("sata", "Select All That Apply", 20, 10, 40, 60)],
    cjmm_skill: [],
    difficulty: [],
  },
  weakest: [{ dimension: "client_needs", key: "PHI.PPT", name: "Pharmacological and Parenteral Therapies", pct: 0.48 }],
};

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <ResultsPage attemptId="attempt-1" />
    </QueryClientProvider>,
  );
}

describe("ResultsPage", () => {
  beforeEach(() => getMock.mockReset());

  it("shows the score, time and the readiness estimate with its disclaimer", async () => {
    getMock.mockResolvedValue(reply(200, result));

    renderPage();

    expect(await screen.findByRole("heading", { name: "Results: Comprehensive Challenge Exam 1" })).toBeInTheDocument();
    const score = screen.getByRole("region", { name: "Your score" });
    expect(score).toHaveTextContent("74%");
    expect(score).toHaveTextContent("68 of 92 points");
    expect(score).toHaveTextContent("52 of 75 questions fully correct");
    expect(score).toHaveTextContent("1 h 42 min in total, 1 min 22 s per question on average");
    expect(score).toHaveTextContent("Likely ready");
    expect(score).toHaveTextContent("(passing threshold 65%)");
    expect(score).toHaveTextContent(disclaimer);
  });

  it("leaves out the estimate when the set has no threshold", async () => {
    getMock.mockResolvedValue(reply(200, { ...result, readiness: null }));

    renderPage();

    expect(await screen.findByRole("region", { name: "Your score" })).not.toHaveTextContent("ready");
    expect(screen.queryByText(disclaimer)).not.toBeInTheDocument();
  });

  it("lists the weakest areas and a table per dimension with partial-credit points", async () => {
    getMock.mockResolvedValue(reply(200, result));

    renderPage();

    const weakest = await screen.findByRole("region", { name: "Focus on these next" });
    expect(within(weakest).getByRole("listitem")).toHaveTextContent("Pharmacological and Parenteral Therapies: 48%");
    const subcategories = screen.getByRole("table", { name: "By Client Needs subcategory" });
    const moc = within(subcategories).getByRole("row", { name: /Management of Care/ });
    expect(within(moc).getAllByRole("cell").map((c) => c.textContent)).toEqual(["12", "8", "8.5 / 12", "71%", "1 min 21 s"]);
    expect(screen.getByRole("table", { name: "By question type" })).toBeInTheDocument();
    // Dimensions with no rows (no CJMM-tagged questions here) are left out.
    expect(screen.queryByRole("table", { name: "By clinical judgment step" })).not.toBeInTheDocument();
  });

  it("links to the review, including incorrect answers only", async () => {
    getMock.mockResolvedValue(reply(200, result));

    renderPage();

    expect(await screen.findByRole("link", { name: "Review all answers" })).toHaveAttribute("href", "/attempts/attempt-1/review");
    expect(screen.getByRole("link", { name: "Review incorrect answers" })).toHaveAttribute(
      "href",
      "/attempts/attempt-1/review?filter=incorrect",
    );
  });

  it("notes when the time ran out", async () => {
    getMock.mockResolvedValue(reply(200, { ...result, submitReason: "time_expired" }));

    renderPage();

    expect(await screen.findByText(/when the time ran out/)).toBeInTheDocument();
  });

  it("explains a test that has not been submitted yet", async () => {
    getMock.mockResolvedValue(reply(409, { code: "ATTEMPT_NOT_ACTIVE", title: "Not submitted." }));

    renderPage();

    expect(await screen.findByRole("alert")).toHaveTextContent("hasn't been submitted yet");
    expect(screen.getByRole("link", { name: "Back to the test" })).toHaveAttribute("href", "/attempts/attempt-1");
  });
});

describe("elapsed", () => {
  it.each([
    [45_000, "45 s"],
    [81_600, "1 min 22 s"],
    [120_000, "2 min"],
    [6_120_000, "1 h 42 min"],
    [7_200_000, "2 h"],
  ])("formats %i ms as %s", (ms, text) => {
    expect(elapsed(ms)).toBe(text);
  });
});
