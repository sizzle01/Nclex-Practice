"use client";

import { useMedia } from "../media/MediaContext";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import type { OptionMark } from "./revealMarks";
import type { Option } from "./types";

interface ChoiceOptionProps {
  type: "radio" | "checkbox";
  /** Radio buttons in one group share a name, which gives the native arrow-key model. */
  name?: string;
  option: Option;
  checked: boolean;
  disabled: boolean;
  onChange(): void;
  struck: boolean;
  /** Shown while the item can still be answered; striking a locked item would mean nothing. */
  showStrike: boolean;
  onToggleStrike(optionId: string): void;
  mark: OptionMark | undefined;
}

/**
 * One answer option with its radio button or checkbox, used by multiple choice, SATA, select N and
 * grouping items. The whole row is the label (44px or more, interfaces.md section 4); the strike
 * button sits beside it, because a button can't go inside a label.
 */
export function ChoiceOption({
  type,
  name,
  option,
  checked,
  disabled,
  onChange,
  struck,
  showStrike,
  onToggleStrike,
  mark,
}: ChoiceOptionProps) {
  return (
    <div className={styles.optionRow}>
      <label className={styles.option}>
        <input type={type} name={name} value={option.id} checked={checked} disabled={disabled} onChange={onChange} />
        <OptionText option={option} struck={struck} />
        <OptionMarkText mark={mark} />
      </label>
      {showStrike && <StrikeButton option={option} struck={struck} onToggle={onToggleStrike} />}
    </div>
  );
}

/** An option's text, plus its picture for graphic options (alt text from the media asset). */
export function OptionText({ option, struck = false }: { option: Option; struck?: boolean }) {
  return (
    <span className={struck ? styles.struck : undefined}>
      {option.mediaId && <OptionImage mediaId={option.mediaId} />}
      {option.text}
    </span>
  );
}

// Its own component, so only graphic options ask for media.
function OptionImage({ mediaId }: { mediaId: string }) {
  const { asset } = useMedia(mediaId);
  if (!asset) return null;
  // Media come from the API or a CDN at their own sizes, so next/image's optimiser does not apply.
  // eslint-disable-next-line @next/next/no-img-element
  return <img src={asset.url} alt={asset.altText ?? ""} className={styles.optionImage} />;
}

/**
 * The strike-through tool (interfaces.md 5.4): crosses an option out as a note to self. It is a
 * toggle button, so screen readers hear "pressed" when the option is struck.
 */
export function StrikeButton({
  option,
  struck,
  onToggle,
}: {
  option: Option;
  struck: boolean;
  onToggle(optionId: string): void;
}) {
  return (
    <button
      type="button"
      className={styles.strikeButton}
      aria-pressed={struck}
      aria-label={`Strike out ${option.text}`}
      title={struck ? "Remove the strike-through" : "Strike out this option"}
      onClick={() => onToggle(option.id)}
    >
      <span aria-hidden="true" className={styles.strikeIcon}>
        S
      </span>
      <span aria-hidden="true">Strike</span>
    </button>
  );
}
