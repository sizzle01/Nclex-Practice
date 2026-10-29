"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { TextField } from "@/components/forms/TextField";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { adminKeys, type AdminTestSetView, createTestSet, fetchTestSet, type MemberQuestion, updateTestSet } from "../api/adminApi";
import styles from "../components/admin.module.css";
import { MemberList } from "./MemberList";
import { QuestionPicker } from "./QuestionPicker";
import { emptyTestSet, type TestSetFormValues, testSetSchema, testSetValuesFrom, toSettings } from "./testSetForm";

/** /admin/test-sets/new and /admin/test-sets/[id]: a curated set's settings and ordered questions. */
export function TestSetEditor({ setId }: { setId?: string }) {
  const { data: set, isPending, isError } = useQuery({
    queryKey: adminKeys.testSet(setId ?? ""),
    queryFn: () => fetchTestSet(setId!),
    enabled: setId !== undefined,
  });

  if (setId !== undefined && isPending) return <p role="status">Loading the set…</p>;
  if (setId !== undefined && (isError || !set)) return <p role="alert">This set could not be loaded.</p>;
  return <TestSetForm key={set?.id ?? "new"} set={set ?? null} />;
}

function TestSetForm({ set }: { set: AdminTestSetView | null }) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const [members, setMembers] = useState<MemberQuestion[]>(set?.questions ?? []);
  const [problems, setProblems] = useState<string[]>([]);
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<TestSetFormValues>({
    resolver: zodResolver(testSetSchema),
    defaultValues: set ? testSetValuesFrom(set) : emptyTestSet("RN"),
  });
  const track = useWatch({ control, name: "track" });
  const timed = useWatch({ control, name: "timed" });

  const mutation = useMutation({
    mutationFn: (values: TestSetFormValues) => {
      const questionIds = members.map((m) => m.questionId);
      const settings = toSettings(values);
      return set
        ? updateTestSet(set.id, { settings, questionIds, isPublished: values.isPublished })
        : createTestSet({ track: values.track, settings, questionIds, isPublished: values.isPublished });
    },
    onSuccess(saved) {
      queryClient.setQueryData(adminKeys.testSet(saved.id), saved);
      void queryClient.invalidateQueries({ queryKey: adminKeys.testSets });
      setProblems([]);
      if (!set) router.replace(`/admin/test-sets/${saved.id}`);
    },
    onError(error) {
      setProblems(
        isApiError(error, "VALIDATION_FAILED")
          ? error.fieldErrors.map((e) => `${memberName(e.field, members)}: ${e.message ?? e.code}`)
          : [isApiError(error) ? error.message : "Something went wrong. Please try again."],
      );
    },
  });

  return (
    <div className={styles.columns}>
      <form className={formStyles.form} onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate aria-label="Test set">
        <h1>{set ? `Edit set: ${set.settings.title}` : "New curated set"}</h1>
        {problems.length > 0 && (
          <ul role="alert" className={formStyles.formError}>
            {problems.map((p) => (
              <li key={p}>{p}</li>
            ))}
          </ul>
        )}
        <div className={formStyles.field}>
          <label htmlFor="set-track">Track</label>
          <select id="set-track" disabled={set !== null} {...register("track")}>
            <option value="RN">NCLEX-RN</option>
            <option value="PN">NCLEX-PN</option>
          </select>
        </div>
        <TextField label="Title" error={errors.title?.message} {...register("title")} />
        <TextField label="Description (optional)" {...register("description")} />
        <div className={styles.filters}>
          <label>
            Mode
            <select {...register("mode")}>
              <option value="practice">Practice</option>
              <option value="challenge">Challenge</option>
            </select>
          </label>
          <label>
            Answers shown
            <select {...register("revealMode")}>
              <option value="tutor">After each question (tutor)</option>
              <option value="exam">At the end (exam)</option>
            </select>
          </label>
          <label>
            Access
            <select {...register("accessTier")}>
              <option value="free">Free</option>
              <option value="premium">Premium</option>
            </select>
          </label>
        </div>
        <label className={formStyles.checkbox}>
          <input type="checkbox" aria-describedby={errors.timed ? "set-timed-error" : undefined} {...register("timed")} /> Timed
        </label>
        {errors.timed && (
          <p id="set-timed-error" className={formStyles.error}>
            {errors.timed.message}
          </p>
        )}
        {timed && (
          <div className={styles.filters}>
            <TextField label="Seconds per question" inputMode="numeric" error={errors.perQuestionSec?.message} {...register("perQuestionSec")} />
            <TextField label="Seconds for the whole set" inputMode="numeric" error={errors.timeBudgetSec?.message} {...register("timeBudgetSec")} />
          </div>
        )}
        <div className={styles.filters}>
          <TextField label="Pass threshold % (optional)" inputMode="decimal" error={errors.passPercent?.message} {...register("passPercent")} />
          <TextField label="Most pauses (optional)" inputMode="numeric" error={errors.maxPauses?.message} {...register("maxPauses")} />
          <TextField label="Longest pause, seconds (optional)" inputMode="numeric" error={errors.maxPauseSec?.message} {...register("maxPauseSec")} />
        </div>
        <label className={formStyles.checkbox}>
          <input type="checkbox" {...register("allowBackNav")} /> Candidates may go back to earlier questions
        </label>
        <label className={formStyles.checkbox}>
          <input type="checkbox" {...register("isSample")} /> Sample set (exactly 10 questions; open before email verification)
        </label>
        <label className={formStyles.checkbox}>
          <input type="checkbox" {...register("isPublished")} /> Published: candidates can see and start it
        </label>

        <h2>Questions, in the order they are served ({members.length})</h2>
        <MemberList label="Questions in this set" members={members} onChange={setMembers} ordered />
        <div className={styles.actions}>
          <button type="submit" className={ui.button} disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : set ? "Save set" : "Create set"}
          </button>
          <p role="status">{mutation.isSuccess && set ? "Saved." : ""}</p>
        </div>
      </form>
      <QuestionPicker track={track} chosenIds={members.map((m) => m.questionId)} onAdd={(q) => setMembers([...members, q])} />
    </div>
  );
}

/** "questionIds[2]" reads better as the question's stem. */
export function memberName(field: string, members: MemberQuestion[]): string {
  const match = /^questionIds\[(\d+)\]$/.exec(field);
  const member = match ? members[Number(match[1])] : undefined;
  return member ? `"${member.stem}"` : field;
}
