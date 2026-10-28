import type { ItemReveal } from "@/features/attempt/api/attemptApi";
import type { ApiSchema } from "@/lib/api/types";

export type ItemType = ApiSchema<"ItemType">;

/** An option as the item content sends it (interfaces.md section 1). */
export interface Option {
  id: string;
  text: string;
  /** Graphic options show this image with the text. */
  mediaId?: string | null;
}

/** Every item renderer implements this; the registry picks one by itemType (interfaces.md 5.4). */
export interface ItemRendererProps<C = unknown, R = unknown> {
  content: C;
  response: R | null;
  /** Feeds the save queue; renderers never call the API themselves. */
  onChange(next: R | null): void;
  /** True when the item is locked, or the player stops input (paused, out of time). */
  disabled: boolean;
  /** Options the candidate struck through: a note to self that never changes the answer. */
  struckOptions: string[];
  onToggleStrike(optionId: string): void;
  /** The id of the stem element, which labels the answer group. */
  labelledBy: string;
  /** When present, the renderer marks each target right or wrong (after the reveal). */
  reveal: ItemReveal | null;
}
