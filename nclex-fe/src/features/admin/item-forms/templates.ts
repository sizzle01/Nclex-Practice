import type { AuthorableType, ItemParts } from "./payloads";

const options = (ids: string[]) => ids.map((id) => ({ id, text: "" }));

/**
 * A blank starting point for each item type, already the right shape and the smallest size the
 * server accepts, so an author fills in text instead of building structure.
 */
export function emptyItem(type: AuthorableType): ItemParts {
  switch (type) {
    case "multiple_choice":
      return { content: { options: options(["a", "b", "c", "d"]) }, answerKey: { correct: "a" } };
    case "sata":
      return { content: { options: options(["a", "b", "c", "d", "e"]) }, answerKey: { correct: ["a"] } };
    case "select_n":
      return { content: { options: options(["a", "b", "c", "d", "e"]), selectCount: 2 }, answerKey: { correct: ["a", "b"] } };
    case "ordered_response":
      return { content: { items: options(["a", "b", "c", "d"]) }, answerKey: { order: ["a", "b", "c", "d"], scoring: "all_or_nothing" } };
    case "matrix_mc":
      return {
        content: { rows: headers("r", 3), columns: headers("c", 2) },
        answerKey: { byRow: { r1: "c1", r2: "c1", r3: "c1" } },
      };
    case "matrix_mr":
      return {
        content: { rows: headers("r", 3), columns: headers("c", 3), groupAxis: "row" },
        answerKey: { byGroup: { r1: ["r1:c1"], r2: ["r2:c1"], r3: ["r3:c1"] } },
      };
    case "multiple_response_grouping":
      return {
        content: {
          groups: [
            { id: "g1", label: "", options: options(["g1a", "g1b", "g1c"]) },
            { id: "g2", label: "", options: options(["g2a", "g2b", "g2c"]) },
          ],
        },
        answerKey: { byGroup: { g1: ["g1a"], g2: ["g2a"] } },
      };
    case "dropdown_cloze":
      return {
        content: {
          template: "The nurse should first {{b1}} because the client is at risk for {{b2}}.",
          blanks: [
            { id: "b1", options: options(["a", "b", "c"]) },
            { id: "b2", options: options(["a", "b", "c"]) },
          ],
        },
        answerKey: { byBlank: { b1: "a", b2: "a" } },
      };
    case "dropdown_table":
      return {
        content: {
          columns: ["Finding", "Interpretation"],
          rows: [
            { id: "r1", cells: [{ text: "" }, { blankId: "b1" }] },
            { id: "r2", cells: [{ text: "" }, { blankId: "b2" }] },
          ],
          blanks: [
            { id: "b1", options: options(["a", "b", "c"]) },
            { id: "b2", options: options(["a", "b", "c"]) },
          ],
        },
        answerKey: { byBlank: { b1: "a", b2: "a" } },
      };
    case "highlight_text":
      return {
        content: {
          segments: [
            { id: "s1", text: "", selectable: true },
            { id: "s2", text: "", selectable: true },
            { id: "s3", text: "", selectable: true },
          ],
        },
        answerKey: { correct: ["s1"] },
      };
    case "highlight_table":
      return {
        content: {
          columns: ["Assessment", "Finding"],
          rows: [
            { id: "r1", cells: ["", ""], selectable: true },
            { id: "r2", cells: ["", ""], selectable: true },
            { id: "r3", cells: ["", ""], selectable: true },
          ],
        },
        answerKey: { correct: ["r1"] },
      };
    case "fill_blank_calc":
      return { content: { unit: "mL/hr", decimals: 0, inputLabel: "" }, answerKey: { value: 0, rounding: "exact" } };
  }
}

function headers(prefix: string, count: number) {
  return Array.from({ length: count }, (_, i) => ({ id: `${prefix}${i + 1}`, text: "" }));
}
