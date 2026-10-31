import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { testTaxonomy } from "@/features/custom-builder/components/testTaxonomy";
import { apiClient } from "@/lib/api/client";
import { reply, staffUser } from "../test/adminFixtures";
import { SubtopicAdmin } from "./SubtopicAdmin";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), POST: vi.fn(), PATCH: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);
const postMock = vi.mocked(apiClient.POST);
const patchMock = vi.mocked(apiClient.PATCH);

const subtopic = {
  id: "s1",
  track: "RN",
  clientNeedsId: 2,
  clientNeedsCode: "SECE.MOC",
  clientNeedsName: "Management of Care",
  slug: "delegation",
  name: "Delegation",
  isActive: true,
  sortOrder: 0,
  questionCount: 4,
};

describe("SubtopicAdmin", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
    patchMock.mockReset();
    getMock.mockImplementation(((path: string) => {
      if (path === "/api/v1/me") return Promise.resolve(reply(200, staffUser("reviewer")));
      if (path === "/api/v1/taxonomy") return Promise.resolve(reply(200, testTaxonomy));
      return Promise.resolve(reply(200, { items: [subtopic] }));
    }) as never);
    postMock.mockResolvedValue(reply(201, subtopic));
    patchMock.mockResolvedValue(reply(200, subtopic));
  });

  it("adds a subtopic to a chosen Client Needs subcategory", async () => {
    renderWithAuth(<SubtopicAdmin />, { status: "authenticated" });
    const select = await screen.findByRole("combobox", { name: "Client Needs subcategory" });
    const [, leaf] = await screen.findAllByRole("option", { name: /›/ });

    await userEvent.selectOptions(select, leaf);
    await userEvent.type(screen.getByRole("textbox", { name: "Name" }), "Consent");
    await userEvent.click(screen.getByRole("button", { name: "Add subtopic" }));

    expect(postMock).toHaveBeenCalledWith("/api/v1/admin/subtopics", {
      body: { clientNeedsId: Number((leaf as HTMLOptionElement).value), name: "Consent", sortOrder: 0 },
    });
  });

  it("shows why a duplicate name was refused", async () => {
    postMock.mockResolvedValue(reply(409, { code: "SUBTOPIC_DUPLICATE", title: "This track already has a subtopic with that name." }));
    renderWithAuth(<SubtopicAdmin />, { status: "authenticated" });
    const [, leaf] = await screen.findAllByRole("option", { name: /›/ });

    await userEvent.selectOptions(screen.getByRole("combobox", { name: "Client Needs subcategory" }), leaf);
    await userEvent.type(screen.getByRole("textbox", { name: "Name" }), "Delegation");
    await userEvent.click(screen.getByRole("button", { name: "Add subtopic" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("already has a subtopic");
  });

  it("deactivates a subtopic instead of deleting it", async () => {
    renderWithAuth(<SubtopicAdmin />, { status: "authenticated" });

    await userEvent.click(await screen.findByRole("button", { name: "Deactivate Delegation" }));

    expect(patchMock).toHaveBeenCalledWith("/api/v1/admin/subtopics/{id}", {
      params: { path: { id: "s1" } },
      body: { name: null, sortOrder: null, isActive: false },
    });
  });

  it("renames a subtopic", async () => {
    renderWithAuth(<SubtopicAdmin />, { status: "authenticated" });
    const name = await screen.findByRole("textbox", { name: "Name of Delegation" });

    await userEvent.clear(name);
    await userEvent.type(name, "Delegation and supervision");
    await userEvent.click(screen.getByRole("button", { name: "Rename" }));

    expect(patchMock).toHaveBeenCalledWith("/api/v1/admin/subtopics/{id}", {
      params: { path: { id: "s1" } },
      body: { name: "Delegation and supervision", sortOrder: null, isActive: null },
    });
  });
});
