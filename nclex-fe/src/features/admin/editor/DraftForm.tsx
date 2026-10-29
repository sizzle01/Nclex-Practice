"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMemo, useState } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { TextField } from "@/components/forms/TextField";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { type ClientNeedsNode, type Track, useTaxonomy } from "@/features/custom-builder/api/builderApi";
import { type ApiError, isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import type { DraftInput } from "../api/adminApi";
import { ItemForm } from "../item-forms/ItemForm";
import { authorableTypes, type AuthorableType } from "../item-forms/payloads";
import { alignPerOption, rationaleTargets } from "../item-forms/rationaleTargets";
import { emptyItem } from "../item-forms/templates";
import { type DraftFormInput, type DraftFormValues, draftSchema, serverFieldMap, toDraftInput } from "./draftValues";
import styles from "./editor.module.css";
import { ExhibitsEditor } from "./ExhibitsEditor";
import { LivePreview } from "./LivePreview";
import { PerOptionEditor } from "./PerOptionEditor";

interface DraftFormProps {
  track: Track;
  initialValues: DraftFormInput;
  /** Shows the version without letting anyone change it (not a draft, or not an author). */
  readOnly: boolean;
  saveLabel: string;
  onSave(draft: DraftInput): Promise<unknown>;
}

/** A server problem the form could not put next to a field, e.g. "content.options[2].text". */
interface Problem {
  field: string;
  message: string;
}

/**
 * The question editor (interfaces.md 5.2): tags, stem, the item type's own form, a "why" for every
 * choice, exhibits, and a live candidate preview beside it. Quick checks run in the browser; the
 * server checks the item against its type's schema on every save, and its problems are listed.
 */
export function DraftForm({ track, initialValues, readOnly, saveLabel, onSave }: DraftFormProps) {
  const [problems, setProblems] = useState<Problem[]>([]);
  const [formError, setFormError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);
  const { data: taxonomy } = useTaxonomy(track);
  const {
    register,
    control,
    handleSubmit,
    setError,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<DraftFormInput, unknown, DraftFormValues>({ resolver: zodResolver(draftSchema), defaultValues: initialValues });

  const itemType = useWatch({ control, name: "itemType" }) as AuthorableType;
  const item = useWatch({ control, name: "item" });
  const stem = useWatch({ control, name: "stem" });
  const exhibits = useWatch({ control, name: "exhibits" });
  const targets = useMemo(() => rationaleTargets(itemType, item.content), [itemType, item.content]);
  const leaves = useMemo(() => leafCategories(taxonomy?.clientNeeds ?? []), [taxonomy]);

  const onSubmit = handleSubmit(async (values) => {
    setProblems([]);
    setFormError(null);
    setSaved(false);
    try {
      // Only the targets the content has now: a removed option's old "why" is not sent.
      await onSave(toDraftInput({ ...values, perOption: alignPerOption(values.perOption, targets) }));
      setSaved(true);
    } catch (error) {
      if (isApiError(error, "VALIDATION_FAILED")) {
        setProblems(placeProblems(error, (field, message) => setError(field, { type: "server", message })));
        setFormError("The draft was not saved. Fix the problems listed and save again.");
      } else {
        setFormError(isApiError(error) ? error.message : "Something went wrong. Please try again.");
      }
    }
  });

  function changeType(next: AuthorableType) {
    setValue("itemType", next);
    setValue("item", emptyItem(next));
    setValue("perOption", {});
  }

  return (
    <div className={styles.split}>
      <form className={formStyles.form} onSubmit={onSubmit} noValidate aria-label="Question draft">
        {formError && (
          <p role="alert" className={formStyles.formError}>
            {formError}
          </p>
        )}
        {problems.length > 0 && (
          <ul className={styles.problems} aria-label="Problems found by the server">
            {problems.map((p) => (
              <li key={`${p.field}:${p.message}`}>
                <code>{p.field}</code>: {p.message}
              </li>
            ))}
          </ul>
        )}

        <fieldset className={styles.fields} disabled={readOnly}>
          <div className={formStyles.field}>
            <label htmlFor="field-itemType">Item type</label>
            <select id="field-itemType" value={itemType} onChange={(event) => changeType(event.target.value as AuthorableType)}>
              {authorableTypes.map((t) => (
                <option key={t.type} value={t.type}>
                  {t.label}
                </option>
              ))}
            </select>
            <p className={formStyles.hint}>Changing the type starts its content again.</p>
          </div>

          <div className={formStyles.field}>
            <label htmlFor="field-stem">Stem</label>
            <textarea id="field-stem" rows={4} aria-invalid={errors.stem ? true : undefined} aria-describedby={errors.stem ? "field-stem-error" : undefined} {...register("stem")} />
            {errors.stem && (
              <p id="field-stem-error" className={formStyles.error}>
                {errors.stem.message}
              </p>
            )}
          </div>

          <Controller
            control={control}
            name="item"
            render={({ field }) => <ItemForm itemType={itemType} value={field.value} onChange={field.onChange} disabled={readOnly} />}
          />

          <div className={formStyles.field}>
            <label htmlFor="field-rationaleCorrect">Why the correct answer is correct</label>
            <textarea
              id="field-rationaleCorrect"
              rows={3}
              aria-invalid={errors.rationaleCorrect ? true : undefined}
              aria-describedby={errors.rationaleCorrect ? "field-rationaleCorrect-error" : undefined}
              {...register("rationaleCorrect")}
            />
            {errors.rationaleCorrect && (
              <p id="field-rationaleCorrect-error" className={formStyles.error}>
                {errors.rationaleCorrect.message}
              </p>
            )}
          </div>

          <Controller
            control={control}
            name="perOption"
            render={({ field }) => <PerOptionEditor targets={targets} value={field.value} onChange={field.onChange} disabled={readOnly} />}
          />

          <TextField label="Educational objective" error={errors.educationalObjective?.message} {...register("educationalObjective")} />

          <div className={formStyles.field}>
            <label htmlFor="field-references">References (one per line, optional)</label>
            <textarea id="field-references" rows={2} {...register("references")} />
          </div>

          <div className={styles.tagGrid}>
            <SelectField label="Client Needs" name="clientNeedsId" error={errors.clientNeedsId?.message} register={register}>
              <option value="">Choose…</option>
              {leaves.map((leaf) => (
                <option key={String(leaf.id)} value={String(leaf.id)}>
                  {leaf.label}
                </option>
              ))}
            </SelectField>
            <SelectField label="Body system" name="bodySystemId" error={errors.bodySystemId?.message} register={register}>
              <option value="">Choose…</option>
              {(taxonomy?.bodySystems ?? []).map((b) => (
                <option key={String(b.id)} value={String(b.id)}>
                  {b.name}
                </option>
              ))}
            </SelectField>
            <SelectField label="Clinical judgment step" name="cjmmSkill" register={register}>
              <option value="">None</option>
              {(taxonomy?.cjmmSkills ?? []).map((s) => (
                <option key={String(s.key)} value={String(s.key)}>
                  {s.label}
                </option>
              ))}
            </SelectField>
            <SelectField label="Difficulty" name="difficulty" error={errors.difficulty?.message} register={register}>
              <option value="easy">Easy</option>
              <option value="moderate">Moderate</option>
              <option value="difficult">Difficult</option>
            </SelectField>
            <TextField label="Time allotment (seconds)" type="number" min={1} error={errors.timeAllotmentSec?.message} {...register("timeAllotmentSec")} />
          </div>

          <Controller
            control={control}
            name="exhibits"
            render={({ field }) => <ExhibitsEditor value={field.value} onChange={field.onChange} disabled={readOnly} />}
          />
        </fieldset>

        {!readOnly && (
          <div className={styles.actions}>
            <button type="submit" className={ui.button} disabled={isSubmitting}>
              {isSubmitting ? "Saving…" : saveLabel}
            </button>
            <p role="status" className={saved ? formStyles.formSuccess : undefined}>
              {saved ? "Saved." : ""}
            </p>
          </div>
        )}
      </form>
      <LivePreview itemType={itemType} stem={stem} content={item.content} exhibits={exhibits} />
    </div>
  );
}

interface SelectFieldProps {
  label: string;
  name: "clientNeedsId" | "bodySystemId" | "cjmmSkill" | "difficulty";
  error?: string;
  register: ReturnType<typeof useForm<DraftFormInput, unknown, DraftFormValues>>["register"];
  children: React.ReactNode;
}

function SelectField({ label, name, error, register, children }: SelectFieldProps) {
  const id = `field-${name}`;
  return (
    <div className={formStyles.field}>
      <label htmlFor={id}>{label}</label>
      <select id={id} aria-invalid={error ? true : undefined} aria-describedby={error ? `${id}-error` : undefined} {...register(name)}>
        {children}
      </select>
      {error && (
        <p id={`${id}-error`} className={formStyles.error}>
          {error}
        </p>
      )}
    </div>
  );
}

/** Questions are tagged to a leaf of the Client Needs tree (database.md 2.3), shown with its parent. */
export function leafCategories(nodes: ClientNeedsNode[], parent = ""): Array<{ id: number; label: string }> {
  return nodes.flatMap((node) => {
    const label = parent ? `${parent} › ${node.name}` : node.name;
    return node.children && node.children.length > 0 ? leafCategories(node.children, label) : [{ id: toNumber(node.id), label }];
  });
}

/** Puts each server problem next to its field when there is one; returns the rest to list. */
function placeProblems(error: ApiError, place: (field: keyof DraftFormInput, message: string) => void): Problem[] {
  const rest: Problem[] = [];
  for (const { field, code, message } of error.fieldErrors) {
    const text = message ?? code;
    const formField = serverFieldMap[field];
    if (formField) place(formField, text);
    else rest.push({ field, message: text });
  }
  return rest;
}
