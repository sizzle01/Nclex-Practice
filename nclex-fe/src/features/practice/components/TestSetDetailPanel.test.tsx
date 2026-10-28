import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import { TestSetDetailPanel } from "./TestSetDetailPanel";
import { challengeDetail } from "./testSets";

// Only the network call is replaced; the query hook and the panel are real.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);
const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

function renderPanel(onClose = vi.fn()) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
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
    postMock.mockReset();
    push.mockReset();
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

  it("starts the set and opens the player", async () => {
    getMock.mockResolvedValue(reply(200, challengeDetail));
    postMock.mockResolvedValue(reply(201, { id: "attempt-1" }));
    renderPanel();

    await userEvent.click(await screen.findByRole("button", { name: "Start" }));

    expect(postMock).toHaveBeenCalledWith("/api/v1/attempts", { body: { testSetId: challengeDetail.id } });
    expect(push).toHaveBeenCalledWith("/attempts/attempt-1");
  });

  it("asks an unverified user to verify before starting", async () => {
    getMock.mockResolvedValue(reply(200, challengeDetail));
    postMock.mockResolvedValue(reply(403, { code: "EMAIL_NOT_VERIFIED", title: "Verify your email address to continue." }));
    renderPanel();

    await userEvent.click(await screen.findByRole("button", { name: "Start" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Verify your email address to start this set.");
    expect(push).not.toHaveBeenCalled();
  });

  it("explains that a locked set needs a premium plan", async () => {
    getMock.mockResolvedValue(reply(200, { ...challengeDetail, accessTier: "premium", locked: true }));

    renderPanel();

    const start = await screen.findByRole("button", { name: "Start" });
    expect(start).toBeDisabled();
    expect(start).toHaveAccessibleDescription("This set needs a premium plan.");
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
