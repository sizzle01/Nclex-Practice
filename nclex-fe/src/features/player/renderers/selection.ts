/**
 * Adds or removes one id, keeping the order the content gives (so the same choice always saves the
 * same answer, whatever order the candidate clicked in).
 */
export function toggleIn(selected: readonly string[], id: string, order: readonly string[]): string[] {
  const next = selected.includes(id) ? selected.filter((s) => s !== id) : [...selected, id];
  return order.filter((o) => next.includes(o));
}

/** Drops the groups (or rows) left empty; null when nothing at all is answered, which clears the answer. */
export function compactGroups<T>(groups: Record<string, T | null | undefined>, isEmpty: (value: T) => boolean) {
  const kept = Object.entries(groups).filter(([, value]) => value !== null && value !== undefined && !isEmpty(value));
  return kept.length > 0 ? (Object.fromEntries(kept) as Record<string, T>) : null;
}
