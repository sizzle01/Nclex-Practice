import ui from "@/components/ui/ui.module.css";
import { sectionKey } from "../sectionCollapse";
import type { SetSection } from "../setCategories";
import { useSectionCollapse } from "../useSectionCollapse";
import styles from "./practice.module.css";
import { TestSetCard } from "./TestSetCard";

interface SetSectionListProps {
  sections: SetSection[];
  selectedId: string | null;
  panelId: string;
  /** Whether sections the candidate has not toggled start open. The long Topics tab starts closed. */
  openByDefault?: boolean;
  onOpen: (id: string) => void;
}

/**
 * The catalog's sections ("Start here", "Practice sets", …), each a heading and a grid of cards.
 * Each heading is a disclosure button (F28) so a long tab can be folded down to its section names.
 */
export function SetSectionList({ sections, selectedId, panelId, openByDefault = true, onOpen }: SetSectionListProps) {
  const { isOpen, toggle, setAll } = useSectionCollapse(openByDefault);
  const keys = sections.map(sectionKey);
  const allOpen = keys.every(isOpen);

  return (
    <div className={styles.sections}>
      {sections.length > 1 && (
        <div className={styles.sectionsToolbar}>
          <button type="button" className={ui.buttonGhost} onClick={() => setAll(keys, !allOpen)}>
            {allOpen ? "Collapse all" : "Expand all"}
          </button>
        </div>
      )}
      {sections.map((section) => {
        const key = sectionKey(section);
        const open = isOpen(key);
        const headingId = `practice-section-${section.id}`;
        const contentId = `practice-section-content-${section.id}`;
        return (
          <section key={section.id} aria-labelledby={headingId} className={styles.section}>
            <div className={styles.sectionHeader}>
              {/* The button sits inside the h2 so the heading stays in the page outline (WAI-ARIA disclosure). */}
              <h2 id={headingId}>
                <button
                  type="button"
                  className={styles.sectionToggle}
                  aria-expanded={open}
                  aria-controls={contentId}
                  onClick={() => toggle(key)}
                >
                  <span className={styles.chevron} aria-hidden="true" />
                  <span>
                    {section.title} <span className={styles.sectionCount}>({section.sets.length})</span>
                  </span>
                </button>
              </h2>
              <p className={ui.muted}>{section.description}</p>
            </div>
            <ul id={contentId} className={styles.list} aria-label={section.title} hidden={!open}>
              {section.sets.map((set) => (
                <li key={set.id}>
                  <TestSetCard set={set} selected={set.id === selectedId} panelId={panelId} onOpen={onOpen} />
                </li>
              ))}
            </ul>
          </section>
        );
      })}
    </div>
  );
}
