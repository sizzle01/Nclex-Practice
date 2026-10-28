"use client";

import { useId, useState } from "react";
import type { ApiSchema } from "@/lib/api/types";
import { belowSplitViewQuery, useMediaQuery } from "@/lib/browser/useMediaQuery";
import { toNumber } from "@/lib/format/toNumber";
import styles from "./exhibits.module.css";
import { exhibitTabs, parseExhibits, parseScenario } from "./exhibitTypes";
import { ExhibitTabs } from "./ExhibitTabs";

interface ExhibitPanelProps {
  /** The item's exhibits (free JSON from the API). */
  exhibits: unknown;
  caseStudy: ApiSchema<"CaseStudyView"> | null;
}

/**
 * The exhibit panel (interfaces.md 5.3): the case study's tabs and the item's own exhibits. From
 * 1024px it sits beside the question (the player lays out the split view); below that it becomes
 * a drawer, closed at first, that the candidate opens with a button. Renders nothing when the item
 * has neither.
 */
export function ExhibitPanel({ exhibits, caseStudy }: ExhibitPanelProps) {
  const drawerId = useId();
  const headingId = useId();
  const narrow = useMediaQuery(belowSplitViewQuery);
  const [open, setOpen] = useState(false);
  const tabs = exhibitTabs(caseStudy ? parseScenario(caseStudy.scenario) : [], parseExhibits(exhibits));
  if (tabs.length === 0) return null;

  const title = caseStudy ? `Case study: ${caseStudy.title}` : "Exhibits";
  const showContent = !narrow || open;

  return (
    <aside className={styles.panel} aria-labelledby={headingId}>
      <div className={styles.panelHeader}>
        <h3 id={headingId} className={styles.panelTitle}>
          {title}
          {caseStudy && <span className={styles.casePosition}> · Question {toNumber(caseStudy.position)}</span>}
        </h3>
        {narrow && (
          <button
            type="button"
            className={styles.drawerToggle}
            aria-expanded={open}
            aria-controls={drawerId}
            onClick={() => setOpen(!open)}
          >
            {open ? "Hide" : "Show"} {caseStudy ? "case details" : "exhibits"}
          </button>
        )}
      </div>
      <div id={drawerId} hidden={!showContent}>
        {showContent && <ExhibitTabs tabs={tabs} label={title} />}
      </div>
    </aside>
  );
}
