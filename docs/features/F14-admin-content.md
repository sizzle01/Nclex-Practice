# F14: Content authoring and review console

## 1. Summary

Staff can now write, review and publish questions in the browser instead of through import files. An author creates a question of any of the 12 MVP item types in a form made for that type. A live preview shows the item exactly as candidates will see it, using F11's renderers. A reviewer works through the review queue, uses "Test the key" to grade sample answers with the real grader, then approves (never their own version) or rejects with notes. Publishing makes a version the one candidates get. In the same transaction it supersedes the previous version, while attempts already started keep the version they had. Admins also manage curated test sets, question banks and users (role, plan tier, unlock), and every admin action is recorded in `audit_events`.

## 2. How to try it

This branch lives in its own worktree, `/Users/tunde/Codes/Nclex-Practice-f14`. It was branched from `main` after F11 was fast-forwarded into it (main = `6a83f30`). F12 and F13 are still unmerged siblings: whichever of them merges after F14 must regenerate `nclex-fe/src/lib/api/schema.d.ts`. F14 adds no EF migration, so the model snapshot needs nothing.

**There is no migration.** Every table F14 writes already exists: `questions` and `question_versions` (with `reviewer_id`, `review_notes`, `reviewed_at` and `qv_review_queue_idx`), `media_assets`, banks, sets and `audit_events`.

```bash
docker compose up -d
cd nclex-be
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=nclex_f14;Username=nclex;Password=nclex"
dotnet tool restore && dotnet build
dotnet ef database update --no-build -p src/Nclex.Infrastructure -s src/Nclex.Api
dotnet run --no-build --project src/Nclex.Worker -- import-questions "$PWD/content/sample-questions.rn.json"
dotnet run --project src/Nclex.Api                    # http://localhost:5080
cd ../nclex-fe && npm install && npm run dev          # http://localhost:3000
```

**Test accounts.** Only an admin can change roles in the console, so the first staff accounts get their role from a new worker command. Sign up three accounts at <http://localhost:3000/signup> (for example `author@example.com`, `reviewer@example.com`, `admin@example.com`), then:

```bash
cd nclex-be   # with the same ConnectionStrings__Postgres exported
dotnet run --no-build --project src/Nclex.Worker -- set-role author@example.com author
dotnet run --no-build --project src/Nclex.Worker -- set-role reviewer@example.com reviewer
dotnet run --no-build --project src/Nclex.Worker -- set-role admin@example.com admin
```

Sign out and back in after a role change: the role is a claim in the access token. Staff see **Admin console** in the user menu.

**The "done when" path:**
1. As the **author**, open **Admin console → New question**. Pick an item type, for example **Matrix Multiple Response**.
   - Fill in the stem, rows and columns, and tick the correct cells.
   - Write a "why" for every cell (one box per cell appears as you add rows or columns), the overall rationale and the objective.
   - Choose the tags, then press **Create draft**. The preview on the right updates as you type.
2. Press **Submit for review**.
3. As the **reviewer**, open **Review queue** and pick the question.
   - Under **Test the key**, answer it (partly wrong, to see partial credit) and press **Grade this answer**. The marks and rationale are what a candidate's reveal would show.
   - Press **Approve**, then **Publish**.
   - Signed in as the author, the reviewer buttons are absent. An admin who wrote the version sees "another reviewer must review it".
4. As the **admin**, open **Test sets → New set**. Add the question with **Add**, save it as published, and start it as a candidate from **Practice**. The new question is served.
5. Back as the author, press **Start a new version**, change the stem, save, and take it through review and publishing again.
   - The history shows v1 as **Superseded**.
   - An attempt started before the second publish still shows the old stem.

**Sample requests** (with a staff access token in `$TOKEN`):

```bash
# Reviewer queue, oldest first
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5080/api/v1/admin/questions?status=in_review"

# Test the key on a version
curl -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"response":{"selected":"b"}}' \
  http://localhost:5080/api/v1/admin/question-versions/<versionId>/preview-grade
# -> {"points":1,"maxPoints":1,"isCorrect":true,"detail":[...]}

# Upload an image: get a signed link, PUT the bytes (no token), then add alt text
curl -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"mimeType":"image/png","bytes":8214}' http://localhost:5080/api/v1/admin/media/upload-url
curl -X PUT -H "Content-Type: image/png" --data-binary @content/media/sacral-skin.png "http://localhost:5080<uploadUrl>"
curl -X PATCH -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"altText":"Stage 2 pressure injury","captions":null,"transcript":null}' \
  http://localhost:5080/api/v1/admin/media/<mediaId>
```

## 3. Reading order

1. `nclex-be/src/Nclex.Domain/Content/VersionWorkflow.cs`: the whole review workflow as a pure rule table. It knows which status each action starts from and blocks reviewing your own version.
2. `nclex-be/src/Nclex.Domain/Entities/QuestionVersion.cs` (`CreateDraft`, `CloneAsDraft`, `EditDraft`, `SubmitForReview`, `Approve`, `Reject`) and `Question.cs` (`Publish`, `Retire`): the transitions on the entities.
3. `nclex-be/src/Nclex.Application/Admin/Questions/AuthoringValidator.cs`: the checks on every save, the same as the importer's: tags, exhibits, media, and the item-type module.
4. `Admin/Questions/VersionLoader.cs` and `MediaReadiness.cs`: workflow refusals become API errors, and media are checked before review and publishing.
5. `Admin/Questions/Publish/PublishVersionHandler.cs`: one transaction that supersedes and publishes. Then skim the other handlers in `Admin/Questions/` (create, new version, update draft, submit, approve, reject, retire, preview-grade, search).
6. `Admin/Media/*`: upload URL, signed PUT, and alt text with captions.
7. `Admin/Membership/QuestionMembership.cs`, `Admin/TestSets/AdminTestSetsHandler.cs`, `Admin/Banks/AdminBanksHandler.cs`, `Admin/Users/AdminUsersHandler.cs`.
8. `nclex-be/src/Nclex.Api/Auth/AuthSetup.cs` (the `content:*` policies), then `Controllers/Admin*Controller.cs`.
9. `nclex-be/src/Nclex.Worker/SetRoleCommand.cs`: bootstrapping the first admin.
10. `nclex-fe/src/features/admin/api/adminApi.ts` and `roles.ts`.
11. `nclex-fe/src/features/admin/item-forms/`: start with `payloads.ts`, `templates.ts` and `rationaleTargets.ts`, then `ItemForm.tsx` and one form, e.g. `MatrixForms.tsx`.
12. `nclex-fe/src/features/admin/editor/`: `draftValues.ts` (Zod schema and conversions), `DraftForm.tsx`, `LivePreview.tsx`, `TestKeyPanel.tsx`, `WorkflowPanel.tsx`, `QuestionEditor.tsx`.
13. `nclex-fe/src/features/admin/catalog/` and `users/`, then `src/app/admin/**` (thin route files).

## 4. Request flow: publishing a version

1. **UI.** On `/admin/questions/[id]`, `WorkflowPanel` shows **Publish** because the version is `approved` and `canReview(me.role)`. Clicking it calls `runWorkflowStep(versionId, "publish")`, which sends `POST /api/v1/admin/question-versions/{id}/publish` through the typed client.
2. **API.** `AdminQuestionVersionsController.Publish` carries `[Authorize(Policy = Policies.ContentReviewer)]`, so only a reviewer or admin token gets in (403 otherwise). It calls `PublishVersionHandler`.
3. **Application.**
   - `VersionLoader.LoadAsync` loads the version and its question, tracked.
   - `EnsureActive` refuses a retired question. `EnsureAllowed(Publish)` asks `VersionWorkflow.Check`, and anything other than `approved` gives `409 INVALID_STATUS_TRANSITION`.
   - `MediaReadiness` re-checks the version's media: alt text, captions, transcript, file uploaded.
   - The handler loads the currently published version, if any.
4. **Domain.** `Question.Publish(version, previous, now)` checks that `previous` really is the current version. It marks it `superseded`, marks the new one `published` with `published_at`, and points `current_version_id` at it.
5. **Database.** The audit row `question.publish` (ids and version numbers only) is added, and one `SaveChanges` writes the three rows and the audit event in a single transaction. Attempt items keep their own `question_version_id`, so attempts in progress are unaffected. `StartAttemptHandler` resolves `current_version_id` for new attempts.
6. **Back.** `AdminQuestionReader` returns the question with all versions. The panel caches it, refreshes the lists, and the history now shows the new status.

## 5. Database changes

None. F14 only writes columns F04 created: `question_versions.reviewer_id`, `review_notes`, `reviewed_at`, `published_at`, `questions.status` and `retired_at`, plus rows in `media_assets`, `question_banks`, `question_bank_items`, `test_sets`, `test_set_items` and `audit_events`. The reviewer queue uses the existing partial index `qv_review_queue_idx (status, created_at) WHERE status = 'in_review'`.

## 6. Key decisions and deviations

- **Uploads go to the API, not straight to MinIO.** api.md describes a presigned S3 PUT. That needs an S3 SDK, which is outside the agreed stack, so `upload-url` returns a signed, short-lived link to `PUT /admin/media/{id}/upload`. It works exactly like a presigned URL (no token, the signature is the permission, it expires) and stores through F11's `IMediaStorage` (local disk). An S3 or MinIO storage can later return a real presigned URL from the same endpoint. The upload signature differs from download signatures (`MediaPart.Upload`), so a viewing link can't overwrite a file.
- **New error code `INVALID_STATUS_TRANSITION` (409)**, added to api.md section 1: for approving a draft, editing a version in review, a second open version, or a retired question.
- **`errors[]` entries may carry a `message`** (additive). Item-type problems such as "Option c needs a rationale" need more than a code. The editor lists them by path.
- **One open version per question.** "New version" is refused while a draft, in-review or approved version exists, so two people never rework the same question at once. It copies the published version, or the latest one when nothing is published (for example after a rejection).
- **Any author may edit any draft.** The version keeps its creator as `author_id`, which is who is barred from reviewing it. An admin who edits someone else's draft could then approve it; admins are trusted with everything else too, and the audit log records both actions.
- **Rejecting your own version is blocked as well as approving it.** Both are review decisions (`reviewer_id <> author_id`).
- **Media are checked at submit-for-review and at publish, not on every save.** A draft may refer to an image whose alt text is still being written. Publishing re-checks, because alt text can change after review.
- **A role change revokes the user's refresh sessions**, so the new role applies when they next sign in (at most 15 minutes for the current access token). An admin can't change their own role.
- **`set-role` worker command** (not in the feature list): without it there is no way to make the first admin. It needs database access, which is already operator access.
- **Search additions.** `GET /admin/questions` also takes `questionStatus` (active or retired) and `limit`/`cursor`. `status=published` means "has a current version", because a published question's latest version may be a newer draft. All of this is in api.md section 8.
- **Sets and banks PATCH whole parts** (all settings, or the whole ordered list), rather than field by field. This matches how the editor saves and avoids ambiguous nulls such as "no timer" versus "unchanged".
- **`TestSetSettings.HasConsistentTiming` is now `[JsonIgnore]`.** It was a computed rule leaking into JSON and the OpenAPI schema.
- **New routes** `/admin/questions/new`, `/admin/test-sets/new`, `/admin/banks/new` and `/admin/users`, added to interfaces.md 5.2.
- **Case studies are not authored here.** A case-study item keeps its case and position when edited; authoring cases (scenario tabs, positions) is a follow-up.

## 7. Tests

```bash
cd nclex-be && dotnet build && dotnet test
cd nclex-fe && npm run lint && npm run typecheck && npm test
```

**Backend**
- `Nclex.Domain.Tests/Content/VersionWorkflowTests.cs`: every action from its own status, every other status refused, self-review blocked, open statuses.
- `Entities/QuestionVersionWorkflowTests.cs`:
  - Draft → review → approve → publish.
  - Editing only while a draft.
  - Approve and reject rules.
  - A clone is a fresh draft with the same content and a new author.
  - Retiring twice is refused.
- `Nclex.Api.IntegrationTests/Admin/QuestionWorkflowTests.cs`:
  - The full path into a new practice set, with no key or rationale sent to the candidate.
  - Self-approval refused.
  - Out-of-order actions give 409.
  - Superseding: the old attempt keeps v1 and a new attempt gets v2.
  - Only one open version at a time.
  - Rejection needs notes, followed by a rework.
  - Item-type validation messages, and a wrong-track category refused.
  - Test the key.
  - A retired question leaves sets.
  - Stem search ignores case and treats `%` as text.
- `AdminRoleGuardTests.cs`: 401 signed out, candidates 403 everywhere, authors can't review or manage sets, reviewers can't write.
- `AdminMediaTests.cs`:
  - Upload, describe, and serve the file.
  - A changed signature is refused.
  - Size and type must match what was declared, and only allowed types and sizes get a link.
  - Alt text is required before review.
  - Captions must be WebVTT.
- `AdminCatalogAndUsersTests.cs`:
  - Publishing a set later.
  - Sets refuse drafts, duplicates and bad timers.
  - Bank membership.
  - A role change revokes sessions.
  - An admin can't change their own role.
  - User search.

**Frontend**
- `item-forms/ItemForm.test.tsx`: an axe check for every type's form, plus behaviour for MC, SATA, ordered, matrix MR, cloze, drop-down table, highlight and calculation.
- `item-forms/rationaleTargets.test.ts` and `editor/draftValues.test.ts`.
- `editor/DraftForm.test.tsx`: browser checks, the live preview, server problems placed by field or listed, the save shape, and read-only with axe.
- `editor/WorkflowPanel.test.tsx`: submit, approve or reject with notes, no self-review, publish refusal reasons, retire confirmation.
- `editor/TestKeyPanel.test.tsx`, `components/AdminGate.test.tsx`, `catalog/TestSetEditor.test.tsx` (ordering, server refusals by stem, timing rule) and `users/UserAdmin.test.tsx`.

**Results on this branch (2026-10-29):**
- `dotnet build`: 0 warnings, 0 errors.
- `dotnet test`: 1010 passed, 0 failed (domain, application and integration).
- `npm run lint`: clean. `npm run typecheck`: clean. `npm test`: 58 files, 361 tests passed.

The admin tests create their sets as challenge sets titled "zz…". They sort after every other set, so creating them can't shift the offset pages that `TestSetsTests.Pages_follow_the_cursor_without_repeating_sets` reads at the same time. An earlier full run failed that test once, with practice sets inserted mid-paging.

## 8. Follow-ups and known limitations

- **Flag moderation (F15)**: `/admin/flags` and `PATCH /admin/flags/{id}`.
- **S3/MinIO storage** behind `IMediaStorage`, with real presigned URLs from `upload-url`. Assets created but never uploaded are left behind and need a clean-up job.
- **The Next.js proxy and large uploads.** Uploads go through the `/api` rewrite. Files near the 25 MB limit should be tried against a production build before relying on them.
- **Case-study authoring** (scenario tabs, positions, whole-case review) is not in the console. Case items can be edited one at a time and keep their place.
- **Question access tier** is chosen at creation only; there's no endpoint to change it later.
- **Media details aren't loaded back into the exhibit editor.** After a reload it shows the media id, but not the alt text already saved. Reviewers see it in the preview.
- **Audit rows from the admin console have no IP address**, unlike sign-in events. The handlers don't receive the client address.
- **No paging in the set and bank lists** beyond the first 100 (the API pages; the screens don't yet).
- **Integration tests need the machine awake.** Access tokens last 15 minutes, and a laptop that sleeps mid-run makes later requests 401. `caffeinate -i dotnet test` avoids it on macOS.

## 9. Try it yourself

1. **Let reviewers leave notes when they publish.** `PublishVersionHandler` takes no body today. Add an optional `ReviewRequest`, store the notes in the audit metadata (not on the version, which is immutable), and add a notes box to `WorkflowPanel` for the approved status. Regenerate `schema.d.ts` with `npm run gen:api` while the API runs.
2. **Show "your drafts" first.** Add a `mine=true` query parameter to `SearchQuestionsRequest` that filters `AuthorId == currentUser.UserId`, and a checkbox on `QuestionList`. Which layer owns the current user's id, and why isn't it sent from the browser?
3. **Add a "duplicate question" action.** It copies a question's current version into a brand-new question as a draft. Reuse `QuestionVersion.CloneAsDraft`'s idea, but think about which fields belong to the question (track, access tier) rather than the version, and which audit action to record.
