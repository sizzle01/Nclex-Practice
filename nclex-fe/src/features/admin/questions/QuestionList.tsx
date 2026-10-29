"use client";

import { useInfiniteQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import { useDebouncedValue } from "@/features/custom-builder/useDebouncedValue";
import { toNumber } from "@/lib/format/toNumber";
import { adminKeys, type QuestionSearch, searchQuestions, type VersionStatus } from "../api/adminApi";
import styles from "../components/admin.module.css";
import { authorableTypes, typeLabel } from "../item-forms/payloads";
import { canAuthor } from "../roles";
import { statusLabel, statusLabels } from "../statusLabels";

interface QuestionListProps {
  title: string;
  /** Fixes the status filter, e.g. in_review for the reviewer queue (oldest first). */
  fixedStatus?: VersionStatus;
  intro?: string;
}

/** /admin/questions and /admin/review: search with filters, one row per question (its latest version). */
export function QuestionList({ title, fixedStatus, intro }: QuestionListProps) {
  const { data: me } = useMe();
  const [track, setTrack] = useState("");
  const [status, setStatus] = useState("");
  const [itemType, setItemType] = useState("");
  const [text, setText] = useState("");
  const q = useDebouncedValue(text.trim(), 300);

  const search: QuestionSearch = {
    track: (track || undefined) as QuestionSearch["track"],
    status: fixedStatus ?? ((status || undefined) as QuestionSearch["status"]),
    itemType: (itemType || undefined) as QuestionSearch["itemType"],
    q: q || undefined,
  };
  const query = useInfiniteQuery({
    queryKey: adminKeys.questions(search),
    queryFn: ({ pageParam }) => searchQuestions({ ...search, cursor: pageParam }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (page) => page.nextCursor ?? undefined,
  });
  const rows = query.data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <section aria-labelledby="questions-heading">
      <div className={styles.toolbar}>
        <div>
          <h1 id="questions-heading">{title}</h1>
          {intro && <p className={ui.muted}>{intro}</p>}
        </div>
        {canAuthor(me?.role) && (
          <Link href="/admin/questions/new" className={ui.button}>
            New question
          </Link>
        )}
      </div>

      <div className={styles.filters} role="search" aria-label="Filter questions">
        <label>
          Search stem or id
          <input type="search" value={text} onChange={(event) => setText(event.target.value)} />
        </label>
        <label>
          Track
          <select value={track} onChange={(event) => setTrack(event.target.value)}>
            <option value="">Both</option>
            <option value="RN">RN</option>
            <option value="PN">PN</option>
          </select>
        </label>
        {!fixedStatus && (
          <label>
            Status
            <select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option value="">Any</option>
              {Object.entries(statusLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>
        )}
        <label>
          Item type
          <select value={itemType} onChange={(event) => setItemType(event.target.value)}>
            <option value="">Any</option>
            {authorableTypes.map((t) => (
              <option key={t.type} value={t.type}>
                {t.label}
              </option>
            ))}
          </select>
        </label>
      </div>

      {query.isPending ? (
        <p role="status">Loading questions…</p>
      ) : query.isError ? (
        <p role="alert">The questions could not be loaded.</p>
      ) : rows.length === 0 ? (
        <p role="status">No questions match.</p>
      ) : (
        <div className={styles.tableWrap}>
          <table className={styles.table} aria-labelledby="questions-heading">
            <thead>
              <tr>
                <th scope="col">Stem</th>
                <th scope="col">Type</th>
                <th scope="col">Track</th>
                <th scope="col">Latest version</th>
                <th scope="col">Published</th>
                <th scope="col">Author</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.id}>
                  <td>
                    <Link href={`/admin/questions/${row.id}`}>{row.stem || "(no stem yet)"}</Link>
                    {row.status === "retired" && <span className={styles.status}> Retired</span>}
                  </td>
                  <td>{typeLabel(row.itemType)}</td>
                  <td>{row.track}</td>
                  <td>
                    v{toNumber(row.latestVersion)} <span className={styles.status}>{statusLabel(row.latestStatus)}</span>
                  </td>
                  <td>{row.publishedVersion == null ? "No" : `v${toNumber(row.publishedVersion)}`}</td>
                  <td>{row.authorName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {query.hasNextPage && (
        <button type="button" className={ui.buttonSecondary} disabled={query.isFetchingNextPage} onClick={() => void query.fetchNextPage()}>
          {query.isFetchingNextPage ? "Loading…" : "Show more"}
        </button>
      )}
    </section>
  );
}
