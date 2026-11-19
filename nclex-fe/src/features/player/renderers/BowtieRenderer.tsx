"use client";

import {
  type Announcements,
  DndContext,
  type DragEndEvent,
  PointerSensor,
  useDraggable,
  useDroppable,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import { type KeyboardEvent, useId, useRef, useState } from "react";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import { type OptionMark, optionMarks } from "./revealMarks";
import type { ItemRendererProps, Option } from "./types";

interface BowtieContent {
  /** Each list is shuffled per attempt by the server. */
  condition: Option[];
  actions: Option[];
  parameters: Option[];
}

interface BowtieResponse {
  condition: string | null;
  actions: string[];
  parameters: string[];
}

type Kind = "condition" | "actions" | "parameters";

/** Left wing, centre, right wing: the order the diagram shows them in. */
const kinds: Kind[] = ["actions", "condition", "parameters"];

const kindInfo: Record<Kind, { list: string; slot: string; slots: number }> = {
  actions: { list: "Actions to take", slot: "Action to take", slots: 2 },
  condition: { list: "Potential conditions", slot: "Condition most likely experiencing", slots: 1 },
  parameters: { list: "Parameters to monitor", slot: "Parameter to monitor", slots: 2 },
};

interface Picked {
  kind: Kind;
  id: string;
}

/**
 * Bowtie (interfaces.md 2.x and section 4). The diagram has one condition slot in the centre, two
 * action slots on the left and two parameter slots on the right; the three option lists sit below.
 * Two ways to fill a slot, both giving the same response:
 * - pick an option (click, tap, Enter or Space), then choose a slot of the same kind, which gets
 *   focus straight away; Escape cancels the pick;
 * - drag an option onto a slot with a mouse or finger.
 * A live region announces each pick, placement and removal. The response is null until something
 * is placed, so an untouched bowtie never counts as an answer.
 */
export function BowtieRenderer({
  content,
  response,
  onChange,
  disabled,
  labelledBy,
  reveal,
}: ItemRendererProps<BowtieContent, BowtieResponse>) {
  const instructionsId = useId();
  const dndId = useId();
  const [picked, setPicked] = useState<Picked | null>(null);
  const [announcement, setAnnouncement] = useState("");
  const slotButtons = useRef(new Map<string, HTMLButtonElement>());
  const marks = optionMarks(reveal);
  // A small distance before a drag starts, so a click or tap on an option still picks it.
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }));

  const placed: Record<Kind, string[]> = {
    condition: response?.condition ? [response.condition] : [],
    actions: response?.actions ?? [],
    parameters: response?.parameters ?? [],
  };
  const optionsOf = (kind: Kind) => content[kind];
  const textOf = (kind: Kind, id: string) => optionsOf(kind).find((o) => o.id === id)?.text ?? id;

  function save(next: Record<Kind, string[]>) {
    const empty = kinds.every((kind) => next[kind].length === 0);
    onChange(empty ? null : { condition: next.condition[0] ?? null, actions: next.actions, parameters: next.parameters });
  }

  function place(kind: Kind, id: string, slot: number) {
    const list = placed[kind].filter((placedId) => placedId !== id);
    // A filled slot is replaced; an empty one takes the next free place, so slots fill from the top.
    if (slot < list.length) list[slot] = id;
    else list.push(id);
    save({ ...placed, [kind]: list.slice(0, kindInfo[kind].slots) });
    setPicked(null);
    setAnnouncement(`${textOf(kind, id)} placed in ${slotName(kind, Math.min(slot, list.length - 1))}.`);
  }

  function remove(kind: Kind, slot: number) {
    const id = placed[kind][slot];
    save({ ...placed, [kind]: placed[kind].filter((_, index) => index !== slot) });
    setAnnouncement(`${textOf(kind, id)} removed from the diagram.`);
  }

  function pick(kind: Kind, id: string) {
    if (picked?.id === id) {
      setPicked(null);
      setAnnouncement(`${textOf(kind, id)} is no longer picked.`);
      return;
    }
    setPicked({ kind, id });
    setAnnouncement(`Picked ${textOf(kind, id)}. Choose a slot for it: ${kindInfo[kind].slot}. Press Escape to cancel.`);
    // Move focus to the first empty slot of this kind (or the first slot when all are full).
    const target = Math.min(placed[kind].length, kindInfo[kind].slots - 1);
    slotButtons.current.get(slotKey(kind, target))?.focus();
  }

  function onSlot(kind: Kind, slot: number) {
    if (picked?.kind === kind) {
      place(kind, picked.id, slot);
    } else {
      setAnnouncement(`Pick an option from ${kindInfo[kind].list} first, then choose this slot.`);
    }
  }

  function onKeyDown(event: KeyboardEvent) {
    if (event.key === "Escape" && picked) {
      setAnnouncement(`${textOf(picked.kind, picked.id)} is no longer picked.`);
      setPicked(null);
    }
  }

  function onDragEnd({ active, over }: DragEndEvent) {
    const from = active.data.current as { kind: Kind } | undefined;
    const to = over?.data.current as { kind: Kind; slot: number } | undefined;
    if (from && to && from.kind === to.kind) place(to.kind, String(active.id), to.slot);
  }

  const announcements: Announcements = {
    onDragStart: ({ active }) => {
      const kind = (active.data.current as { kind: Kind }).kind;
      return `Picked up ${textOf(kind, String(active.id))}. Drop it on a slot: ${kindInfo[kind].slot}.`;
    },
    onDragOver: () => undefined,
    onDragEnd: () => undefined,
    onDragCancel: () => "Dragging was cancelled.",
  };

  const counts = kinds.map((kind) => `${kindInfo[kind].list}: ${placed[kind].length} of ${kindInfo[kind].slots}`).join("; ");

  return (
    <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId} onKeyDown={onKeyDown}>
      <p id={instructionsId} className={styles.instructions}>
        Complete the diagram: 1 condition in the centre, 2 actions on the left and 2 parameters on the right. Choose an
        option from a list below, then choose a slot, or drag the option onto a slot. Placed so far: {counts}.
      </p>
      <DndContext id={dndId} sensors={sensors} onDragEnd={onDragEnd} accessibility={{ announcements }}>
        <div className={styles.bowtie}>
          {kinds.map((kind) => (
            <div key={kind} className={`${styles.bowtieWing} ${styles[`bowtie_${kind}`]}`}>
              {Array.from({ length: kindInfo[kind].slots }, (_, slot) => {
                const id = placed[kind][slot];
                return (
                  <Slot
                    key={slot}
                    kind={kind}
                    slot={slot}
                    option={id ? { id, text: textOf(kind, id) } : null}
                    disabled={disabled}
                    awaiting={picked?.kind === kind}
                    mark={id ? marks[id] : undefined}
                    buttonRef={(button) => {
                      if (button) slotButtons.current.set(slotKey(kind, slot), button);
                      else slotButtons.current.delete(slotKey(kind, slot));
                    }}
                    onChoose={() => onSlot(kind, slot)}
                    onRemove={() => remove(kind, slot)}
                  />
                );
              })}
            </div>
          ))}
        </div>
        <div className={styles.bowtieLists}>
          {(["condition", "actions", "parameters"] as Kind[]).map((kind) => (
            <OptionList
              key={kind}
              kind={kind}
              options={optionsOf(kind)}
              placed={placed[kind]}
              picked={picked}
              disabled={disabled}
              marks={marks}
              onPick={(id) => pick(kind, id)}
            />
          ))}
        </div>
      </DndContext>
      <div role="status" className={styles.visuallyHidden}>
        {announcement}
      </div>
    </div>
  );
}

function slotKey(kind: Kind, slot: number) {
  return `${kind}-${slot}`;
}

/** "Action to take, slot 1 of 2", or just the slot's name when its kind has one slot. */
function slotName(kind: Kind, slot: number) {
  const { slot: name, slots } = kindInfo[kind];
  return slots > 1 ? `${name}, slot ${slot + 1} of ${slots}` : name;
}

interface SlotProps {
  kind: Kind;
  slot: number;
  option: Option | null;
  disabled: boolean;
  /** An option of this slot's kind is picked, so choosing the slot will place it. */
  awaiting: boolean;
  mark: OptionMark | undefined;
  buttonRef(button: HTMLButtonElement | null): void;
  onChoose(): void;
  onRemove(): void;
}

function Slot({ kind, slot, option, disabled, awaiting, mark, buttonRef, onChoose, onRemove }: SlotProps) {
  const { setNodeRef, isOver } = useDroppable({ id: slotKey(kind, slot), data: { kind, slot }, disabled });
  const name = slotName(kind, slot);
  const body = (
    <>
      <span className={styles.slotName}>{name}</span>
      <span className={option ? undefined : styles.slotEmpty}>{option?.text ?? "Empty"}</span>
      <OptionMarkText mark={mark} />
    </>
  );

  return (
    <div
      ref={setNodeRef}
      className={`${styles.slot} ${awaiting ? styles.slotAwaiting : ""} ${isOver ? styles.slotOver : ""}`}
    >
      {disabled ? (
        <div className={styles.slotBody}>{body}</div>
      ) : (
        <button
          type="button"
          ref={buttonRef}
          className={styles.slotBody}
          aria-label={`${name}: ${option?.text ?? "empty"}`}
          onClick={onChoose}
        >
          {body}
        </button>
      )}
      {option && !disabled && (
        <button type="button" className={styles.slotRemove} aria-label={`Remove ${option.text} from ${name}`} onClick={onRemove}>
          Remove
        </button>
      )}
    </div>
  );
}

interface OptionListProps {
  kind: Kind;
  options: Option[];
  placed: string[];
  picked: Picked | null;
  disabled: boolean;
  marks: Record<string, OptionMark>;
  onPick(id: string): void;
}

function OptionList({ kind, options, placed, picked, disabled, marks, onPick }: OptionListProps) {
  const headingId = useId();
  return (
    <div role="group" aria-labelledby={headingId} className={styles.bowtieList}>
      <p id={headingId} className={styles.legend}>
        {kindInfo[kind].list}
      </p>
      <ul className={styles.bowtieOptions}>
        {options.map((option) => (
          <BowtieOption
            key={option.id}
            kind={kind}
            option={option}
            placed={placed.includes(option.id)}
            picked={picked?.id === option.id}
            disabled={disabled}
            // A placed option is marked in its slot; only a correct option left out is marked here.
            mark={marks[option.id] === "missed" ? "missed" : undefined}
            onPick={() => onPick(option.id)}
          />
        ))}
      </ul>
    </div>
  );
}

interface BowtieOptionProps {
  kind: Kind;
  option: Option;
  placed: boolean;
  picked: boolean;
  disabled: boolean;
  mark: OptionMark | undefined;
  onPick(): void;
}

function BowtieOption({ kind, option, placed, picked, disabled, mark, onPick }: BowtieOptionProps) {
  const inactive = disabled || placed;
  // Only the pointer listeners: the keyboard places options by picking, not by dnd-kit's keyboard drag.
  const { setNodeRef, listeners, transform, isDragging } = useDraggable({ id: option.id, data: { kind }, disabled: inactive });
  const style = transform ? { transform: `translate3d(${transform.x}px, ${transform.y}px, 0)` } : undefined;

  return (
    <li className={styles.bowtieOptionRow}>
      <button
        type="button"
        ref={setNodeRef}
        style={style}
        className={`${styles.bowtieOption} ${isDragging ? styles.dragging : ""}`}
        aria-pressed={picked}
        disabled={inactive}
        onClick={onPick}
        {...listeners}
      >
        {option.text}
        {/* The space sits outside the note, or the accessible name runs the words together. */}
        {placed && (
          <>
            {" "}
            <span className={styles.placedNote}>(in the diagram)</span>
          </>
        )}
        <OptionMarkText mark={mark} />
      </button>
    </li>
  );
}
