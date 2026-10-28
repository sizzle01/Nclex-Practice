"use client";

import {
  type Announcements,
  closestCenter,
  DndContext,
  type DragEndEvent,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { useEffect, useId, useRef, useState } from "react";
import { OptionMarkText } from "./OptionMarkText";
import styles from "./renderers.module.css";
import { choiceMarks, type OptionMark } from "./revealMarks";
import type { ItemRendererProps, Option } from "./types";

interface OrderedContent {
  /** Shuffled per attempt by the server (seeded by the attempt id). */
  items: Option[];
}

interface OrderedResponse {
  order: string[];
}

type Direction = "up" | "down";

/**
 * Ordered response (interfaces.md 2.9). Three ways to reorder, all giving the same response:
 * Move up / Move down buttons; the drag handle with the keyboard (Space picks up, arrows move,
 * Space drops); or dragging with a mouse or finger. A live region announces each new position
 * (interfaces.md section 4).
 *
 * The response stays null until the candidate changes the order or presses "Keep this order", so
 * an untouched (shuffled) list never counts as an answer.
 */
export function OrderedResponseRenderer({
  content,
  response,
  onChange,
  disabled,
  labelledBy,
  reveal,
}: ItemRendererProps<OrderedContent, OrderedResponse>) {
  const instructionsId = useId();
  const dndId = useId();
  const [announcement, setAnnouncement] = useState("");
  const pendingFocus = useRef<{ id: string; direction: Direction } | null>(null);
  const items = new Map(content.items.map((item) => [item.id, item]));
  const order = response?.order ?? content.items.map((item) => item.id);
  const marks = choiceMarks("ordered_response", reveal);
  const expectedPosition = new Map((reveal?.grade.detail ?? []).map((detail, index) => [detail.target, index + 1]));
  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const textOf = (id: string) => items.get(id)?.text ?? id;
  const positionText = (index: number) => `position ${index + 1} of ${order.length}`;

  function move(from: number, to: number) {
    if (to < 0 || to >= order.length || from === to) return;
    onChange({ order: arrayMove(order, from, to) });
    setAnnouncement(`${textOf(order[from])} moved to ${positionText(to)}.`);
  }

  function moveWithButton(index: number, direction: Direction) {
    pendingFocus.current = { id: order[index], direction };
    move(index, direction === "up" ? index - 1 : index + 1);
  }

  // Keep focus on the moved item's button. At the top or bottom that button is now disabled, so
  // focus goes to the other one rather than being lost.
  const orderKey = order.join(",");
  useEffect(() => {
    const target = pendingFocus.current;
    pendingFocus.current = null;
    if (!target) return;
    const button = (direction: Direction) =>
      [...document.querySelectorAll<HTMLButtonElement>(`[data-move="${direction}"]`)].find(
        (candidate) => candidate.dataset.item === target.id,
      );
    const same = button(target.direction);
    (same && !same.disabled ? same : button(target.direction === "up" ? "down" : "up"))?.focus();
  }, [orderKey]);

  function onDragEnd({ active, over }: DragEndEvent) {
    if (!over) return;
    move(order.indexOf(String(active.id)), order.indexOf(String(over.id)));
  }

  const announcements: Announcements = {
    onDragStart: ({ active }) =>
      `Picked up ${textOf(String(active.id))}. Use the arrow keys to move it, Space to drop it, or Escape to cancel.`,
    onDragOver: ({ active, over }) =>
      over ? `${textOf(String(active.id))} is over ${positionText(order.indexOf(String(over.id)))}.` : undefined,
    onDragEnd: ({ active, over }) =>
      over ? `${textOf(String(active.id))} dropped at ${positionText(order.indexOf(String(over.id)))}.` : undefined,
    onDragCancel: ({ active }) => `Moving ${textOf(String(active.id))} was cancelled.`,
  };

  return (
    <div role="group" aria-labelledby={labelledBy} aria-describedby={instructionsId}>
      <p id={instructionsId} className={styles.instructions}>
        Put the items in order, first at the top. Use Move up and Move down, or drag an item by its handle.
      </p>
      <DndContext
        id={dndId}
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragEnd={onDragEnd}
        accessibility={{
          announcements,
          screenReaderInstructions: {
            draggable:
              "Press Space to pick up the item. Use the arrow keys to move it, Space to drop it, Escape to cancel.",
          },
        }}
      >
        <SortableContext items={order} strategy={verticalListSortingStrategy}>
          <ol className={styles.orderedList}>
            {order.map((id, index) => (
              <SortableRow
                key={id}
                item={items.get(id) ?? { id, text: id }}
                index={index}
                count={order.length}
                disabled={disabled}
                onMove={moveWithButton}
                mark={marks[id]}
                expectedPosition={expectedPosition.get(id)}
              />
            ))}
          </ol>
        </SortableContext>
      </DndContext>
      {response === null && !disabled && (
        <button type="button" className={styles.keepOrder} onClick={() => onChange({ order })}>
          Keep this order
        </button>
      )}
      <div role="status" className={styles.visuallyHidden}>
        {announcement}
      </div>
    </div>
  );
}

interface SortableRowProps {
  item: Option;
  index: number;
  count: number;
  disabled: boolean;
  onMove(index: number, direction: Direction): void;
  mark: OptionMark | undefined;
  expectedPosition: number | undefined;
}

function SortableRow({ item, index, count, disabled, onMove, mark, expectedPosition }: SortableRowProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: item.id,
    disabled,
  });
  const style = { transform: CSS.Transform.toString(transform), transition };

  return (
    <li ref={setNodeRef} style={style} className={`${styles.orderedItem} ${isDragging ? styles.dragging : ""}`}>
      <span className={styles.orderedNumber} aria-hidden="true">
        {index + 1}
      </span>
      <span className={styles.orderedText}>
        {item.text}
        <OptionMarkText
          mark={mark}
          label={
            mark === "correct"
              ? "✓ Correct position"
              : mark && expectedPosition
                ? `✗ Wrong position; belongs at position ${expectedPosition}`
                : undefined
          }
        />
      </span>
      {!disabled && (
        <span className={styles.orderedControls}>
          <button
            type="button"
            className={styles.handle}
            aria-label={`Drag ${item.text}, now at position ${index + 1} of ${count}`}
            {...attributes}
            {...listeners}
          >
            <span aria-hidden="true">⠿</span>
          </button>
          <button
            type="button"
            className={styles.moveButton}
            data-move="up"
            data-item={item.id}
            aria-label={`Move up: ${item.text}`}
            disabled={index === 0}
            onClick={() => onMove(index, "up")}
          >
            <span aria-hidden="true">▲</span> Move up
          </button>
          <button
            type="button"
            className={styles.moveButton}
            data-move="down"
            data-item={item.id}
            aria-label={`Move down: ${item.text}`}
            disabled={index === count - 1}
            onClick={() => onMove(index, "down")}
          >
            <span aria-hidden="true">▼</span> Move down
          </button>
        </span>
      )}
    </li>
  );
}
