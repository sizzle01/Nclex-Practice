import ui from "@/components/ui/ui.module.css";
import type { SetSection } from "../setCategories";
import styles from "./practice.module.css";
import { TestSetCard } from "./TestSetCard";

interface SetSectionListProps {
  sections: SetSection[];
  selectedId: string | null;
  panelId: string;
  onOpen: (id: string) => void;
}

/** The catalog's sections ("Start here", "Practice sets", …), each a heading and a grid of cards. */
export function SetSectionList({ sections, selectedId, panelId, onOpen }: SetSectionListProps) {
  return (
    <div className={styles.sections}>
      {sections.map((section) => {
        const headingId = `practice-section-${section.id}`;
        return (
          <section key={section.id} aria-labelledby={headingId} className={styles.section}>
            <div className={styles.sectionHeader}>
              <h2 id={headingId}>
                {section.title} <span className={styles.sectionCount}>({section.sets.length})</span>
              </h2>
              <p className={ui.muted}>{section.description}</p>
            </div>
            <ul className={styles.list} aria-label={section.title}>
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
