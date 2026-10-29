import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithAuth } from "@/features/auth/test/renderWithAuth";
import { ApiError } from "@/lib/api/errors";
import { expectNoAxeViolations } from "@/test/axe";
import { reportIssue } from "../api/flagsApi";
import { aMyFlag } from "../test/flags";
import { ReportIssueButton } from "./ReportIssueButton";
import { ReportIssueDialog } from "./ReportIssueDialog";

vi.mock("../api/flagsApi", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/flagsApi")>()),
  reportIssue: vi.fn(),
}));
const reportMock = vi.mocked(reportIssue);

describe("ReportIssueDialog", () => {
  beforeEach(() => {
    reportMock.mockReset().mockResolvedValue(aMyFlag());
  });

  it("sends the reason, the details and the attempt, then thanks the candidate", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ReportIssueDialog questionVersionId="v-1" attemptId="a-1" onClose={vi.fn()} />, { status: "authenticated" });

    await user.click(screen.getByRole("radio", { name: "The answer seems wrong" }));
    await user.type(screen.getByRole("textbox", { name: "Details (optional)" }), "C is also right.");
    await user.click(screen.getByRole("button", { name: "Send report" }));

    expect(await screen.findByRole("status")).toHaveTextContent("your report was sent");
    expect(reportMock).toHaveBeenCalledWith({
      questionVersionId: "v-1",
      reason: "incorrect_answer_key",
      comment: "C is also right.",
      attemptId: "a-1",
    });
    expect(screen.getByRole("link", { name: "My reports" })).toHaveAttribute("href", "/settings/reports");
  });

  it("asks for a reason before sending", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ReportIssueDialog questionVersionId="v-1" onClose={vi.fn()} />, { status: "authenticated" });

    await user.click(screen.getByRole("button", { name: "Send report" }));

    expect(await screen.findByText("Choose what is wrong.")).toBeInTheDocument();
    expect(reportMock).not.toHaveBeenCalled();
  });

  it("explains a duplicate report", async () => {
    reportMock.mockRejectedValue(new ApiError(409, "FLAG_DUPLICATE", [], "Duplicate"));
    const user = userEvent.setup();
    renderWithAuth(<ReportIssueDialog questionVersionId="v-1" onClose={vi.fn()} />, { status: "authenticated" });

    await user.click(screen.getByRole("radio", { name: "Typo or wording mistake" }));
    await user.click(screen.getByRole("button", { name: "Send report" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("already reported this question for that reason");
  });

  it("keeps Tab inside the dialog and closes on Escape", async () => {
    const onClose = vi.fn();
    const user = userEvent.setup();
    renderWithAuth(<ReportIssueDialog questionVersionId="v-1" onClose={onClose} />, { status: "authenticated" });
    const first = screen.getByRole("radio", { name: "The rationale is unclear" });
    expect(first).toHaveFocus();

    await user.click(screen.getByRole("button", { name: "Send report" }));
    screen.getByRole("button", { name: "Send report" }).focus();
    await user.tab();
    expect(first).toHaveFocus();

    await user.keyboard("{Escape}");
    expect(onClose).toHaveBeenCalled();
  });

  it("has no accessibility violations", async () => {
    const { container } = renderWithAuth(<ReportIssueDialog questionVersionId="v-1" onClose={vi.fn()} />, {
      status: "authenticated",
    });

    await expectNoAxeViolations(container);
  });
});

describe("ReportIssueButton", () => {
  it("opens the dialog and returns focus to the button when it closes", async () => {
    const user = userEvent.setup();
    renderWithAuth(<ReportIssueButton questionVersionId="v-1" />, { status: "authenticated" });
    const button = screen.getByRole("button", { name: "Report issue" });

    await user.click(button);
    expect(screen.getByRole("dialog", { name: "Report an issue" })).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Cancel" }));

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    expect(button).toHaveFocus();
  });
});
