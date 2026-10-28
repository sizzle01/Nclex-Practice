import { render } from "@testing-library/react";
import { vi } from "vitest";
import type { ItemReveal } from "@/features/attempt/api/attemptApi";
import { sampleItems } from "../gallery/sampleItems";
import { ItemAnswer } from "./ItemAnswer";
import type { ItemType } from "./types";

/** The gallery's sample of an item type: content from the F04/F05 sample questions, and a graded answer. */
export function sampleOf(itemType: ItemType) {
  const sample = sampleItems.find((item) => item.itemType === itemType);
  if (!sample) throw new Error(`No sample for ${itemType}`);
  return sample;
}

interface RenderOptions {
  content?: unknown;
  response?: unknown;
  disabled?: boolean;
  struckOptions?: string[];
  reveal?: ItemReveal | null;
}

/**
 * Renders one item type through the registry, as the player does, with the stem as its label.
 * The stem text is "Stem"; content defaults to the type's sample.
 */
export function renderItem(itemType: ItemType, options: RenderOptions = {}) {
  const onChange = vi.fn();
  const onToggleStrike = vi.fn();
  const { container } = render(
    <>
      <p id="stem">Stem</p>
      <ItemAnswer
        itemType={itemType}
        content={options.content ?? sampleOf(itemType).content}
        response={options.response ?? null}
        onChange={onChange}
        disabled={options.disabled ?? false}
        struckOptions={options.struckOptions ?? []}
        onToggleStrike={onToggleStrike}
        labelledBy="stem"
        reveal={options.reveal ?? null}
      />
    </>,
  );
  return { onChange, onToggleStrike, container };
}

/** The sample's graded answer and its reveal, as the review page shows them. */
export function renderRevealed(itemType: ItemType) {
  const sample = sampleOf(itemType);
  return renderItem(itemType, { response: sample.revealResponse, reveal: sample.reveal, disabled: true });
}
