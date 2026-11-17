import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import type { WeakAreasView } from "./api/weakAreasApi";
import { WeakAreasCard } from "./WeakAreasCard";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);

const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function renderCard() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <WeakAreasCard />
    </QueryClientProvider>,
  );
}

const weak: WeakAreasView = {
  track: "RN",
  areas: [
    { clientNeedsId: 7, code: "PHI.PPT", name: "Pharmacological and Parenteral Therapies", pct: 0.42, items: 12 },
    { clientNeedsId: 3, code: "SECE.SIC", name: "Safety and Infection Control", pct: 0.6, items: 5 },
  ],
  attemptsConsidered: 4,
  questionsAvailable: 180,
  questionCount: 25,
};

describe("Practice your weak areas card", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
    push.mockReset();
  });

  it("lists the weak areas worst first with their scores", async () => {
    getMock.mockResolvedValue(reply(200, weak));

    renderCard();

    const items = within(await screen.findByRole("list")).getAllByRole("listitem");
    expect(items.map((i) => i.textContent)).toEqual([
      "Pharmacological and Parenteral Therapies42%12 questions",
      "Safety and Infection Control60%5 questions",
    ]);
    expect(screen.getByText(/Under 65% in your last 4 tests/)).toBeInTheDocument();
    expect(screen.queryByText(/shorter than the usual/)).not.toBeInTheDocument();
  });

  it("starts the practice test and opens the player", async () => {
    getMock.mockResolvedValue(reply(200, weak));
    postMock.mockResolvedValue(reply(201, { testSet: { id: "set-1" }, attempt: { id: "attempt-7" } }));

    renderCard();
    await userEvent.click(await screen.findByRole("button", { name: "Practice 25 questions" }));

    expect(postMock).toHaveBeenCalledWith("/api/v1/test-sets/weak-areas", { body: {} });
    await waitFor(() => expect(push).toHaveBeenCalledWith("/attempts/attempt-7"));
  });

  it("warns before the click when the areas run short", async () => {
    getMock.mockResolvedValue(reply(200, { ...weak, questionsAvailable: 18, questionCount: 18 }));

    renderCard();

    expect(await screen.findByText(/Only 18 questions are available/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Practice 18 questions" })).toBeInTheDocument();
  });

  it("offers no test when the areas have too few questions", async () => {
    getMock.mockResolvedValue(reply(200, { ...weak, questionsAvailable: 3, questionCount: 0 }));

    renderCard();

    expect(await screen.findByText(/aren't enough questions in these areas/)).toBeInTheDocument();
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("says so plainly when nothing is weak, with no button", async () => {
    getMock.mockResolvedValue(reply(200, { ...weak, areas: [], questionsAvailable: 0, questionCount: 0 }));

    renderCard();

    expect(await screen.findByText(/No weak areas in your last 4 tests/)).toBeInTheDocument();
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("asks for a finished test first", async () => {
    getMock.mockResolvedValue(reply(200, { ...weak, areas: [], attemptsConsidered: 0, questionsAvailable: 0, questionCount: 0 }));

    renderCard();

    expect(await screen.findByText(/Finish a test/)).toBeInTheDocument();
  });

  it("explains an unverified email instead of starting", async () => {
    getMock.mockResolvedValue(reply(200, weak));
    postMock.mockResolvedValue(reply(403, { code: "EMAIL_NOT_VERIFIED", title: "Verify" }));

    renderCard();
    await userEvent.click(await screen.findByRole("button", { name: "Practice 25 questions" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Verify your email address to start a test.");
    expect(push).not.toHaveBeenCalled();
  });

  it("keeps the page usable when the areas can't load", async () => {
    getMock.mockResolvedValue(reply(500, { title: "boom" }));

    renderCard();

    expect(await screen.findByText(/could not be loaded/)).toBeInTheDocument();
  });
});
