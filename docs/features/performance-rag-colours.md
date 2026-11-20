# Performance page: red, amber and green scores

## 1. Summary

Scores on the Performance page are now coloured so a candidate can see at a glance where they stand.
Below 70% is red (needs work), 70% to 85% is amber (borderline), and 86% and above is green
(strong). These match the F23 pass marks; before F23, exactly 70% was red. This applies to the overall score tile, the Score column of the breakdown table and the
Score column of each trend chart's data table. Screen readers hear the band as well, for example
"62% (needs work)", so the colour is never the only signal. Other pages (results, dashboard, history,
weak areas) are unchanged.

## 2. How to try it

```bash
docker compose up -d                       # Postgres, Redis, MinIO, Mailpit
cd nclex-be && dotnet run --project src/Nclex.Api
cd nclex-fe && npm run dev                 # http://localhost:3000
```

Sign in with an account that has finished tests (see the F01 walkthrough, section 2, for test
accounts), then open `/performance`. Change **Break down by** and **Category to follow**: every
Score cell keeps its band colour. Switch the theme to Dark: the bands use the dark variants of the
same tokens.

## 3. Reading order

1. `nclex-fe/src/features/performance/format.ts`: `performanceBand()` and `bandLabel`, the band rule.
2. `nclex-fe/src/features/performance/components/ScorePercent.tsx`: renders a percentage in its band's colour with a hidden band label.
3. `nclex-fe/src/features/performance/components/ScorePercent.module.css`: maps bands to the theme's `--error`, `--warning` and `--success` tokens.
4. `SummaryStats.tsx`, `BreakdownTable.tsx`, `TrendChart.tsx` (same folder): the three places that use it.

## 4. Request flow

Nothing new on the server. `PerformancePage` fetches the summary, breakdown and trends exactly as in
F13. Each score share (0 to 1) is passed to `<ScorePercent value={…} />`. That calls
`performanceBand()`, which rounds to the whole percentage the user sees and picks red, amber or green.
The component then renders the percentage with the band's class plus a visually hidden
" (needs work | borderline | strong)".

## 5. Database changes

None.

## 6. Key decisions and deviations

- **Bands use the displayed whole percentage.** 69.6% shows as "70%", so it is amber. Banding on the
  raw share would make it red while the label reads 70%.
- **F23 moved the red/amber edge** from "70% and below" to "below 70%" so the colours match the
  pass marks. The edges now come from `PASS_MARK` and `CLOSE_TO_PASS_MARK` in `features/results/ResultOutcome.tsx`.
- **Existing theme tokens.** Red, amber and green reuse `--error`, `--warning` and `--success`
  (`app/globals.css`), which already have light and dark variants with readable contrast. In light
  mode `--warning` is a dark amber (#a16207); in dark mode it is a bright yellow (#facc15).
- **Colour plus text.** interfaces.md 4 says information must not rely on colour alone, so each
  score carries a visually hidden band label.
- **The trend chart line stays one colour.** Only the numbers in the tables are banded.
- **Performance page only,** as you asked. The results page, dashboard, history and weak-areas card
  still show plain percentages.

## 7. Tests

- `format.test.ts`: band edges (0, 69%, 69.4%, 69.5%, 70%, 85%, 85.4%, 85.5%, 86%, 100%, and a string value).
- `ScorePercent.test.tsx`: each band renders the right `data-band` and hidden label.
- `PerformancePage.test.tsx`: the existing row assertions now include the band label.

Run: `cd nclex-fe && npm run lint && npm run typecheck && npm test`.
Results on this branch: lint clean, typecheck clean, 77 files / 489 tests passed. Backend unchanged;
`dotnet build` 0 errors and `dotnet test` passed.

## 8. Follow-ups and known limitations

- If the bands are wanted elsewhere (results breakdown, dashboard, history), move `ScorePercent` and
  `performanceBand` to a shared place and reuse them.
- The thresholds are hard-coded in the frontend. If they ever need to vary (for example per test set
  or for PN), they belong in configuration.

## 9. Try it yourself

1. Change the amber band to 75% to 85% in `performanceBand()` and update the edge cases in `format.test.ts`.
2. Colour the trend chart's dots by band using `performanceBand()` in the `dot` render prop of `<Line>`.
3. Add a small legend above the breakdown table that explains the three colours.
