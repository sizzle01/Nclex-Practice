"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import formStyles from "@/components/forms/forms.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import type { Track } from "@/features/custom-builder/api/builderApi";
import { adminKeys, createQuestion, type DraftInput } from "../api/adminApi";
import { DraftForm } from "./DraftForm";
import { emptyFormValues } from "./draftValues";
import styles from "./editor.module.css";

/** /admin/questions/new: a question whose version 1 starts as a draft (api.md section 8). */
export function NewQuestion() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { data: me } = useMe();
  const [track, setTrack] = useState<Track>(me?.track ?? "RN");
  const [accessTier, setAccessTier] = useState<"free" | "premium">("free");

  async function save(draft: DraftInput) {
    const created = await createQuestion({ track, accessTier, draft });
    queryClient.setQueryData(adminKeys.question(created.id), created);
    router.replace(`/admin/questions/${created.id}`);
  }

  return (
    <div className={styles.fields}>
      <h1>New question</h1>
      <div className={styles.tagGrid}>
        <div className={formStyles.field}>
          <label htmlFor="new-track">Track</label>
          <select id="new-track" value={track ?? "RN"} onChange={(event) => setTrack(event.target.value as Track)}>
            <option value="RN">NCLEX-RN</option>
            <option value="PN">NCLEX-PN</option>
          </select>
          <p className={formStyles.hint}>Fixed once saved; it decides the Client Needs categories.</p>
        </div>
        <div className={formStyles.field}>
          <label htmlFor="new-tier">Access</label>
          <select id="new-tier" value={accessTier} onChange={(event) => setAccessTier(event.target.value as "free" | "premium")}>
            <option value="free">Free</option>
            <option value="premium">Premium</option>
          </select>
        </div>
      </div>
      {/* A new form per track: the Client Needs choices differ. */}
      <DraftForm key={track} track={track} initialValues={emptyFormValues("multiple_choice")} readOnly={false} saveLabel="Create draft" onSave={save} />
    </div>
  );
}
