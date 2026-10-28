"use client";

import Link from "next/link";
import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { toNumber } from "@/lib/format/toNumber";
import {
  type BreakdownDimension,
  type TrendBucket,
  useBreakdown,
  usePerformanceSummary,
  useTrend,
} from "../api/performanceApi";
import { breakdownDimensions, dimensionInfo } from "../dimensions";
import { BreakdownTable } from "./BreakdownTable";
import styles from "./performance.module.css";
import { SummaryStats } from "./SummaryStats";
import { TrendChart } from "./TrendChart";

/**
 * /performance (interfaces.md 5.2): totals, a breakdown by the chosen dimension, and trend charts
 * (overall and one category) by week or day. The filters sit above everything they change.
 */
export function PerformancePage() {
  const summary = usePerformanceSummary();
  const [dimension, setDimension] = useState<BreakdownDimension>("client_needs_parent");
  const [bucket, setBucket] = useState<TrendBucket>("week");
  const [category, setCategory] = useState<string>();

  const breakdown = useBreakdown(dimension);
  const rows = breakdown.data?.items ?? [];
  // The picked category may not exist in a newly chosen dimension; fall back to the first one.
  const followedKey = rows.some((r) => r.key === category) ? category : rows[0]?.key;
  const followed = rows.find((r) => r.key === followedKey);
  const overallTrend = useTrend({ dimension: "overall", bucket });
  const categoryTrend = useTrend(followedKey ? { dimension, key: followedKey, bucket } : undefined);
  const info = dimensionInfo(dimension);

  if (summary.isPending) return <p className={ui.muted}>Loading your performance…</p>;
  if (summary.isError)
    return (
      <p role="alert" className={styles.error}>
        Your performance could not be loaded. Please try again.
      </p>
    );

  return (
    <div className={styles.page}>
      <header>
        <h1 className={styles.title}>Performance</h1>
        <p className={ui.muted}>Across every test you have finished in NCLEX-{summary.data.track}. Scores count partial credit.</p>
      </header>

      {toNumber(summary.data.attemptsCount) === 0 ? (
        <section className={ui.card}>
          <h2 className={ui.cardTitle}>No finished tests yet</h2>
          <p className={ui.muted}>Finish a practice test and your scores by category and over time show up here.</p>
          <p>
            <Link href="/practice" className={ui.button}>
              Browse practice sets
            </Link>
          </p>
        </section>
      ) : (
        <>
          <SummaryStats summary={summary.data} />

          <div className={styles.controls}>
            <label className={styles.field}>
              Break down by
              <select value={dimension} onChange={(e) => setDimension(e.target.value as BreakdownDimension)}>
                {breakdownDimensions.map((d) => (
                  <option key={d.value} value={d.value}>
                    {d.label}
                  </option>
                ))}
              </select>
            </label>
            {rows.length > 0 && (
              <label className={styles.field}>
                Category to follow
                <select value={followedKey} onChange={(e) => setCategory(e.target.value)}>
                  {rows.map((r) => (
                    <option key={r.key} value={r.key}>
                      {r.name}
                    </option>
                  ))}
                </select>
              </label>
            )}
            <fieldset className={styles.segmented}>
              <legend>Show trends by</legend>
              {(["week", "day"] as const).map((value) => (
                <label key={value}>
                  <input
                    type="radio"
                    name="trend-bucket"
                    value={value}
                    checked={bucket === value}
                    onChange={() => setBucket(value)}
                  />
                  {value === "week" ? "Week" : "Day"}
                </label>
              ))}
            </fieldset>
          </div>

          <section aria-labelledby="trends-heading" className={`${ui.card} ${styles.trends}`}>
            <h2 id="trends-heading" className={ui.cardTitle}>
              Trends
            </h2>
            <TrendChart title="Overall score" points={overallTrend.data?.series} bucket={bucket} isError={overallTrend.isError} />
            {followed && (
              <TrendChart
                title={`${followed.name} score`}
                points={categoryTrend.data?.series}
                bucket={bucket}
                isError={categoryTrend.isError}
              />
            )}
          </section>

          <section aria-labelledby="breakdown-heading" className={ui.card}>
            <h2 id="breakdown-heading" className={ui.cardTitle}>
              Breakdown
            </h2>
            {breakdown.isPending && <p className={ui.muted}>Loading…</p>}
            {breakdown.isError && (
              <p role="alert" className={styles.error}>
                The breakdown could not be loaded.
              </p>
            )}
            {breakdown.isSuccess && <BreakdownTable title={info.title} column={info.column} rows={rows} />}
          </section>
        </>
      )}
    </div>
  );
}
