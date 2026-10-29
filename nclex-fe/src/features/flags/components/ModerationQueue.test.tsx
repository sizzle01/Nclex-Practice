import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { fetchMe } from "@/features/auth/api/authApi";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { ApiError } from "@/lib/api/errors";
import { fetchFlagQueue, updateFlag } from "../api/flagsApi";
import { aProfile, aQueueItem } from "../test/flags";
import { ModerationQueue } from "./ModerationQueue";
import { RequireModerator } from "./RequireModerator";

vi.mock("next/navigation", () => ({ useRouter: () => ({ replace: vi.fn() }), usePathname: () => "/admin/flags" }));
vi.mock("@/features/auth/api/authApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@/features/auth/api/authApi")>()),
  fetchMe: vi.fn(),
}));
vi.mock("../api/flagsApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/flagsApi")>()),
  fetchFlagQueue: vi.fn(),
  updateFlag: vi.fn(),
}));
const meMock = vi.mocked(fetchMe);
const queueMock = vi.mocked(fetchFlagQueue);
const updateMock = vi.mocked(updateFlag);

describe("ModerationQueue", () => {
  beforeEach(() => {
    meMock.mockReset().mockResolvedValue(aProfile("reviewer"));
    queueMock.mockReset().mockImplementation(async (status) => ({
      items: status === "open" ? [aQueueItem()] : [aQueueItem({ id: "t", status: "triaged", reason: "typo" })],
      nextCursor: null,
    }));
    updateMock.mockReset().mockResolvedValue(aQueueItem());
  });

  it("lists open reports with a link to the version in the editor", async () => {
    renderWithAuth(<ModerationQueue />, { status: "authenticated" });

    const card = (await screen.findByRole("heading", { name: "The rationale is unclear" })).closest("article")!;
    expect(within(card).getByText("The rationale skips a step.")).toBeInTheDocument();
    expect(within(card).getByText("Ada")).toBeInTheDocument();
    expect(within(card).getByRole("link", { name: "Open in editor" })).toHaveAttribute("href", "/admin/questions/q-1?version=v-1");
    expect(queueMock).toHaveBeenCalledWith("open", null);
  });

  it("switches to another status", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ModerationQueue />, { status: "authenticated" });
    await screen.findByText("The rationale is unclear");

    await user.click(screen.getByRole("button", { name: "Triaged" }));

    expect(await screen.findByRole("heading", { name: "Typo or wording mistake" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Triaged" })).toHaveAttribute("aria-pressed", "true");
  });

  it("assigns a report to me and triages it", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ModerationQueue />, { status: "authenticated" });

    await user.click(await screen.findByRole("button", { name: "Assign to me" }));
    await user.click(screen.getByRole("button", { name: "Triage" }));

    expect(updateMock).toHaveBeenCalledWith("f-1", { assigneeId: "me-1" });
    expect(updateMock).toHaveBeenCalledWith("f-1", { status: "triaged" });
  });

  it("needs a note to resolve, then sends it", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ModerationQueue />, { status: "authenticated" });

    await user.click(await screen.findByRole("button", { name: "Resolve…" }));
    await user.click(screen.getByRole("button", { name: "Resolve and notify" }));
    expect(await screen.findByText(/Write a note/)).toBeInTheDocument();
    expect(updateMock).not.toHaveBeenCalled();

    await user.type(screen.getByRole("textbox", { name: "Note to the reporter" }), "Fixed the rationale.");
    await user.click(screen.getByRole("button", { name: "Resolve and notify" }));

    expect(updateMock).toHaveBeenCalledWith("f-1", { status: "resolved", resolutionNote: "Fixed the rationale." });
  });

  it("explains when someone else closed the report first", async () => {
    updateMock.mockRejectedValue(new ApiError(409, "FLAG_CLOSED", [], "Closed"));
    const user = userEvent.setup();
    renderWithAuth(<ModerationQueue />, { status: "authenticated" });

    await user.click(await screen.findByRole("button", { name: "Reject…" }));
    await user.type(screen.getByRole("textbox", { name: "Why no change is needed" }), "Matches guidance.");
    await user.click(screen.getByRole("button", { name: "Reject report" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("already closed");
  });
});

describe("RequireModerator", () => {
  it("keeps candidates out", async () => {
    meMock.mockReset().mockResolvedValue(aProfile("candidate"));
    renderWithAuth(<RequireModerator>secret queue</RequireModerator>, { status: "authenticated" });

    expect(await screen.findByRole("alert")).toHaveTextContent("for reviewers and admins");
    expect(screen.queryByText("secret queue")).not.toBeInTheDocument();
  });

  it("lets admins in", async () => {
    meMock.mockReset().mockResolvedValue(aProfile("admin"));
    renderWithAuth(<RequireModerator>secret queue</RequireModerator>, { status: "authenticated" });

    expect(await screen.findByText("secret queue")).toBeInTheDocument();
  });
});
