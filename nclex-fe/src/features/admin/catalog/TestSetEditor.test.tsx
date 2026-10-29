import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { reply } from "../test/adminFixtures";
import { TestSetEditor } from "./TestSetEditor";
import { emptyTestSet, testSetSchema, toSettings } from "./testSetForm";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn(), PATCH: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);
const replace = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ replace }) }));

const published = [
  { id: "q1", stem: "First question", itemType: "multiple_choice", status: "active", publishedVersion: 1 },
  { id: "q2", stem: "Second question", itemType: "sata", status: "active", publishedVersion: 2 },
];

describe("TestSetEditor", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
    getMock.mockResolvedValue(reply(200, { items: published, nextCursor: null }));
  });

  it("builds an ordered set from published questions", async () => {
    postMock.mockResolvedValue(reply(201, { id: "set-1", questions: [] }));
    renderWithAuth(<TestSetEditor />);

    await userEvent.type(screen.getByRole("textbox", { name: "Title" }), "Cardiac basics");
    await userEvent.click(await screen.findByRole("button", { name: "Add: First question" }));
    await userEvent.click(screen.getByRole("button", { name: "Add: Second question" }));
    await userEvent.click(screen.getByRole("button", { name: "Move up: Second question" }));
    await userEvent.click(screen.getByRole("button", { name: "Create set" }));

    await vi.waitFor(() => expect(postMock).toHaveBeenCalled());
    const [path, init] = postMock.mock.calls[0] as unknown as [string, { body: { questionIds: string[]; settings: { title: string } } }];
    expect(path).toBe("/api/v1/admin/test-sets");
    expect(init.body.questionIds).toEqual(["q2", "q1"]);
    expect(init.body.settings.title).toBe("Cardiac basics");
    await vi.waitFor(() => expect(replace).toHaveBeenCalledWith("/admin/test-sets/set-1"));
  });

  it("names the question the server refused", async () => {
    postMock.mockResolvedValue(
      reply(400, { code: "VALIDATION_FAILED", title: "x", errors: [{ field: "questionIds[0]", code: "UNKNOWN_ID", message: "Question q1 is not a published, active RN question." }] }),
    );
    renderWithAuth(<TestSetEditor />);
    await userEvent.type(screen.getByRole("textbox", { name: "Title" }), "Set");
    await userEvent.click(await screen.findByRole("button", { name: "Add: First question" }));
    await userEvent.click(screen.getByRole("button", { name: "Create set" }));

    expect(await screen.findByRole("alert")).toHaveTextContent('"First question": Question q1 is not a published, active RN question.');
  });
});

describe("testSetForm", () => {
  it("needs a timer for a timed set and none for an untimed one", () => {
    expect(testSetSchema.safeParse({ ...emptyTestSet("RN"), title: "T", timed: true }).success).toBe(false);
    expect(testSetSchema.safeParse({ ...emptyTestSet("RN"), title: "T", timed: false, timeBudgetSec: "600" }).success).toBe(false);
    expect(testSetSchema.safeParse({ ...emptyTestSet("RN"), title: "T", timed: true, timeBudgetSec: "600" }).success).toBe(true);
  });

  it("turns the pass percentage into a share of points", () => {
    expect(toSettings({ ...emptyTestSet("RN"), title: "T", passPercent: "65" }).passThreshold).toBe(0.65);
  });
});
