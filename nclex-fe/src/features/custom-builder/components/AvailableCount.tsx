import styles from "./builder.module.css";

interface AvailableCountProps {
  available: number | undefined;
  requested: number;
  isFetching: boolean;
}

/**
 * The live pool size. It is a polite live region, so screen-reader users hear the new number
 * once the filters settle, without losing their place.
 */
export function AvailableCount({ available, requested, isFetching }: AvailableCountProps) {
  const tooFew = available !== undefined && Number.isFinite(requested) && available < requested;
  return (
    <p role="status" aria-live="polite" className={`${styles.count} ${tooFew ? styles.countShort : ""}`}>
      {available === undefined
        ? "Counting matching questions…"
        : `${available} ${available === 1 ? "question matches" : "questions match"} your filters.`}
      {tooFew && " Choose fewer questions or widen the filters."}
      {isFetching && available !== undefined && <span className={styles.updating}> Updating…</span>}
    </p>
  );
}
