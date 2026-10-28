import type { ApiSchema } from "@/lib/api/types";
import { MultipleChoiceRenderer } from "./MultipleChoiceRenderer";
import { SataRenderer } from "./SataRenderer";
import type { ItemRendererProps } from "./types";
import { UnsupportedRenderer } from "./UnsupportedRenderer";

type ItemType = ApiSchema<"ItemType">;

type ItemAnswerProps = ItemRendererProps & { itemType: ItemType };

/**
 * Picks the renderer for an item type (interfaces.md 5.4). F08 ships multiple choice and SATA;
 * the other MVP types arrive in F11 and show a notice until then. The payload shapes are
 * checked by the server, so the casts here only narrow `unknown` to the type's own shape.
 */
export function ItemAnswer({ itemType, ...props }: ItemAnswerProps) {
  switch (itemType) {
    case "multiple_choice":
      return <MultipleChoiceRenderer {...(props as ItemRendererProps<never, never>)} />;
    case "sata":
      return <SataRenderer {...(props as ItemRendererProps<never, never>)} />;
    default:
      return <UnsupportedRenderer />;
  }
}
