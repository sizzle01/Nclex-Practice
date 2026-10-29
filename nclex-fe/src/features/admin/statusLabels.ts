import type { VersionStatus } from "./api/adminApi";

/** How each review-workflow status reads in the console. */
export const statusLabels: Record<NonNullable<VersionStatus>, string> = {
  draft: "Draft",
  in_review: "In review",
  approved: "Approved",
  published: "Published",
  superseded: "Superseded",
  rejected: "Rejected",
};

export function statusLabel(status: VersionStatus): string {
  return status ? statusLabels[status] : "";
}
