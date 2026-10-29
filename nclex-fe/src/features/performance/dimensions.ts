import type { BreakdownDimension } from "./api/performanceApi";

/** The dimensions the performance page can break down by, in the order the picker lists them. */
export const breakdownDimensions: { value: BreakdownDimension; label: string; title: string; column: string }[] = [
  { value: "client_needs_parent", label: "Client Needs category", title: "By Client Needs category", column: "Category" },
  { value: "client_needs", label: "Client Needs subcategory", title: "By Client Needs subcategory", column: "Subcategory" },
  { value: "body_system", label: "Body system", title: "By body system", column: "Body system" },
  { value: "item_type", label: "Question type", title: "By question type", column: "Question type" },
  { value: "cjmm_skill", label: "Clinical judgment step", title: "By clinical judgment step", column: "Step" },
  { value: "difficulty", label: "Difficulty", title: "By difficulty", column: "Difficulty" },
];

export function dimensionInfo(dimension: BreakdownDimension) {
  return breakdownDimensions.find((d) => d.value === dimension) ?? breakdownDimensions[0];
}
