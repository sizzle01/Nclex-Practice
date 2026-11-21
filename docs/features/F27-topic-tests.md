# F27: Topic tests on the Practice page

## 1. Summary
The Practice page has a new "Topics" tab. It lists one test for each subtopic of the review book, grouped under the
book's sections: Fundamental, Holistic health assessment, Pharmacology, Mental health, Maternity, Pediatrics and
Med-Surg.

Each topic test:
- serves only questions on that subtopic, with its case studies first (kept whole and in order) and then its
  standalone questions;
- is untimed, with no answers during the test;
- ends with the score, the Pass / Close to pass / Fail outcome, the clinical judgment panel, and every rationale on
  the review page.

Candidates can test themselves topic by topic. Topics with fewer than 10 questions stay hidden until the case
conversion fills them. Long topics are split into even parts of up to 50 questions.

## 2. How to try it
Build the bank and import it (RN-question-bank.md), then sign in as a verified candidate and open `/practice`.
1. Choose the **Topics** tab (or go to `/practice?filter=topics`).
2. Open "Pharmacology › Endocrine: insulin". The panel says it is untimed, with answers at the end, and shows the
   pass mark.
3. Start it, answer a few questions, and submit. The results page shows the score with Pass, Close to pass or Fail.

Sample request:
```bash
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5080/api/v1/test-sets?limit=50" | jq '.items[] | select(.topicSection) | {title, topicSection, questionCount}'
```

## 3. Reading order
1. `nclex-be/content/rn-bank/book_topics.py`: `BOOK` (every subtopic and its case studies) and `MATCH` (which
   standalone questions belong, by tag and stem keywords).
2. `nclex-be/content/rn-bank/catalog.py` (`_topic_sets`, `_matches`): builds the topic tests, splits them into even
   parts and decides what is published.
3. `nclex-be/src/Nclex.Domain/Entities/TestSetSettings.cs` and `TestSet.cs` (`TopicSection`).
4. `nclex-be/src/Nclex.Application/Content/ImportQuestions/CatalogImporter.cs` (`CheckSettings`): validates the
   section.
5. `nclex-be/src/Nclex.Application/Catalog/TestSetSummary.cs` and `ListTestSets/ListTestSetsHandler.cs`: return
   `topicSection`.
6. `nclex-fe/src/features/practice/setCategories.ts` (`isTopicTest`, `groupTopicSets`, the `topics` filter).
7. `nclex-fe/src/features/practice/components/PracticeCatalog.tsx`: the tab and its empty message.

## 4. Request flow
1. **Build.** `build_all.py` calls `catalog.build_catalog`. `_topic_sets` walks `book_topics.BOOK` in order. For each
   subtopic it:
   - takes the listed case studies;
   - adds every active standalone question whose tags or stem match `MATCH`;
   - splits the result into even parts of at most 50 questions.
   Each part gets set number 5000 + subtopic index × 20 + part, so a subtopic keeps its ids as the bank grows. The
   settings are `mode: practice`, `revealMode: exam`, untimed, back navigation allowed and `topicSection` set to the
   book section. Fewer than 10 questions means `isPublished: false`.
2. **Import.** `CatalogImporter` checks the section (1 to 80 characters) and stores it in
   `test_sets.topic_section`, along with the usual check that case studies stay whole and in order.
3. **Practice page.** `usePrebuiltSets` loads every set, as before.
   - `filterSets` keeps topic tests out of All, Practice, Timed and Untimed, and shows them only under Topics.
   - `groupTopicSets` groups them by `topicSection` in book order.
4. **Taking a test.** This is the existing untimed practice path with answers at the end. F23 grades it against the
   86% pass mark, and the F25 panel shows clinical judgment by step and by half.

## 5. Database changes
Migration `F27_TestSetTopic` adds `test_sets.topic_section text NULL`, documented in database.md. It is NULL for every
set that is not a topic test.

## 6. Key decisions and deviations
- **Pass / Close to pass / Fail is shown** (user decision). Topic tests use F23's outcome, unlike the timed blocks.
- **Hidden below 10 questions; parts of up to 50, kept even** (recommendations accepted).
- **An explicit section instead of guessing from titles.** Case sets and timed blocks are still recognized by title.
  `topicSection` is the first explicit grouping field, and the comment in `setCategories.ts` notes the older approach.
- **Topic tests appear only under Topics,** so the other tabs are not flooded with about 100 extra sets.
- **The branch stacks on `content/case-conversion`,** because `book_topics.py` lives there. Merge order:
  case-conversion, then F27.
- **Matching is by tags and stem keywords.** A standalone question can belong to more than one topic; for example, a
  warfarin question can count for both anticoagulants and Lab. Keyword rules were narrowed where they caught too
  much (head-to-toe, defense mechanisms, toddler ages).
- **The admin form carries `topicSection` through unchanged,** so saving a topic test in admin does not clear it.

## 7. Tests
- Backend, `TestSetsTests`: a topic test returns its section in the list and the detail; a blank section is refused.
- Frontend:
  - `setCategories.test.ts`: topic tests appear only under Topics, are grouped in book order, and the tab is read
    from the URL;
  - `PracticeCatalog.test.tsx`: six tabs, keyboard movement, and the Topics tab listing sections.
- Trial import of the whole bank: 77 published topic tests and 26 hidden, alongside the 368 retirements.

To run them:
```bash
cd nclex-be && caffeinate -i dotnet test
cd nclex-fe && npm run lint && npm run typecheck && npm test
```
Results at commit time: 1,455 backend tests passed; lint and typecheck clean; 507 frontend tests passed.
`dotnet ef migrations has-pending-model-changes` reports none.

## 8. Follow-ups and known limitations
- 26 subtopics are hidden until their chunks add cases: head-to-toe, abruption, previa, antilipidemics, transplant,
  STIs and others. Each chunk should add its cases to `book_topics.BOOK`.
- Keyword matching can include an odd question or miss one. Review the topic sizes in the build output after each
  chunk.
- A "topics completed" view, showing which topic tests the candidate has passed, would make progress through the
  book visible.

## 9. Try it yourself
1. Lower `TOPIC_MIN_QUESTIONS` to 5 and rebuild. Which topics appear?
2. Add a stem keyword to "Antilipidemics" in `MATCH` (for example "triglyceride") and see how the topic's size
   changes.
3. Move the Topics tab to be the second tab in `filters`. Which tests change, and why?
