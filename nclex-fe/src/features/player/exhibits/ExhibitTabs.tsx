"use client";

import { type KeyboardEvent, useId, useRef, useState } from "react";
import styles from "./exhibits.module.css";
import type { ExhibitTab } from "./exhibitTypes";
import { ExhibitView } from "./ExhibitView";

/**
 * The case-study tabs (interfaces.md 5.3): a WAI-ARIA tab list. Only the selected tab is in the Tab
 * order; Left and Right arrows (and Home, End) move between tabs and show them straight away.
 * A single tab needs no tab list, so its exhibits are shown directly.
 */
export function ExhibitTabs({ tabs, label }: { tabs: ExhibitTab[]; label: string }) {
  const baseId = useId();
  const [selectedIndex, setSelectedIndex] = useState(0);
  const tabRefs = useRef<Array<HTMLButtonElement | null>>([]);
  const selected = tabs[Math.min(selectedIndex, tabs.length - 1)];

  if (tabs.length === 1) {
    return <TabExhibits tab={tabs[0]} />;
  }

  function select(index: number) {
    setSelectedIndex(index);
    tabRefs.current[index]?.focus();
  }

  function onKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    const last = tabs.length - 1;
    const next =
      event.key === "ArrowRight"
        ? (selectedIndex + 1) % tabs.length
        : event.key === "ArrowLeft"
          ? (selectedIndex - 1 + tabs.length) % tabs.length
          : event.key === "Home"
            ? 0
            : event.key === "End"
              ? last
              : null;
    if (next === null) return;
    event.preventDefault();
    select(next);
  }

  return (
    <div>
      <div role="tablist" aria-label={label} className={styles.tablist} onKeyDown={onKeyDown}>
        {tabs.map((tab, index) => (
          <button
            key={tab.id}
            ref={(element) => {
              tabRefs.current[index] = element;
            }}
            type="button"
            role="tab"
            id={`${baseId}-tab-${index}`}
            aria-selected={tab === selected}
            aria-controls={`${baseId}-panel`}
            tabIndex={tab === selected ? 0 : -1}
            className={styles.tab}
            onClick={() => select(index)}
          >
            {tab.title}
          </button>
        ))}
      </div>
      <div
        role="tabpanel"
        id={`${baseId}-panel`}
        aria-labelledby={`${baseId}-tab-${tabs.indexOf(selected)}`}
        tabIndex={0}
        className={styles.tabpanel}
      >
        <TabExhibits tab={selected} />
      </div>
    </div>
  );
}

function TabExhibits({ tab }: { tab: ExhibitTab }) {
  return (
    <div className={styles.exhibits}>
      {tab.exhibits.length === 0 ? (
        <p className={styles.unavailable}>Nothing recorded.</p>
      ) : (
        tab.exhibits.map((exhibit) => <ExhibitView key={exhibit.id} exhibit={exhibit} />)
      )}
    </div>
  );
}
