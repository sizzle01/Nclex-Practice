import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { authorId, mcVersion, question, reply, reviewerId, staffUser } from "../test/adminFixtures";
import { WorkflowPanel } from "./WorkflowPanel";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);

async function renderPanel(role: Parameters<typeof staffUser>[0], userId: string, status: Parameters<typeof mcVersion>[0]) {
  getMock.mockResolvedValue(reply(200, staffUser(role, userId)));
  const version = mcVersion(status);
  const onChanged = vi.fn();
  renderWithAuth(<WorkflowPanel question={question([version])} version={version} onChanged={onChanged} />, { status: "authenticated" });
  await screen.findByRole("heading", { name: /Version 1/ });
  // useMe resolves after the first render.
  await vi.waitFor(() => expect(getMock).toHaveBeenCalled());
  return { version, onChanged };
}

describe("WorkflowPanel", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
  });

  it("lets the author submit a draft for review", async () => {
    const { version, onChanged } = await renderPanel("author", authorId, "draft");
    const updated = question([mcVersion("in_review")]);
    postMock.mockResolvedValue(reply(200, updated));

    await userEvent.click(await screen.findByRole("button", { name: "Submit for review" }));

    expect(postMock).toHaveBeenCalledWith("/api/v1/admin/question-versions/{id}/submit-for-review", { params: { path: { id: version.id } } });
    await vi.waitFor(() => expect(onChanged).toHaveBeenCalledWith(updated));
  });

  it("offers approve and reject to another reviewer, and reject needs notes", async () => {
    const { version } = await renderPanel("reviewer", reviewerId, "in_review");
    postMock.mockResolvedValue(reply(200, question([mcVersion("rejected")])));

    const reject = await screen.findByRole("button", { name: "Reject" });
    expect(reject).toBeDisabled();
    await userEvent.type(screen.getByRole("textbox", { name: "Review notes (required to reject)" }), "Option c is also right.");
    await userEvent.click(reject);

    expect(postMock).toHaveBeenCalledWith("/api/v1/admin/question-versions/{id}/reject", {
      params: { path: { id: version.id } },
      body: { notes: "Option c is also right." },
    });
  });

  it("does not let a reviewer review their own version", async () => {
    await renderPanel("admin", authorId, "in_review");

    expect(await screen.findByText("You wrote this version, so another reviewer must review it.")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Approve" })).not.toBeInTheDocument();
  });

  it("shows the server's reasons when publishing is refused", async () => {
    await renderPanel("reviewer", reviewerId, "approved");
    postMock.mockResolvedValue(
      reply(400, {
        code: "VALIDATION_FAILED",
        title: "Some fields are missing or invalid.",
        errors: [{ field: "exhibits[0].mediaId.altText", code: "REQUIRED", message: "An image needs alt text." }],
      }),
    );

    await userEvent.click(await screen.findByRole("button", { name: "Publish" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("An image needs alt text.");
  });

  it("asks before retiring", async () => {
    await renderPanel("reviewer", reviewerId, "published");
    postMock.mockResolvedValue(reply(200, question([mcVersion("published")], { status: "retired" })));

    await userEvent.click(await screen.findByRole("button", { name: "Retire question" }));
    await userEvent.click(screen.getByRole("button", { name: "Yes, retire it" }));

    expect(postMock).toHaveBeenCalledWith("/api/v1/admin/questions/{id}/retire", expect.anything());
  });
});
