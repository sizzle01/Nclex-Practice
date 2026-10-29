import type { FlagReason, FlagStatus } from "./api/flagsApi";

/** Reasons in the order of the database enum flag_reason, with the words a candidate sees. */
export const reasonOptions: { value: FlagReason; label: string }[] = [
  { value: "rationale_unclear", label: "The rationale is unclear" },
  { value: "incorrect_answer_key", label: "The answer seems wrong" },
  { value: "typo", label: "Typo or wording mistake" },
  { value: "outdated_guideline", label: "Out-of-date guideline" },
  { value: "media_problem", label: "Image, audio or video problem" },
  { value: "other", label: "Something else" },
];

export function reasonLabel(reason: FlagReason): string {
  return reasonOptions.find((option) => option.value === reason)?.label ?? reason;
}

/** Status names for the reporter. A rejected report is shown kindly: we looked and changed nothing. */
export const statusLabels: Record<FlagStatus, string> = {
  open: "Received",
  triaged: "Being reviewed",
  resolved: "Resolved",
  rejected: "Reviewed, no change needed",
};

/** Status names for moderators. */
export const queueStatusLabels: Record<FlagStatus, string> = {
  open: "Open",
  triaged: "Triaged",
  resolved: "Resolved",
  rejected: "Rejected",
};

export const queueStatuses: FlagStatus[] = ["open", "triaged", "resolved", "rejected"];

/** Where the editor shows a question at one version (interfaces.md 5.2, /admin/questions/[id]). */
export function editorPath(questionId: string, versionId: string): string {
  return `/admin/questions/${questionId}?version=${versionId}`;
}
