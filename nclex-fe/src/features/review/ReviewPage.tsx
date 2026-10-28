"use client";

import { useInfiniteQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useId, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import { type CandidateItem, fetchReview, type ReviewFilter, reviewQueryKey } from "../attempt/api/attemptApi";
import { ItemAnswer } from "../attempt/renderers/ItemAnswer";
import { RationalePanel } from "../attempt/reveal/RationalePanel";
import styles from "./review.module.css";

const filters: { value: ReviewFilter; label: string; empty: string }[] = [
  { value: "all", label: "All", empty: "There are no answers to review yet." },
  { value: "incorrect", label: "Incorrect", empty: "No incorrect answers. Well done!" },
  { value: "flagged", label: "Marked for review", empty: "You didn't mark any questions for review." },
];

/**
 * /attempts/[id]/review (interfaces.md 5.2): each question with the candidate's answer marked
 * right or wrong, the partial-credit points, and the rationale for every option. Filters show
 * all, incorrect (including partial credit and blanks) or marked-for-review questions.
 */
export function ReviewPage({ attemptId, initialFilter }: { attemptId: string; initialFilter: ReviewFilter }) {
  const [filter, setFilter] = useState<ReviewFilter>(initialFilter);
  const review = useInfiniteQuery({
    queryKey: reviewQueryKey(attemptId, filter),
    queryFn: ({ pageParam }) => fetchReview(attemptId, filter, pageParam),
    initialPageParam: null as string | null,
    getNextPageParam: (page) => page.nextCursor,
  });
  const items = review.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1 className={styles.title}>Review your answers</h1>
        <Link href={`/attempts/${attemptId}/results`} className={ui.buttonGhost}>
          Back to results
        </Link>
      </header>

      <div role="group" aria-label="Show" className={styles.filters}>
        {filters.map((option) => (
          <button
            key={option.value}
            type="button"
            className={filter === option.value ? ui.button : ui.buttonSecondary}
            aria-pressed={filter === option.value}
            onClick={() => setFilter(option.value)}
          >
            {option.label}
          </button>
        ))}
      </div>

      {review.isPending ? (
        <p className={ui.muted}>Loading your answers…</p>
      ) : review.isError ? (
        <p role="alert" className={ui.card}>
          {isApiError(review.error, "ATTEMPT_NOT_ACTIVE")
            ? "Answers are shown after you submit this test."
            : "Your answers could not be loaded. Please try again."}
        </p>
      ) : items.length === 0 ? (
        <p className={ui.card}>{filters.find((f) => f.value === filter)!.empty}</p>
      ) : (
        <>
          <ol className={styles.list}>
            {items.map((item) => (
              <li key={toNumber(item.position)}>
                <ReviewItem item={item} />
              </li>
            ))}
          </ol>
          {review.hasNextPage && (
            <button
              type="button"
              className={ui.buttonSecondary}
              disabled={review.isFetchingNextPage}
              onClick={() => void review.fetchNextPage()}
            >
              {review.isFetchingNextPage ? "Loading…" : "Show more"}
            </button>
          )}
        </>
      )}
    </div>
  );
}

function ReviewItem({ item }: { item: CandidateItem }) {
  const stemId = useId();
  return (
    <article className={`${ui.card} ${styles.item}`} aria-labelledby={`${stemId}-title`}>
      <h2 id={`${stemId}-title`} className={styles.itemTitle}>
        Question {toNumber(item.position)}
        {item.flagged && (
          <>
            {" "}
            <span className={ui.badge}>Marked for review</span>
          </>
        )}
      </h2>
      <p id={stemId} className={styles.stem}>
        {item.stem}
      </p>
      <ItemAnswer
        itemType={item.itemType}
        content={item.content}
        response={item.response ?? null}
        onChange={() => undefined}
        disabled
        struckOptions={item.struckOptions}
        labelledBy={stemId}
        reveal={item.reveal}
      />
      {item.response == null && <p className={ui.muted}>Not answered.</p>}
      {item.reveal && <RationalePanel reveal={item.reveal} content={item.content} />}
    </article>
  );
}
