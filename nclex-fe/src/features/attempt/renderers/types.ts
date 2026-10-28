import type { ItemReveal } from "../api/attemptApi";

/** An option as the item content sends it (interfaces.md section 1). */
export interface Option {
  id: string;
  text: string;
  mediaId?: string | null;
}

/** Every item renderer implements this; the registry picks one by itemType (interfaces.md 5.4). */
export interface ItemRendererProps<C = unknown, R = unknown> {
  content: C;
  response: R | null;
  /** Feeds the save queue; renderers never call the API themselves. */
  onChange(next: R | null): void;
  /** True when the item is locked. */
  disabled: boolean;
  struckOptions: string[];
  /** The id of the stem element, which labels the answer group. */
  labelledBy: string;
  /** When present, the renderer marks each target right or wrong (after the reveal). */
  reveal?: ItemReveal | null;
}
