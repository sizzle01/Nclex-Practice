import type { ItemType, Option } from "./types";

/**
 * A readable name for every choice id an item's rationale.perOption can use (interfaces.md 2), so
 * the rationale says "Warfarin: Unusual bleeding" rather than "r1:c1". Ids it can't name are left
 * out, and the panel shows the id itself.
 */
export function choiceLabels(itemType: ItemType, content: unknown): Map<string, string> {
  const c = (content ?? {}) as Record<string, unknown>;
  switch (itemType) {
    case "matrix_mc":
    case "matrix_mr":
      return matrixLabels(c);
    case "multiple_response_grouping":
      return new Map(
        list<{ label: string; options: Option[] }>(c.groups).flatMap((group) =>
          list<Option>(group.options).map((o) => [o.id, `${group.label}: ${o.text}`] as const),
        ),
      );
    case "dropdown_cloze":
    case "dropdown_table":
      return new Map(
        list<{ id: string; options: Option[] }>(c.blanks).flatMap((blank, index) =>
          list<Option>(blank.options).map((o) => [`${blank.id}:${o.id}`, `Blank ${index + 1}: ${o.text}`] as const),
        ),
      );
    case "highlight_text":
      return new Map(list<{ id: string; text: string }>(c.segments).map((s) => [s.id, s.text]));
    case "highlight_table":
      return new Map(list<{ id: string; cells: string[] }>(c.rows).map((r) => [r.id, r.cells.join(", ")]));
    case "ordered_response":
      return optionLabels(c.items);
    default:
      return optionLabels(c.options);
  }
}

function matrixLabels(c: Record<string, unknown>) {
  const columns = list<{ id: string; text: string }>(c.columns);
  return new Map(
    list<{ id: string; text: string }>(c.rows).flatMap((row) =>
      columns.map((column) => [`${row.id}:${column.id}`, `${row.text}: ${column.text}`] as const),
    ),
  );
}

function optionLabels(options: unknown) {
  return new Map(list<Option>(options).map((o) => [o.id, o.text]));
}

function list<T>(value: unknown): T[] {
  return Array.isArray(value) ? (value as T[]) : [];
}
