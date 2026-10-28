import type { ComponentType } from "react";
import { CalcRenderer } from "./CalcRenderer";
import { DropdownClozeRenderer } from "./DropdownClozeRenderer";
import { DropdownTableRenderer } from "./DropdownTableRenderer";
import { GroupingRenderer } from "./GroupingRenderer";
import { HighlightTableRenderer } from "./HighlightTableRenderer";
import { HighlightTextRenderer } from "./HighlightTextRenderer";
import { MatrixMcRenderer, MatrixMrRenderer } from "./MatrixRenderer";
import { MultipleChoiceRenderer } from "./MultipleChoiceRenderer";
import { OrderedResponseRenderer } from "./OrderedResponseRenderer";
import { SataRenderer } from "./SataRenderer";
import { SelectNRenderer } from "./SelectNRenderer";
import type { ItemRendererProps, ItemType } from "./types";
import { UnsupportedRenderer } from "./UnsupportedRenderer";

type AnyRenderer = ComponentType<ItemRendererProps<never, never>>;

/**
 * The renderer for each item type (interfaces.md 5.4). Phase 2 types show a notice until their
 * own feature. The payload shapes are checked by the server, so each renderer can trust its
 * content and response to have its type's shape.
 */
export const renderers: Record<ItemType, AnyRenderer> = {
  multiple_choice: MultipleChoiceRenderer,
  sata: SataRenderer,
  select_n: SelectNRenderer,
  matrix_mc: MatrixMcRenderer,
  matrix_mr: MatrixMrRenderer,
  multiple_response_grouping: GroupingRenderer,
  dropdown_cloze: DropdownClozeRenderer,
  dropdown_table: DropdownTableRenderer,
  highlight_text: HighlightTextRenderer,
  highlight_table: HighlightTableRenderer,
  fill_blank_calc: CalcRenderer,
  ordered_response: OrderedResponseRenderer,
  dropdown_rationale: UnsupportedRenderer,
  drag_drop_cloze: UnsupportedRenderer,
  drag_drop_rationale: UnsupportedRenderer,
  bowtie: UnsupportedRenderer,
  hotspot: UnsupportedRenderer,
};

type ItemAnswerProps = ItemRendererProps & { itemType: ItemType };

/** Picks the renderer for an item type; the casts only narrow `unknown` to that type's own shape. */
export function ItemAnswer({ itemType, ...props }: ItemAnswerProps) {
  const Renderer = renderers[itemType] ?? UnsupportedRenderer;
  return <Renderer {...(props as ItemRendererProps<never, never>)} />;
}
