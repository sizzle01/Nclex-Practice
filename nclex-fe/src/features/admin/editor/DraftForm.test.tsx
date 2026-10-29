import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { testTaxonomy } from "@/features/custom-builder/components/testTaxonomy";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import { expectNoAxeViolations } from "@/test/axe";
import { mcVersion, reply } from "../test/adminFixtures";
import { DraftForm } from "./DraftForm";
import { emptyFormValues, formValuesFrom } from "./draftValues";

vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn(), use: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

describe("DraftForm", () => {
  beforeEach(() => {
    getMock.mockReset();
    getMock.mockResolvedValue(reply(200, testTaxonomy));
  });

  it("checks the simple fields before sending anything", async () => {
    const onSave = vi.fn();
    renderWithAuth(<DraftForm track="RN" initialValues={emptyFormValues("multiple_choice")} readOnly={false} saveLabel="Create draft" onSave={onSave} />);

    await userEvent.click(screen.getByRole("button", { name: "Create draft" }));

    expect(await screen.findByText("Write the question stem.")).toBeInTheDocument();
    expect(screen.getByText("Choose a Client Needs subcategory.")).toBeInTheDocument();
    expect(onSave).not.toHaveBeenCalled();
  });

  it("shows the stem in the candidate preview as it is typed", async () => {
    renderWithAuth(<DraftForm track="RN" initialValues={emptyFormValues("sata")} readOnly={false} saveLabel="Save draft" onSave={vi.fn()} />);

    await userEvent.type(screen.getByRole("textbox", { name: "Stem" }), "Which findings need follow-up?");

    const preview = screen.getByRole("region", { name: "Candidate preview" });
    expect(within(preview).getByText("Which findings need follow-up?")).toBeInTheDocument();
  });

  it("puts server problems next to their fields and lists the rest", async () => {
    const onSave = vi.fn().mockRejectedValue(
      new ApiError(400, "VALIDATION_FAILED", [
        { field: "stem", code: "REQUIRED", message: "Write the question stem." },
        { field: "rationale.perOption.c", code: "MISSING_RATIONALE", message: 'Option "c" needs a rationale.' },
      ], "Some fields are missing or invalid."),
    );
    renderWithAuth(<DraftForm track="RN" initialValues={formValuesFrom(mcVersion("draft"))} readOnly={false} saveLabel="Save draft" onSave={onSave} />);

    await userEvent.click(screen.getByRole("button", { name: "Save draft" }));

    expect(await screen.findByRole("list", { name: "Problems found by the server" })).toHaveTextContent('Option "c" needs a rationale.');
    expect(screen.getByText("Write the question stem.")).toBeInTheDocument();
  });

  it("sends the draft in the API's shape", async () => {
    const onSave = vi.fn().mockResolvedValue(undefined);
    renderWithAuth(<DraftForm track="RN" initialValues={formValuesFrom(mcVersion("draft"))} readOnly={false} saveLabel="Save draft" onSave={onSave} />);

    await userEvent.click(screen.getByRole("button", { name: "Save draft" }));

    await vi.waitFor(() => expect(onSave).toHaveBeenCalled());
    expect(onSave.mock.calls[0][0]).toMatchObject({ itemType: "multiple_choice", answerKey: { correct: "b" }, clientNeedsId: 2 });
    expect(await screen.findByText("Saved.")).toBeInTheDocument();
  });

  it("is read-only for a version that left draft", async () => {
    const { container } = renderWithAuth(
      <DraftForm track="RN" initialValues={formValuesFrom(mcVersion("published"))} readOnly saveLabel="Save draft" onSave={vi.fn()} />,
    );

    expect(screen.getByRole("textbox", { name: "Stem" })).toBeDisabled();
    expect(screen.queryByRole("button", { name: "Save draft" })).not.toBeInTheDocument();
    await expectNoAxeViolations(container);
  });
});
