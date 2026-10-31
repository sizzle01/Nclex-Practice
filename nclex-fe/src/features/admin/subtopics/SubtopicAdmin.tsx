"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { type FormEvent, useMemo, useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { type Track, useTaxonomy } from "@/features/custom-builder/api/builderApi";
import { isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import { adminKeys, createSubtopic, listSubtopics, type SubtopicView, updateSubtopic, type UpdateSubtopicRequest } from "../api/adminApi";
import styles from "../components/admin.module.css";
import { leafCategories } from "../editor/DraftForm";

function problem(error: unknown): string {
  return isApiError(error) ? error.message : "Something went wrong. Please try again.";
}

/**
 * /admin/subtopics: the specific topics questions are tagged with, which are what candidates are
 * told to study after a test (docs/features/F17). Subtopics are deactivated rather than deleted.
 */
export function SubtopicAdmin() {
  const [track, setTrack] = useState<Track>("RN");
  const { data, isPending, isError } = useQuery({
    queryKey: adminKeys.subtopics({ track }),
    queryFn: () => listSubtopics({ track }),
  });

  return (
    <section aria-labelledby="subtopics-heading">
      <h1 id="subtopics-heading">Subtopics</h1>
      <div className={styles.filters}>
        <label>
          Track
          <select value={track} onChange={(event) => setTrack(event.target.value as Track)}>
            <option value="RN">RN</option>
            <option value="PN">PN</option>
          </select>
        </label>
      </div>
      <NewSubtopicForm key={track} track={track} />
      {isPending ? (
        <p role="status">Loading subtopics…</p>
      ) : isError ? (
        <p role="alert">The subtopics could not be loaded.</p>
      ) : data.items.length === 0 ? (
        <p role="status">This track has no subtopics yet.</p>
      ) : (
        <div className={styles.tableWrap}>
          <table className={styles.table} aria-labelledby="subtopics-heading">
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Client Needs</th>
                <th scope="col">Questions</th>
                <th scope="col">Status</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((subtopic) => (
                <SubtopicRow key={subtopic.id} subtopic={subtopic} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

function NewSubtopicForm({ track }: { track: Track }) {
  const queryClient = useQueryClient();
  const { data: taxonomy } = useTaxonomy(track);
  const leaves = useMemo(() => leafCategories(taxonomy?.clientNeeds ?? []), [taxonomy]);
  const [clientNeedsId, setClientNeedsId] = useState("");
  const [name, setName] = useState("");
  const mutation = useMutation({
    mutationFn: () => createSubtopic({ clientNeedsId: Number(clientNeedsId), name: name.trim(), sortOrder: 0 }),
    onSuccess() {
      setName("");
      void queryClient.invalidateQueries({ queryKey: ["admin", "subtopics"] });
    },
  });

  function submit(event: FormEvent) {
    event.preventDefault();
    mutation.mutate();
  }

  return (
    <form className={styles.filters} onSubmit={submit} aria-label="Add a subtopic">
      <label>
        Client Needs subcategory
        <select value={clientNeedsId} onChange={(event) => setClientNeedsId(event.target.value)} required>
          <option value="">Choose…</option>
          {leaves.map((leaf) => (
            <option key={leaf.id} value={leaf.id}>
              {leaf.label}
            </option>
          ))}
        </select>
      </label>
      <label>
        Name
        <input value={name} maxLength={120} onChange={(event) => setName(event.target.value)} required />
      </label>
      <button type="submit" className={ui.button} disabled={mutation.isPending || clientNeedsId === "" || name.trim() === ""}>
        Add subtopic
      </button>
      {mutation.isError && <p role="alert">{problem(mutation.error)}</p>}
    </form>
  );
}

function SubtopicRow({ subtopic }: { subtopic: SubtopicView }) {
  const queryClient = useQueryClient();
  const [name, setName] = useState(subtopic.name);
  const mutation = useMutation({
    mutationFn: (body: UpdateSubtopicRequest) => updateSubtopic(subtopic.id, body),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["admin", "subtopics"] }),
  });
  const trimmed = name.trim();

  return (
    <tr>
      <td>
        <input aria-label={`Name of ${subtopic.name}`} value={name} maxLength={120} onChange={(event) => setName(event.target.value)} />
        {mutation.isError && <p role="alert">{problem(mutation.error)}</p>}
      </td>
      <td>{subtopic.clientNeedsName}</td>
      <td>{toNumber(subtopic.questionCount)}</td>
      <td>{subtopic.isActive ? "Active" : "Inactive"}</td>
      <td>
        <button
          type="button"
          className={ui.buttonSecondary}
          disabled={mutation.isPending || trimmed === "" || trimmed === subtopic.name}
          onClick={() => mutation.mutate({ name: trimmed, sortOrder: null, isActive: null })}
        >
          Rename
        </button>{" "}
        <button
          type="button"
          className={ui.buttonGhost}
          disabled={mutation.isPending}
          aria-label={`${subtopic.isActive ? "Deactivate" : "Activate"} ${subtopic.name}`}
          onClick={() => mutation.mutate({ name: null, sortOrder: null, isActive: !subtopic.isActive })}
        >
          {subtopic.isActive ? "Deactivate" : "Activate"}
        </button>
      </td>
    </tr>
  );
}
