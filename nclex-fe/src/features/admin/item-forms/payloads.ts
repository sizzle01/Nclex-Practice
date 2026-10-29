import type { ItemType, Option } from "@/features/player/renderers/types";

// The authoring payloads of interfaces.md section 2: what each item type's content and answer key
// look like. The server checks them on every save; these types keep the editors honest.

export interface Header {
  id: string;
  text: string;
}

export interface Blank {
  id: string;
  options: Option[];
}

export type DropdownCell = { text: string } | { blankId: string };

export interface ItemContentByType {
  multiple_choice: { options: Option[] };
  sata: { options: Option[] };
  select_n: { options: Option[]; selectCount: number };
  ordered_response: { items: Option[] };
  matrix_mc: { rows: Header[]; columns: Header[] };
  matrix_mr: { rows: Header[]; columns: Header[]; groupAxis: "row" | "column" };
  multiple_response_grouping: { groups: Array<{ id: string; label: string; options: Option[] }> };
  dropdown_cloze: { template: string; blanks: Blank[] };
  dropdown_table: { columns: string[]; rows: Array<{ id: string; cells: DropdownCell[] }>; blanks: Blank[] };
  highlight_text: { segments: Array<{ id: string; text: string; selectable: boolean }> };
  highlight_table: { columns: string[]; rows: Array<{ id: string; cells: string[]; selectable: boolean }> };
  fill_blank_calc: { unit: string; decimals: number; inputLabel?: string | null };
}

export interface ItemKeyByType {
  multiple_choice: { correct: string };
  sata: { correct: string[] };
  select_n: { correct: string[]; scoringOverride?: "all_or_nothing" | null };
  ordered_response: { order: string[]; scoring: "all_or_nothing" | "per_position" };
  matrix_mc: { byRow: Record<string, string> };
  matrix_mr: { byGroup: Record<string, string[]> };
  multiple_response_grouping: { byGroup: Record<string, string[]> };
  dropdown_cloze: { byBlank: Record<string, string> };
  dropdown_table: { byBlank: Record<string, string> };
  highlight_text: { correct: string[] };
  highlight_table: { correct: string[] };
  fill_blank_calc: { value: number; tolerance?: number | null; rounding: "exact" | "round" };
}

/** The item types the console can author: the MVP ones (interfaces.md 2.1 to 2.9). */
export type AuthorableType = keyof ItemContentByType;

export const authorableTypes: Array<{ type: AuthorableType; label: string }> = [
  { type: "multiple_choice", label: "Multiple Choice" },
  { type: "sata", label: "Select All That Apply" },
  { type: "select_n", label: "Select N" },
  { type: "matrix_mc", label: "Matrix Multiple Choice" },
  { type: "matrix_mr", label: "Matrix Multiple Response" },
  { type: "multiple_response_grouping", label: "Multiple Response Grouping" },
  { type: "dropdown_cloze", label: "Drop-Down Cloze" },
  { type: "dropdown_table", label: "Drop-Down Table" },
  { type: "highlight_text", label: "Highlight Text" },
  { type: "highlight_table", label: "Highlight Table" },
  { type: "fill_blank_calc", label: "Fill in the Blank (Calculation)" },
  { type: "ordered_response", label: "Ordered Response" },
];

export function isAuthorable(type: ItemType): type is AuthorableType {
  return authorableTypes.some((t) => t.type === type);
}

export function typeLabel(type: ItemType): string {
  return authorableTypes.find((t) => t.type === type)?.label ?? type;
}

/** One item type's content and key, edited together because the key refers to the content's ids. */
export interface ItemParts<T extends AuthorableType = AuthorableType> {
  content: ItemContentByType[T];
  answerKey: ItemKeyByType[T];
}

/** The props every item-type form takes. */
export interface ItemFormProps<T extends AuthorableType> {
  value: ItemParts<T>;
  onChange(next: ItemParts<T>): void;
  disabled: boolean;
}

/** The next free id with a prefix: "a", "b"... for options, or "r1", "r2"... for rows. */
export function nextId(existing: string[], prefix = ""): string {
  if (prefix === "") {
    for (let code = 97; code < 123; code++) {
      const id = String.fromCharCode(code);
      if (!existing.includes(id)) return id;
    }
    prefix = "o";
  }
  let n = 1;
  while (existing.includes(`${prefix}${n}`)) n++;
  return `${prefix}${n}`;
}
