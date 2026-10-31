import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { mcVersion, question, reply, staffUser } from "../test/adminFixtures";
import { SubtopicPicker } from "./SubtopicPicker";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), PUT: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const putMock = vi.mocked(apiClient.PUT);

function subtopic(id: string, name: string, isActive = true) {
  return { id, track: "RN", clientNeedsId: 2, clientNeedsCode: "SECE.MOC", clientNeedsName: "Management of Care", slug: id, name, isActive, sortOrder: 0, questionCount: 0 };
}

const items = [subtopic("s1", "Delegation"), subtopic("s2", "Prioritizing"), subtopic("s3", "Advance directives"), subtopic("s4", "Consent"), subtopic("s5", "Retired topic", false)];
const draft = question([mcVersion("draft")]);

function render(view = draft, editable = true) {
  renderWithAuth(<SubtopicPicker question={view} editable={editable} />, { status: "authenticated" });
}

describe("SubtopicPicker", () => {
  beforeEach(() => {
    getMock.mockReset();
    putMock.mockReset();
    getMock.mockImplementation(((path: string) =>
      Promise.resolve(path === "/api/v1/me" ? reply(200, staffUser("author")) : reply(200, { items }))) as never);
    putMock.mockResolvedValue(reply(200, draft));
  });

  it("saves the chosen subtopics using the keyboard", async () => {
    render();
    const user = userEvent.setup();

    (await screen.findByRole("checkbox", { name: "Delegation" })).focus();
    await user.keyboard(" ");
    await user.click(screen.getByRole("button", { name: "Save subtopics" }));

    expect(putMock).toHaveBeenCalledWith("/api/v1/admin/questions/{id}/subtopics", {
      params: { path: { id: draft.id } },
      body: { subtopicIds: ["s1"] },
    });
    expect(await screen.findByText("Saved.")).toBeInTheDocument();
  });

  it("hides inactive subtopics and stops at three", async () => {
    render();

    await userEvent.click(await screen.findByRole("checkbox", { name: "Delegation" }));
    await userEvent.click(screen.getByRole("checkbox", { name: "Prioritizing" }));
    await userEvent.click(screen.getByRole("checkbox", { name: "Advance directives" }));

    expect(screen.queryByRole("checkbox", { name: "Retired topic" })).not.toBeInTheDocument();
    expect(screen.getByRole("checkbox", { name: "Consent" })).toBeDisabled();
  });

  it("needs at least one subtopic to save", async () => {
    render();

    await screen.findByRole("checkbox", { name: "Delegation" });

    expect(screen.getByRole("button", { name: "Save subtopics" })).toBeDisabled();
  });

  it("points out a tag that no longer fits", async () => {
    render({ ...draft, subtopics: [{ id: "old", name: "Old topic", clientNeedsId: 9, isActive: true }] });

    expect(await screen.findByText(/No longer usable here: Old topic/)).toBeInTheDocument();
  });

  it("only lists the tags for people who can't edit them", async () => {
    render({ ...draft, subtopics: [{ id: "s1", name: "Delegation", clientNeedsId: 2, isActive: true }] }, false);

    expect(await screen.findByText("Delegation")).toBeInTheDocument();
    expect(screen.queryByRole("checkbox")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Save subtopics" })).not.toBeInTheDocument();
  });
});
