# F17: Subtopic recommendations

## 1. Summary

After a test, candidates now see **What to study next** on the results page: up to 5 specific subtopics they struggled with, such as "Anticoagulant therapy and monitoring: missed 2 of 3". Broad categories like "Pharmacology" are no longer the suggestion, and there are no links or reading materials. To make this possible, questions now carry subtopic tags. Staff manage the list at `/admin/subtopics`, authors tag questions in the editor, and a question can't go to review without a tag. All 44 RN sample questions are tagged.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f17`, and branches from `main` (F00 to F16).

```bash
docker compose up -d
cd nclex-be
dotnet tool restore
dotnet ef database update -p src/Nclex.Infrastructure -s src/Nclex.Api   # applies F17_Subtopics
dotnet run --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
# Imported 34 subtopic(s): 34 created, 0 updated, 0 unchanged.

dotnet run --project src/Nclex.Api          # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev   # http://localhost:3000
```

During development I used a separate database and ports so as not to disturb other branches: `nclex_f17`, the API on `:5097` and the frontend on `:3017`.

**As a candidate:**

1. Sign up (the free **RN Sample Set** needs no verified email) and start the sample set.
2. Answer a few questions wrongly, leave some blank, and submit.
3. On the results page, **What to study next** lists the subtopics you missed, most missed first. Questions you left blank don't count.
4. Finish a test with every answered question correct: it says "No specific topics to review from this test."

**As staff:** make an admin with `dotnet run --project src/Nclex.Worker -- set-role <email> admin`, or `update users set role = 'admin' where email = '…'`, then sign in again.

1. Open **Subtopics** in the admin console. Add one, rename one and deactivate one. Adding "Delegation and Supervisions" is refused because it means the same as an existing name.
2. Create a question, and try **Submit for review**: it is refused until you choose a subtopic.
3. Tag a published question. No new version is made, and past results use the new tag straight away.

**With curl:**

```bash
B=http://localhost:5080/api/v1
curl -s -H "Authorization: Bearer $TOKEN" $B/attempts/$ATTEMPT/recommendations
# -> {"attemptId":"…","subtopics":[{"id":"…","name":"Infant growth and development","missed":1,"answered":1}]}

curl -s -H "Authorization: Bearer $STAFF_TOKEN" "$B/admin/subtopics?track=RN"
curl -s -X PUT -H "Authorization: Bearer $STAFF_TOKEN" -H 'Content-Type: application/json' \
  -d "{\"subtopicIds\":[\"$SUBTOPIC_ID\"]}" $B/admin/questions/$QUESTION_ID/subtopics
```

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/Recommendations/SubtopicRecommendations.cs`: **the heart of the feature.** Which subtopics qualify and in what order.
2. `nclex-be/src/Nclex.Domain/Recommendations/SubtopicNames.cs`: slugs, the 1-to-3 limit and the comparison that spots two names for one topic.
3. `nclex-be/src/Nclex.Domain/Entities/Subtopic.cs` and `QuestionSubtopic.cs`: the two new tables, and why tags sit on the question.
4. `nclex-be/src/Nclex.Application/Attempts/Recommendations/GetRecommendationsHandler.cs`: loads the answers and calls the rules.
5. `nclex-be/src/Nclex.Application/Admin/Subtopics/QuestionSubtopicTags.cs`: tagging, and the check before review.
6. `nclex-be/src/Nclex.Application/Admin/Subtopics/AdminSubtopicsHandler.cs`: the list, and name uniqueness.
7. `nclex-be/src/Nclex.Application/Content/ImportQuestions/SubtopicImporter.cs`: the automated tag checks run on every import.
8. `nclex-be/src/Nclex.Api/Controllers/AdminSubtopicsController.cs`, then `AttemptsController.GetRecommendations`: thin controllers.
9. `nclex-fe/src/features/results/StudyRecommendations.tsx`: the card on the results page.
10. `nclex-fe/src/features/admin/editor/SubtopicPicker.tsx` and `nclex-fe/src/features/admin/subtopics/SubtopicAdmin.tsx`: the staff screens.

## 4. Request flow

1. `ResultsPage` mounts `StudyRecommendations`, which has its own TanStack Query, so a failure here leaves the score and breakdown on screen.
2. `fetchRecommendations` calls `GET /api/v1/attempts/{id}/recommendations` through the Next.js proxy.
3. `AttemptsController.GetRecommendations` passes the id to `GetRecommendationsHandler`.
4. `AttemptStore.FindOwnedAsync` returns 404 for anyone but the owner. The handler then answers 409 `ATTEMPT_NOT_ACTIVE` unless the attempt is submitted, as `/result` does.
5. One query joins `attempt_items` (with a response) to `question_versions`, `question_subtopics` and active `subtopics`, giving one `TaggedAnswer` per answered question and tag.
6. `SubtopicRecommendations.From` groups them by subtopic, keeps those with a miss, sorts them and takes 5. Partial credit counts as a miss, because `IsCorrect` means full marks.
7. The handler returns names and counts, with no keys or rationales, and the card renders the list.

Nothing is stored, so a corrected tag also corrects past results.

## 5. Database changes

Migration `F17_Subtopics`:

- **`subtopics`:** `id`, `client_needs_id`, `slug`, `name`, `is_active`, `sort_order`, with a unique index on `(client_needs_id, slug)`. A subtopic belongs to one Client Needs leaf, which also keeps RN and PN apart.
- **`question_subtopics`:** `(question_id, subtopic_id)` as the primary key, which serves the recommendations lookup, plus an index on `subtopic_id` for the question counts.

## 6. Key decisions and deviations

This feature was not in the original design. `database.md`, `api.md` and `interfaces.md` now include it.

- **Subtopics only:** after discussion, candidates get no links, reading materials or named sources.
- **Tags sit on the question, not the version:** this is a deliberate exception to "versions are immutable". A tag is study metadata, so fixing one shouldn't need re-review. The cost is that an old attempt uses today's tags, which is what we want here.
- **The `weakest` field stays:** the results page no longer shows it because it has no score floor, but the API and `result_fail` notifications still use it. Those notifications still name broad categories.
- **Names are unique across a track, not just a category:** candidates see the name alone, so the same name twice would look like a mistake. The same concept in two categories needs distinct names, such as "Recognizing hypoglycemia" and "Hypoglycemia treatment".
- **"Lowest score" in ranking uses the share of questions correct,** not points. Partial credit is already treated as a miss.
- **Import rules are stricter than the API's:** a file with a `subtopics` list must tag every question and use every entry. A file without one still imports, so older files keep working. Listing a deactivated subtopic doesn't reactivate it.
- **The starter list has no "Insulin administration" or "Medication interactions":** no sample question tests them, and unused subtopics are refused. Add them with the questions that test them.

### Automated tag audit

No clinician reviewed the list or the tags. I checked them in four automated steps:

| Check | Result |
|---|---|
| Data rules | Every question has 1 to 3 subtopics from its own category; no unused or near-duplicate subtopics. These run on every import and in tests. |
| Coverage against the [2026 NCLEX-RN test plan](https://www.ncsbn.org/publications/2026-nclex-rn-test-plan) | All 8 Client Needs areas are covered, and each of the 34 subtopics relates to a test plan activity statement. The list is **not** complete: it only covers what the 44 samples test, so most activity statements have no subtopic yet. |
| Blind re-tagging | A separate AI pass tagged all 44 questions without seeing my tags: 40 matched exactly, 4 overlapped, none disagreed entirely. |
| Disagreements | Questions 21, 24, 40 and 41 keep only the tag both passes chose. Every question ends with one tag. |

The blind pass also led to two renames: "Heart failure medications" became "Heart failure interventions", because that question is mostly about non-drug care, and "Laboratory value interpretation" became "Electrolyte and glucose lab results", because it was too broad.

**What this does not prove:** two AI passes can share a mistake, so agreement shows consistency, not clinical accuracy. Tag mistakes are low risk because candidates only see a topic name. The sample questions themselves remain unreviewed and aren't for exam preparation. Any real question bank needs a qualified human reviewer before real candidates use it.

## 7. Tests

```bash
cd nclex-be && caffeinate -i dotnet test
cd nclex-fe && npm run lint && npm run typecheck && npm test
```

- **Domain:** ranking, ties, partial credit, a miss on two tags, a perfect score, the limit of 5, slugs and name comparison.
- **Integration:**
  - Recommendations: ranking through the real API, unanswered questions, no answer data, deactivated subtopics, retagging, ownership and unsubmitted attempts.
  - Admin: roles, duplicate names, leaf-only subtopics, the 1-to-3 limit, retired questions and the review check.
  - Import: repeat imports, renames, unused and duplicate subtopics, another category, RN and PN kept apart, and the sample file.
- **Frontend:** the card's list, empty and failed states, the picker (including keyboard use, inactive subtopics and the limit of 3) and the admin screen.

## 8. Follow-ups and known limitations

- A "practice this subtopic" button.
- Dashboard or email recommendations; `result_fail` still names broad categories.
- Trends across several tests.
- A filter for finding untagged questions.
- PN subtopics: PN sample content doesn't exist yet.
- More subtopics as questions are added, to close the coverage gaps above.
- Questions in a curated set that have no tags can't appear in suggestions. If nothing else qualifies, the page says there's nothing specific to review, even if those questions were missed.

## 9. Try it yourself

1. Change the limit from 5 to 3 in `SubtopicRecommendations.MaxCount`. Which test needs updating, and why?
2. Make a subtopic qualify only after two misses. Change the `Where` in `SubtopicRecommendations.From`, then decide what `The_response_holds_no_answer_keys_or_rationales` and the ranking tests should expect.
3. Show each subtopic's Client Needs name in small text on the card. You'll need to add it to `RecommendedSubtopic` and regenerate the API types with `npm run gen:api`.
