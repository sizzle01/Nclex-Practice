"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { z } from "zod";
import { TextField } from "@/components/forms/TextField";
import formStyles from "@/components/forms/forms.module.css";
import ui from "@/components/ui/ui.module.css";
import { isApiError } from "@/lib/api/errors";
import { type AdminBankView, adminKeys, createBank, fetchBank, type MemberQuestion, updateBank } from "../api/adminApi";
import styles from "../components/admin.module.css";
import { MemberList } from "./MemberList";
import { QuestionPicker } from "./QuestionPicker";
import { memberName } from "./TestSetEditor";

const bankSchema = z.object({
  track: z.enum(["RN", "PN"]),
  name: z.string().trim().min(1, "Give the bank a name.").max(200, "Keep the name under 200 characters."),
  description: z.string(),
  accessTier: z.enum(["free", "premium"]),
  isPublished: z.boolean(),
});
type BankFormValues = z.infer<typeof bankSchema>;

/** /admin/banks/new and /admin/banks/[id]: a bank's details and its questions (no order). */
export function BankEditor({ bankId }: { bankId?: string }) {
  const { data: bank, isPending, isError } = useQuery({
    queryKey: adminKeys.bank(bankId ?? ""),
    queryFn: () => fetchBank(bankId!),
    enabled: bankId !== undefined,
  });

  if (bankId !== undefined && isPending) return <p role="status">Loading the bank…</p>;
  if (bankId !== undefined && (isError || !bank)) return <p role="alert">This bank could not be loaded.</p>;
  return <BankForm key={bank?.id ?? "new"} bank={bank ?? null} />;
}

function BankForm({ bank }: { bank: AdminBankView | null }) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const [members, setMembers] = useState<MemberQuestion[]>(bank?.questions ?? []);
  const [problems, setProblems] = useState<string[]>([]);
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<BankFormValues>({
    resolver: zodResolver(bankSchema),
    defaultValues: bank
      ? {
          track: bank.track as "RN" | "PN",
          name: bank.name,
          description: bank.description ?? "",
          accessTier: bank.accessTier === "premium" ? "premium" : "free",
          isPublished: bank.isPublished,
        }
      : { track: "RN", name: "", description: "", accessTier: "free", isPublished: false },
  });
  const track = useWatch({ control, name: "track" });

  const mutation = useMutation({
    mutationFn: (values: BankFormValues) => {
      const questionIds = members.map((m) => m.questionId);
      const details = { name: values.name.trim(), description: values.description.trim(), accessTier: values.accessTier, isPublished: values.isPublished };
      return bank
        ? updateBank(bank.id, { ...details, questionIds })
        : createBank({ ...details, track: values.track, description: details.description || null, questionIds });
    },
    onSuccess(saved) {
      queryClient.setQueryData(adminKeys.bank(saved.id), saved);
      void queryClient.invalidateQueries({ queryKey: adminKeys.banks });
      setProblems([]);
      if (!bank) router.replace(`/admin/banks/${saved.id}`);
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
      <form className={formStyles.form} onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate aria-label="Question bank">
        <h1>{bank ? `Edit bank: ${bank.name}` : "New question bank"}</h1>
        {problems.length > 0 && (
          <ul role="alert" className={formStyles.formError}>
            {problems.map((p) => (
              <li key={p}>{p}</li>
            ))}
          </ul>
        )}
        <div className={formStyles.field}>
          <label htmlFor="bank-track">Track</label>
          <select id="bank-track" disabled={bank !== null} {...register("track")}>
            <option value="RN">NCLEX-RN</option>
            <option value="PN">NCLEX-PN</option>
          </select>
        </div>
        <TextField label="Name" error={errors.name?.message} {...register("name")} />
        <TextField label="Description (optional)" {...register("description")} />
        <div className={formStyles.field}>
          <label htmlFor="bank-tier">Access</label>
          <select id="bank-tier" {...register("accessTier")}>
            <option value="free">Free</option>
            <option value="premium">Premium</option>
          </select>
        </div>
        <label className={formStyles.checkbox}>
          <input type="checkbox" {...register("isPublished")} /> Published: candidates can see it
        </label>
        <h2>Questions ({members.length})</h2>
        <MemberList label="Questions in this bank" members={members} onChange={setMembers} ordered={false} />
        <div className={styles.actions}>
          <button type="submit" className={ui.button} disabled={mutation.isPending}>
            {mutation.isPending ? "Saving…" : bank ? "Save bank" : "Create bank"}
          </button>
          <p role="status">{mutation.isSuccess && bank ? "Saved." : ""}</p>
        </div>
      </form>
      <QuestionPicker track={track} chosenIds={members.map((m) => m.questionId)} onAdd={(q) => setMembers([...members, q])} />
    </div>
  );
}
