"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import { usePrebuiltSets } from "../api/practiceApi";
import styles from "./practice.module.css";
import { TestSetCard, detailsButtonId } from "./TestSetCard";
import { TestSetDetailPanel } from "./TestSetDetailPanel";

const panelId = "test-set-details";

/**
 * The /practice screen (interfaces.md 5.2): the curated sets of the user's track as cards, and
 * the details of the set named by ?set=<id>, so a set's details can be linked to and survive a
 * reload.
 */
export function PracticeCatalog() {
  const { data: me } = useMe();
  const { data, error, isPending, hasNextPage, fetchNextPage, isFetchingNextPage } = usePrebuiltSets();
  const router = useRouter();
  const pathname = usePathname();
  const selectedId = useSearchParams().get("set");

  const sets = data?.pages.flatMap((page) => page.items) ?? [];

  function openSet(id: string) {
    router.replace(`${pathname}?set=${encodeURIComponent(id)}`, { scroll: false });
  }

  function closeSet() {
    const id = selectedId;
    router.replace(pathname, { scroll: false });
    // Back to the button that opened the panel, so keyboard users keep their place.
    if (id) document.getElementById(detailsButtonId(id))?.focus();
  }

  return (
    <section aria-labelledby="practice-heading" className={styles.page}>
      <div className={styles.header}>
        <h1 id="practice-heading">Practice</h1>
        <p className={ui.muted}>
          Curated sets{me ? ` for the NCLEX-${me.track}` : ""}. Start with the sample set, then try a full practice or
          challenge exam.
        </p>
      </div>

      {isPending && <p className={ui.muted}>Loading sets…</p>}
      {error && (
        <p role="alert" className={styles.error}>
          The sets could not be loaded. Please try again.
        </p>
      )}
      {data && sets.length === 0 && (
        <div className={`${ui.card} ${styles.empty}`}>
          <p className={ui.muted}>There are no practice sets for your track yet. Check back soon.</p>
        </div>
      )}

      {sets.length > 0 && (
        <div className={`${styles.layout} ${selectedId ? styles.layoutWithPanel : ""}`}>
          <div>
            <ul className={styles.list} aria-label="Practice sets">
              {sets.map((set) => (
                <li key={set.id}>
                  <TestSetCard set={set} selected={set.id === selectedId} panelId={panelId} onOpen={openSet} />
                </li>
              ))}
            </ul>
            {hasNextPage && (
              <p>
                <button
                  type="button"
                  className={ui.buttonSecondary}
                  disabled={isFetchingNextPage}
                  onClick={() => fetchNextPage()}
                >
                  {isFetchingNextPage ? "Loading…" : "Show more sets"}
                </button>
              </p>
            )}
          </div>
          {selectedId && <TestSetDetailPanel id={panelId} setId={selectedId} onClose={closeSet} />}
        </div>
      )}
    </section>
  );
}
