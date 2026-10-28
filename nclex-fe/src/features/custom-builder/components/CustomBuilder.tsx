"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import forms from "@/components/forms/forms.module.css";
import { TextField } from "@/components/forms/TextField";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import { isApiError } from "@/lib/api/errors";
import {
  createCustomSet,
  type CreateCustomSetRequest,
  type Difficulty,
  type ItemType,
  type PoolFilters,
  type PoolMode,
  type Track,
  useAvailableCount,
  useTaxonomy,
} from "../api/builderApi";
import {
  type BuilderInput,
  type BuilderValues,
  builderSchema,
  defaultBuilderValues,
  maxQuestions,
  minQuestions,
} from "../schema";
import { useDebouncedValue } from "../useDebouncedValue";
import { AvailableCount } from "./AvailableCount";
import styles from "./builder.module.css";
import { CategoryTree } from "./CategoryTree";
import { CheckboxGroup } from "./CheckboxGroup";
import { CreatedSetCard } from "./CreatedSetCard";

/** How long the filters must stay still before the live count is asked for. */
export const countDebounceMs = 400;

const difficultyOptions: { value: Difficulty; label: string }[] = [
  { value: "easy", label: "Easy" },
  { value: "moderate", label: "Moderate" },
  { value: "difficult", label: "Difficult" },
];

const poolOptions: { value: NonNullable<PoolMode>; label: string; hint: string }[] = [
  { value: "all", label: "All questions", hint: "Every question that matches." },
  { value: "unused", label: "Unused", hint: "Questions you have not seen yet." },
  { value: "incorrect", label: "Incorrect", hint: "Questions you last answered wrong." },
  { value: "flagged", label: "Marked for review", hint: "Questions you marked during a test." },
];

function toRequest(values: BuilderValues, track: Track, startNow: boolean): CreateCustomSetRequest {
  return {
    ...toFilters(values, track),
    questionCount: values.questionCount,
    mode: values.mode,
    revealMode: values.revealMode,
    timing: {
      timed: values.timed,
      timeBudgetSec: values.timed && values.timeBudgetMin !== undefined ? values.timeBudgetMin * 60 : null,
      perQuestionSec: values.timed ? (values.perQuestionSec ?? null) : null,
    },
    // The server starts the attempt in the same request (api.md section 3), so the player opens at once.
    startNow,
  };
}

function toFilters(values: Pick<BuilderInput, keyof Omit<PoolFilters, "track">>, track: Track): PoolFilters {
  return {
    track,
    clientNeedsIds: values.clientNeedsIds,
    bodySystemIds: values.bodySystemIds,
    itemTypes: values.itemTypes as ItemType[],
    difficulties: values.difficulties as Difficulty[],
    pool: values.pool,
  };
}

/**
 * The /practice/custom screen (interfaces.md 5.2, architecture.md 5.7): filters on the left, how
 * the test is taken on the right, and a live count of matching questions that updates once the
 * filters stop changing.
 */
export function CustomBuilder() {
  const { data: me } = useMe();
  const track = me?.track;
  const { data: taxonomy, error: taxonomyError } = useTaxonomy(track);

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<BuilderInput, unknown, BuilderValues>({
    resolver: zodResolver(builderSchema),
    defaultValues: defaultBuilderValues,
  });

  const [clientNeedsIds, bodySystemIds, itemTypes, difficulties, pool, questionCount, timed] = useWatch({
    control,
    name: ["clientNeedsIds", "bodySystemIds", "itemTypes", "difficulties", "pool", "questionCount", "timed"],
  });
  const filters = useMemo(
    () => (track ? toFilters({ clientNeedsIds, bodySystemIds, itemTypes, difficulties, pool }, track) : undefined),
    [track, clientNeedsIds, bodySystemIds, itemTypes, difficulties, pool],
  );
  const debouncedFilters = useDebouncedValue(filters, countDebounceMs);
  const count = useAvailableCount(debouncedFilters);

  const router = useRouter();
  const create = useMutation({
    mutationFn: createCustomSet,
    onSuccess: (created) => {
      if (created.attempt) router.push(`/attempts/${created.attempt.id}`);
    },
  });

  function submit(startNow: boolean) {
    return handleSubmit((values) => track && create.mutate(toRequest(values, track, startNow)));
  }

  // With startNow the player opens instead; the card is for "Create test" only.
  if (create.data && !create.data.attempt) {
    return (
      <CreatedSetCard
        created={create.data}
        onBuildAnother={() => {
          create.reset();
          reset(defaultBuilderValues);
        }}
      />
    );
  }

  const itemTypeOptions = (taxonomy?.itemTypes ?? [])
    .filter((t) => t.available)
    .map((t) => ({ value: t.type, label: t.label }));
  const bodySystemOptions = (taxonomy?.bodySystems ?? []).map((b) => ({ value: Number(b.id), label: b.name }));
  const available = count.data;

  return (
    <form
      noValidate
      aria-labelledby="builder-heading"
      className={styles.builder}
      onSubmit={submit(true)}
    >
      <div className={styles.header}>
        <h1 id="builder-heading">Build a custom test</h1>
        <p className={ui.muted}>
          Pick the topics and question types to practise{track ? ` for the NCLEX-${track}` : ""}.{" "}
          <Link href="/practice">Back to practice sets</Link>
        </p>
      </div>

      {taxonomyError && (
        <p role="alert" className={forms.formError}>
          The categories could not be loaded. Please reload the page.
        </p>
      )}

      <div className={styles.columns}>
        <div className={`${ui.card} ${styles.section}`}>
          <h2>What to include</h2>
          {taxonomy && (
            <>
              <Controller
                control={control}
                name="clientNeedsIds"
                render={({ field }) => (
                  <CategoryTree categories={taxonomy.clientNeeds} value={field.value} onChange={field.onChange} />
                )}
              />
              <Controller
                control={control}
                name="bodySystemIds"
                render={({ field }) => (
                  <CheckboxGroup
                    legend="Body systems"
                    hint="Choose none for every system."
                    options={bodySystemOptions}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
              <Controller
                control={control}
                name="itemTypes"
                render={({ field }) => (
                  <CheckboxGroup
                    legend="Question types"
                    hint="Choose none for every type."
                    options={itemTypeOptions}
                    value={field.value as ItemType[]}
                    onChange={field.onChange}
                  />
                )}
              />
            </>
          )}
          <Controller
            control={control}
            name="difficulties"
            render={({ field }) => (
              <CheckboxGroup
                legend="Difficulty"
                hint="Choose none for every level."
                options={difficultyOptions}
                value={field.value as Difficulty[]}
                onChange={field.onChange}
              />
            )}
          />
          <fieldset className={forms.fieldset}>
            <legend>Questions to use</legend>
            {poolOptions.map((option) => (
              <label key={option.value} className={forms.radio}>
                <input type="radio" value={option.value} {...register("pool")} />
                <span>
                  {option.label} <span className={ui.muted}>({option.hint})</span>
                </span>
              </label>
            ))}
          </fieldset>
        </div>

        <div className={`${ui.card} ${styles.section}`}>
          <h2>How to take it</h2>
          <TextField
            label="Number of questions"
            hint={`From ${minQuestions} to ${maxQuestions}.`}
            type="number"
            inputMode="numeric"
            min={minQuestions}
            max={maxQuestions}
            error={errors.questionCount?.message}
            {...register("questionCount", { valueAsNumber: true })}
          />
          <fieldset className={forms.fieldset}>
            <legend>Mode</legend>
            <label className={forms.radio}>
              <input type="radio" value="practice" {...register("mode")} /> Practice
            </label>
            <label className={forms.radio}>
              <input type="radio" value="challenge" {...register("mode")} /> Challenge (with a pass estimate)
            </label>
          </fieldset>
          <fieldset className={forms.fieldset}>
            <legend>Answers and rationales</legend>
            <label className={forms.radio}>
              <input type="radio" value="tutor" {...register("revealMode")} /> After each question
            </label>
            <label className={forms.radio}>
              <input type="radio" value="exam" {...register("revealMode")} /> At the end
            </label>
          </fieldset>
          <fieldset className={forms.fieldset}>
            <legend>Timing</legend>
            <label className={styles.check}>
              <input type="checkbox" {...register("timed")} /> Timed
            </label>
            {timed && (
              <>
                <TextField
                  label="Time limit for the test (minutes)"
                  type="number"
                  inputMode="numeric"
                  min={1}
                  error={errors.timeBudgetMin?.message}
                  {...register("timeBudgetMin", { valueAsNumber: true })}
                />
                <TextField
                  label="Time per question (seconds)"
                  type="number"
                  inputMode="numeric"
                  min={1}
                  error={errors.perQuestionSec?.message}
                  {...register("perQuestionSec", { valueAsNumber: true })}
                />
              </>
            )}
          </fieldset>

          <AvailableCount available={available} requested={Number(questionCount)} isFetching={count.isFetching} />

          {create.error && (
            <p role="alert" className={forms.formError}>
              {isApiError(create.error, "INSUFFICIENT_QUESTIONS")
                ? "Not enough questions match any more. Choose fewer questions or widen the filters."
                : isApiError(create.error, "EMAIL_NOT_VERIFIED")
                  ? "Verify your email address to start a test. You can still create it and start it later."
                  : "The test could not be created. Please try again."}
            </p>
          )}

          <div className={styles.actions}>
            <button type="submit" className={ui.button} disabled={create.isPending}>
              {create.isPending ? "Creating…" : "Create and start"}
            </button>
            <button type="button" className={ui.buttonSecondary} disabled={create.isPending} onClick={submit(false)}>
              Create test
            </button>
          </div>
        </div>
      </div>
    </form>
  );
}
