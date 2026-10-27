import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import { TestSetDetailPanel } from "./TestSetDetailPanel";
import { challengeDetail } from "./testSets";

// Only the network call is replaced; the query hook and the panel are real.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function renderPanel(onClose = vi.fn()) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <TestSetDetailPanel id="panel" setId={challengeDetail.id} onClose={onClose} />
    </QueryClientProvider>,
  );
  return { onClose };
}

describe("TestSetDetailPanel", () => {
  beforeEach(() => {
    getMock.mockReset();
  });

  it("shows the set's settings and moves focus to its heading", async () => {
    getMock.mockResolvedValue(reply(200, challengeDetail));

    renderPanel();

    const heading = await screen.findByRole("heading", { name: "Comprehensive Challenge 1" });
    expect(heading).toHaveFocus();
    expect(getMock).toHaveBeenCalledWith("/api/v1/test-sets/{id}", { params: { path: { id: challengeDetail.id } } });
    expect(screen.getByText("Exam conditions: 80 minutes, answers revealed at the end.")).toBeInTheDocument();
    expect(screen.getByText("Answers at the end")).toBeInTheDocument();
    expect(screen.getByText("65% of points")).toBeInTheDocument();
    expect(screen.getByText("Unlimited")).toBeInTheDocument();
  });

  it("has a disabled Start button that says why", async () => {
    getMock.mockResolvedValue(reply(200, challengeDetail));

    renderPanel();

    const start = await screen.findByRole("button", { name: "Start" });
    expect(start).toBeDisabled();
    expect(start).toHaveAccessibleDescription("Starting a set is coming in the next update.");
  });

  it("explains that a locked set needs a premium plan", async () => {
    getMock.mockResolvedValue(reply(200, { ...challengeDetail, accessTier: "premium", locked: true }));

    renderPanel();

    expect(await screen.findByRole("button", { name: "Start" })).toHaveAccessibleDescription("This set needs a premium plan.");
    expect(screen.getByText(/Locked: premium/)).toBeInTheDocument();
  });

  it("says when the set no longer exists", async () => {
    getMock.mockResolvedValue(reply(404, { code: "NOT_FOUND", title: "That test set was not found." }));

    renderPanel();

    expect(await screen.findByRole("alert")).toHaveTextContent("This set is no longer available.");
  });

  it("closes from its Close button", async () => {
    getMock.mockResolvedValue(reply(200, challengeDetail));
    const user = userEvent.setup();
    const { onClose } = renderPanel();

    await user.click(await screen.findByRole("button", { name: "Close details" }));

    expect(onClose).toHaveBeenCalled();
  });
});
