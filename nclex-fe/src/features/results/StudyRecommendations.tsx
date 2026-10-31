"use client";

import { useQuery } from "@tanstack/react-query";
import ui from "@/components/ui/ui.module.css";
import { toNumber } from "@/lib/format/toNumber";
import { fetchRecommendations, recommendationsQueryKey } from "../attempt/api/attemptApi";
import styles from "./results.module.css";

/**
 * "What to study next": the specific subtopics the candidate missed in this test, most missed first
 * (docs/features/F17). It loads on its own, so a failure here leaves the rest of the results intact.
 */
export function StudyRecommendations({ attemptId }: { attemptId: string }) {
  const { data, isPending, isError } = useQuery({
    queryKey: recommendationsQueryKey(attemptId),
    queryFn: () => fetchRecommendations(attemptId),
  });

  return (
    <section aria-labelledby="study-next-heading" className={ui.card}>
      <h2 id="study-next-heading" className={ui.cardTitle}>
        What to study next
      </h2>
      {isPending ? (
        <p className={ui.muted}>Finding topics to review…</p>
      ) : isError ? (
        <p className={ui.muted}>Your study suggestions could not be loaded. Your results are not affected.</p>
      ) : data.subtopics.length === 0 ? (
        <p>No specific topics to review from this test.</p>
      ) : (
        <>
          <ol className={styles.weakest}>
            {data.subtopics.map((subtopic) => (
              <li key={subtopic.id}>
                <strong>{subtopic.name}</strong>: missed {toNumber(subtopic.missed)} of {toNumber(subtopic.answered)}
              </li>
            ))}
          </ol>
          <p className={ui.muted}>Based on this test&apos;s question topics.</p>
        </>
      )}
    </section>
  );
}
