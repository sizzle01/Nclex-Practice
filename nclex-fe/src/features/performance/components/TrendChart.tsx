"use client";

import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import ui from "@/components/ui/ui.module.css";
import { toNumber } from "@/lib/format/toNumber";
import { elapsed, percent } from "@/features/results/format";
import type { TrendBucket, TrendPoint } from "../api/performanceApi";
import { bucketLabel, bucketTick } from "../format";
import styles from "./performance.module.css";

type Props = {
  title: string;
  points: TrendPoint[] | undefined;
  bucket: TrendBucket;
  isError?: boolean;
};

type ChartRow = { bucketStart: string; pct: number; items: number; avgTimeMs: number };

/**
 * One trend line with its data table beside it (interfaces.md 4: never information only in a
 * chart). The chart is a picture for sighted users; the table carries the same numbers for screen
 * readers and keyboards, so the chart itself is labelled as an image and takes no focus.
 */
export function TrendChart({ title, points, bucket, isError }: Props) {
  const titleId = `trend-${title.replace(/\W+/g, "-").toLowerCase()}`;
  return (
    <figure className={styles.trend} aria-labelledby={titleId}>
      <figcaption id={titleId}>{title}</figcaption>
      <TrendBody title={title} points={points} bucket={bucket} isError={isError} />
    </figure>
  );
}

function TrendBody({ title, points, bucket, isError }: Props) {
  if (isError)
    return (
      <p role="alert" className={styles.error}>
        This trend could not be loaded.
      </p>
    );
  if (!points) return <p className={ui.muted}>Loading…</p>;
  if (points.length === 0) return <p className={ui.muted}>No finished tests in this period yet.</p>;

  const rows: ChartRow[] = points.map((p) => ({
    bucketStart: p.bucketStart,
    pct: Math.round(toNumber(p.pct) * 100),
    items: toNumber(p.items),
    avgTimeMs: toNumber(p.avgTimeMs),
  }));
  const last = rows[rows.length - 1];

  return (
    <div className={styles.trendBody}>
      <div
        className={styles.chart}
        role="img"
        aria-label={`${title}: line chart of ${rows.length} ${bucket === "week" ? "weeks" : "days"}, latest ${last.pct}%. The numbers are in the table that follows.`}
      >
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={rows} margin={{ top: 8, right: 16, bottom: 0, left: 0 }} accessibilityLayer={false}>
            <CartesianGrid vertical={false} stroke="var(--border)" />
            <XAxis
              dataKey="bucketStart"
              tickFormatter={bucketTick}
              stroke="var(--border-strong)"
              tick={{ fill: "var(--muted)", fontSize: 12 }}
              tickLine={false}
            />
            <YAxis
              domain={[0, 100]}
              ticks={[0, 25, 50, 75, 100]}
              tickFormatter={(value: number) => `${value}%`}
              width={44}
              stroke="var(--border-strong)"
              tick={{ fill: "var(--muted)", fontSize: 12 }}
              tickLine={false}
              axisLine={false}
            />
            <Tooltip
              cursor={{ stroke: "var(--border-strong)" }}
              content={({ active, payload }) => {
                const row = active ? (payload?.[0]?.payload as ChartRow | undefined) : undefined;
                if (!row) return null;
                return (
                  <div className={styles.tooltip}>
                    <p>
                      <strong>{bucketLabel(row.bucketStart, bucket)}</strong>
                    </p>
                    <p>
                      {row.pct}% on {row.items} questions
                    </p>
                    <p className={ui.muted}>{elapsed(row.avgTimeMs)} per question</p>
                  </div>
                );
              }}
            />
            <Line
              type="monotone"
              dataKey="pct"
              stroke="var(--accent)"
              strokeWidth={2}
              dot={{ r: 4, fill: "var(--accent)", stroke: "var(--surface)", strokeWidth: 2 }}
              activeDot={{ r: 6, fill: "var(--accent)", stroke: "var(--surface)", strokeWidth: 2 }}
              isAnimationActive={false}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>

      <div className={styles.tableScroll}>
        <table className={styles.table}>
          <caption>{title}: data</caption>
          <thead>
            <tr>
              <th scope="col">{bucket === "week" ? "Week" : "Day"}</th>
              <th scope="col">Score</th>
              <th scope="col">Questions</th>
              <th scope="col">Avg time</th>
            </tr>
          </thead>
          <tbody>
            {points.map((p) => (
              <tr key={p.bucketStart}>
                <th scope="row">{bucketLabel(p.bucketStart, bucket)}</th>
                <td>{percent(p.pct)}</td>
                <td>{toNumber(p.items)}</td>
                <td>{elapsed(p.avgTimeMs)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
