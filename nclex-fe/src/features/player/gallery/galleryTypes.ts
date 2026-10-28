import type { ItemReveal } from "@/features/attempt/api/attemptApi";
import type { ApiSchema } from "@/lib/api/types";
import type { ItemType } from "../renderers/types";

/** One sample in the dev gallery: an item as the player receives it, plus a graded answer to reveal. */
export interface GalleryItem {
  itemType: ItemType;
  stem: string;
  content: unknown;
  exhibits?: unknown;
  caseStudy?: ApiSchema<"CaseStudyView"> | null;
  /** The answer the reveal was graded on; shown when "Show the reveal" is ticked. */
  revealResponse: unknown;
  reveal: ItemReveal;
}
