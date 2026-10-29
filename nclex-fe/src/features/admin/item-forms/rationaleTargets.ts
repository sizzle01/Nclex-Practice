import { choiceLabels } from "@/features/player/renderers/choiceLabels";
import type { ItemType } from "@/features/player/renderers/types";

/** One id that needs a "why" in rationale.perOption, with the text authors recognise it by. */
export interface RationaleTarget {
  id: string;
  label: string;
}

/**
 * Every id rationale.perOption must cover (PRD 5.1, interfaces.md section 2): options; matrix
 * cells "row:column"; dropdown choices "blank:option"; selectable highlight segments or rows.
 * A calculation has none. Mirrors the server's checks, so the form can show the boxes to fill.
 */
export function rationaleTargets(itemType: ItemType, content: unknown): RationaleTarget[] {
  const labels = choiceLabels(itemType, content);
  const all = [...labels.entries()].map(([id, label]) => ({ id, label: label || id }));
  const c = (content ?? {}) as { segments?: Array<{ id: string; selectable: boolean }>; rows?: Array<{ id: string; selectable?: boolean }> };

  switch (itemType) {
    case "highlight_text":
      return all.filter((t) => c.segments?.find((s) => s.id === t.id)?.selectable);
    case "highlight_table":
      return all.filter((t) => c.rows?.find((r) => r.id === t.id)?.selectable);
    case "fill_blank_calc":
      return [];
    default:
      return all;
  }
}

/** perOption with an entry for each target, keeping what was written and dropping stale ids. */
export function alignPerOption(perOption: Record<string, string>, targets: RationaleTarget[]): Record<string, string> {
  return Object.fromEntries(targets.map((t) => [t.id, perOption[t.id] ?? ""]));
}
