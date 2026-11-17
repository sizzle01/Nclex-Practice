"use client";

import { type KeyboardEvent, useRef } from "react";
import { type SetFilter, filterLabels, filters } from "../setCategories";
import styles from "./practice.module.css";

interface SetFilterTabsProps {
  selected: SetFilter;
  /** How many sets each tab would show, so empty tabs are visible before they are opened. */
  counts: Record<SetFilter, number>;
  panelId: string;
  onSelect: (filter: SetFilter) => void;
}

/** The id of a filter tab, for the tab panel's aria-labelledby. */
export function filterTabId(filter: SetFilter): string {
  return `practice-filter-${filter}`;
}

/**
 * The filter tabs above the catalog: a WAI-ARIA tab list (interfaces.md 4), like the case-study
 * exhibit tabs. Only the selected tab is in the Tab order; Left and Right arrows, Home and End move
 * between tabs and show them straight away.
 */
export function SetFilterTabs({ selected, counts, panelId, onSelect }: SetFilterTabsProps) {
  const tabRefs = useRef<Partial<Record<SetFilter, HTMLButtonElement | null>>>({});

  function select(filter: SetFilter) {
    onSelect(filter);
    tabRefs.current[filter]?.focus();
  }

  function onKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    const index = filters.indexOf(selected);
    const last = filters.length - 1;
    const next =
      event.key === "ArrowRight"
        ? (index + 1) % filters.length
        : event.key === "ArrowLeft"
          ? (index - 1 + filters.length) % filters.length
          : event.key === "Home"
            ? 0
            : event.key === "End"
              ? last
              : null;
    if (next === null) return;
    event.preventDefault();
    select(filters[next]);
  }

  return (
    <div role="tablist" aria-label="Filter practice sets" className={styles.tablist} onKeyDown={onKeyDown}>
      {filters.map((filter) => (
        <button
          key={filter}
          ref={(element) => {
            tabRefs.current[filter] = element;
          }}
          type="button"
          role="tab"
          id={filterTabId(filter)}
          aria-selected={filter === selected}
          aria-controls={panelId}
          tabIndex={filter === selected ? 0 : -1}
          className={styles.tab}
          onClick={() => select(filter)}
        >
          {filterLabels[filter]} <span className={styles.tabCount}>{counts[filter]}</span>
        </button>
      ))}
    </div>
  );
}
