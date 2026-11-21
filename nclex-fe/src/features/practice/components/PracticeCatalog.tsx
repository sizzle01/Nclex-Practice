"use client";

import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import { type TestSetSummary, usePrebuiltSets } from "../api/practiceApi";
import { type SetFilter, filterSets, filters, groupSets, parseFilter } from "../setCategories";
import styles from "./practice.module.css";
import { SetFilterTabs, filterTabId } from "./SetFilterTabs";
import { SetSectionList } from "./SetSectionList";
import { detailsButtonId } from "./TestSetCard";
import { TestSetDetailPanel } from "./TestSetDetailPanel";

const panelId = "test-set-details";
const tabPanelId = "practice-sets-panel";

/** Builds /practice?filter=…&set=…, leaving out the defaults so the plain URL stays /practice. */
function catalogUrl(pathname: string, filter: SetFilter, setId: string | null): string {
  const params = new URLSearchParams();
  if (filter !== "all") params.set("filter", filter);
  if (setId) params.set("set", setId);
  const query = params.toString();
  return query ? `${pathname}?${query}` : pathname;
}

/**
 * The /practice screen (interfaces.md 5.2): the curated sets of the user's track under filter tabs
 * (All, Practice, Timed, Untimed, Adaptive, Topics) and grouped into sections, plus the details of the set
 * named by ?set=<id>. Both the tab and the set live in the URL, so they can be linked to and
 * survive a reload.
 */
export function PracticeCatalog() {
  const { data: me } = useMe();
  const { data: sets, error, isPending } = usePrebuiltSets();
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const selectedId = searchParams.get("set");
  const filter = parseFilter(searchParams.get("filter"));

  function openSet(id: string) {
    router.replace(catalogUrl(pathname, filter, id), { scroll: false });
  }

  function closeSet() {
    const id = selectedId;
    router.replace(catalogUrl(pathname, filter, null), { scroll: false });
    // Back to the button that opened the panel, so keyboard users keep their place.
    if (id) document.getElementById(detailsButtonId(id))?.focus();
  }

  function selectFilter(next: SetFilter) {
    router.replace(catalogUrl(pathname, next, selectedId), { scroll: false });
  }

  return (
    <section aria-labelledby="practice-heading" className={styles.page}>
      <div className={styles.header}>
        <h1 id="practice-heading">Practice</h1>
        <p className={ui.muted}>
          Curated sets{me ? ` for the NCLEX-${me.track}` : ""}. Start with the sample set, then try a full practice or
          challenge exam.
        </p>
        <p>
          <Link href="/practice/custom" className={ui.buttonSecondary}>
            Build a custom test
          </Link>
        </p>
      </div>

      {isPending && <p className={ui.muted}>Loading sets…</p>}
      {error && (
        <p role="alert" className={styles.error}>
          The sets could not be loaded. Please try again.
        </p>
      )}
      {sets && sets.length === 0 && (
        <div className={`${ui.card} ${styles.empty}`}>
          <p className={ui.muted}>There are no practice sets for your track yet. Check back soon.</p>
        </div>
      )}

      {sets && sets.length > 0 && (
        <div>
          <SetFilterTabs
            selected={filter}
            counts={countPerFilter(sets)}
            panelId={tabPanelId}
            onSelect={selectFilter}
          />
          <div className={`${styles.layout} ${selectedId ? styles.layoutWithPanel : ""}`}>
            <div role="tabpanel" id={tabPanelId} aria-labelledby={filterTabId(filter)} className={styles.tabpanel}>
              <TabContent sets={sets} filter={filter} selectedId={selectedId} onOpen={openSet} />
            </div>
            {selectedId && <TestSetDetailPanel id={panelId} setId={selectedId} onClose={closeSet} />}
          </div>
        </div>
      )}
    </section>
  );
}

function countPerFilter(sets: TestSetSummary[]): Record<SetFilter, number> {
  return Object.fromEntries(filters.map((f) => [f, filterSets(sets, f).length])) as Record<SetFilter, number>;
}

interface TabContentProps {
  sets: TestSetSummary[];
  filter: SetFilter;
  selectedId: string | null;
  onOpen: (id: string) => void;
}

/** The selected tab's sections, or a note when the tab is empty. */
function TabContent({ sets, filter, selectedId, onOpen }: TabContentProps) {
  const sections = groupSets(sets, filter);
  if (sections.length > 0) {
    return <SetSectionList sections={sections} selectedId={selectedId} panelId={panelId} onOpen={onOpen} />;
  }
  // Adaptive exams arrive with F19 (architecture.md 5.8), so that tab says they are on the way.
  const message =
    filter === "adaptive"
      ? "Adaptive exams are coming soon. Like the real NCLEX, they will stop once your result is clear."
      : filter === "topics"
        ? "Topic tests appear here as each topic gets enough questions."
        : "No sets match this filter yet.";
  return (
    <div className={`${ui.card} ${styles.empty}`}>
      <p className={ui.muted}>{message}</p>
    </div>
  );
}
