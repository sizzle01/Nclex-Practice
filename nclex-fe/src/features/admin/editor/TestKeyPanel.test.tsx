import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { mcVersion, reply } from "../test/adminFixtures";
import { TestKeyPanel } from "./TestKeyPanel";

vi.mock("@/lib/api/client", () => ({ apiClient: { POST: vi.fn(), use: vi.fn() } }));
const postMock = vi.mocked(apiClient.POST);

describe("TestKeyPanel", () => {
  beforeEach(() => postMock.mockReset());

  it("grades a sample answer with the real grader and shows the rationale", async () => {
    const version = mcVersion("in_review");
    postMock.mockResolvedValue(
      reply(200, { points: 1, maxPoints: 1, isCorrect: true, detail: [{ target: "b", delta: 1, selected: "b" }] }),
    );
    renderWithAuth(<TestKeyPanel version={version} />);

    await userEvent.click(screen.getByRole("radio", { name: /Option b/ }));
    await userEvent.click(screen.getByRole("button", { name: "Grade this answer" }));

    expect(postMock).toHaveBeenCalledWith("/api/v1/admin/question-versions/{id}/preview-grade", {
      params: { path: { id: version.id } },
      body: { response: { selected: "b" } },
    });
    expect(await screen.findByRole("heading", { name: "Correct: 1 of 1 point" })).toBeInTheDocument();
    expect(screen.getByText("Why c")).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: "Try another answer" }));
    expect(screen.getByRole("button", { name: "Grade this answer" })).toBeInTheDocument();
  });
});
