import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { CandidateItem, ReviewPage as ReviewPageBody } from "../attempt/api/attemptApi";
import { ReviewPage } from "./ReviewPage";

// Only the network call is replaced; the query, the page and the renderers are real.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function reviewed(position: number, overrides: Partial<CandidateItem> = {}): CandidateItem {
  return {
    position,
    questionVersionId: `v-${position}`,
    itemType: "multiple_choice",
    stem: `Question text ${position}`,
    content: { options: [{ id: "a", text: "Option A" }, { id: "b", text: "Option B" }] },
    exhibits: [],
    caseStudy: null,
    state: "locked",
    response: { selected: "a" },
    clientSeq: 1,
    flagged: false,
    struckOptions: [],
    timeAllotmentSec: 120,
    reveal: {
      answerKey: { correct: "b" },
      grade: {
        points: 0,
        maxPoints: 1,
        isCorrect: false,
        detail: [
          { target: "a", delta: 0, selected: "a", missed: false },
          { target: "b", delta: 0, missed: true },
        ],
      },
      rationale: { correct: `Why B for ${position}`, perOption: { a: "A is not first.", b: "B is first." } },
      educationalObjective: "Prioritise.",
      tags: { clientNeeds: "SECE.MOC", bodySystem: "fundamentals", cjmmSkill: null, difficulty: "easy" },
    },
    ...overrides,
  };
}

function page(items: CandidateItem[], nextCursor: string | null = null): ReviewPageBody {
  return { items, nextCursor };
}

type Query = { filter: string; cursor?: string };

function queriesSent() {
  return (getMock.mock.calls as unknown as [string, { params: { query: Query } }][]).map(([, init]) => init.params.query);
}

function renderPage(initialFilter: "all" | "incorrect" | "flagged" = "all") {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <ReviewPage attemptId="attempt-1" initialFilter={initialFilter} />
    </QueryClientProvider>,
  );
}

describe("ReviewPage", () => {
  beforeEach(() => getMock.mockReset());

  it("shows each question with the answer marked and the rationale", async () => {
    getMock.mockResolvedValue(reply(200, page([reviewed(1), reviewed(2, { flagged: true })])));

    renderPage();

    const first = await screen.findByRole("article", { name: "Question 1" });
    expect(within(first).getByRole("radio", { name: "Option A ✗ Incorrect" })).toBeChecked();
    expect(within(first).getByRole("radio", { name: "Option A ✗ Incorrect" })).toBeDisabled();
    expect(within(first).getByRole("radio", { name: "Option B Correct answer, not selected" })).toBeInTheDocument();
    expect(within(first).getByRole("region", { name: "Rationale" })).toHaveTextContent("Why B for 1");
    expect(screen.getByRole("article", { name: "Question 2 Marked for review" })).toBeInTheDocument();
  });

  it("starts with the filter from the link and switches filters", async () => {
    getMock.mockResolvedValue(reply(200, page([reviewed(1)])));
    renderPage("incorrect");
    await screen.findByRole("article", { name: "Question 1" });
    expect(screen.getByRole("button", { name: "Incorrect" })).toHaveAttribute("aria-pressed", "true");

    await userEvent.click(screen.getByRole("button", { name: "Marked for review" }));

    expect(screen.getByRole("button", { name: "Marked for review" })).toHaveAttribute("aria-pressed", "true");
    expect(queriesSent().map((q) => q.filter)).toEqual(["incorrect", "flagged"]);
  });

  it("loads the next page with the cursor", async () => {
    getMock
      .mockResolvedValueOnce(reply(200, page([reviewed(1)], "o:1")))
      .mockResolvedValueOnce(reply(200, page([reviewed(2)])));
    renderPage();
    await screen.findByRole("article", { name: "Question 1" });

    await userEvent.click(screen.getByRole("button", { name: "Show more" }));

    expect(await screen.findByRole("article", { name: "Question 2" })).toBeInTheDocument();
    expect(queriesSent()[1]).toEqual({ filter: "all", cursor: "o:1" });
    expect(screen.queryByRole("button", { name: "Show more" })).not.toBeInTheDocument();
  });

  it("says when a filter has nothing to show", async () => {
    getMock.mockResolvedValue(reply(200, page([])));

    renderPage("incorrect");

    expect(await screen.findByText("No incorrect answers. Well done!")).toBeInTheDocument();
  });

  it("marks a question left blank", async () => {
    getMock.mockResolvedValue(reply(200, page([reviewed(1, { response: null })])));

    renderPage();

    expect(await screen.findByText("Not answered.")).toBeInTheDocument();
  });

  it("explains that answers come after submitting an exam-mode test", async () => {
    getMock.mockResolvedValue(reply(409, { code: "ATTEMPT_NOT_ACTIVE", title: "Not submitted." }));

    renderPage();

    expect(await screen.findByRole("alert")).toHaveTextContent("Answers are shown after you submit this test.");
  });
});
