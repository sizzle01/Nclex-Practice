"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useStore } from "zustand";
import { isApiError } from "@/lib/api/errors";
import { toNumber } from "@/lib/format/toNumber";
import {
  attemptQueryKey,
  type AttemptState,
  type CandidateItem,
  fetchAttempt,
  fetchItem,
  itemQueryKey,
  navigateTo,
  openAttempt,
  saveResponse,
  setFlagged,
} from "../api/attemptApi";
import { browserStorage, createSaveQueue, newClientId, pendingStorageKey } from "./saveQueue";

export type OpenState = "opening" | "open" | "failed";

/**
 * Everything the player needs for one attempt: opening it with a fresh client id (the device
 * lease), the current item, autosave through the save queue, navigation and marks
 * (interfaces.md 5.4, PlayerController). Components only render what this returns.
 */
export function useAttemptSession(attemptId: string) {
  const queryClient = useQueryClient();
  const [clientId] = useState(newClientId);
  const [queue] = useState(() =>
    createSaveQueue({
      clientId,
      send: (position, write, { keepalive }) => saveResponse(attemptId, clientId, position, write, keepalive),
      storage: browserStorage(),
      storageKey: pendingStorageKey(attemptId),
    }),
  );
  const saveStatus = useStore(queue, (s) => s.status);
  const leaseLost = useStore(queue, (s) => s.leaseLost);
  const pending = useStore(queue, (s) => s.pending);

  const [navError, setNavError] = useState<string | null>(null);
  const [navigating, setNavigating] = useState(false);

  // Claims the lease (architecture.md 5.2). Runs when the player mounts and again on "Continue here".
  const openMutation = useMutation({
    mutationFn: () => openAttempt(attemptId, clientId),
    onSuccess: async (state) => {
      queryClient.setQueryData(attemptQueryKey(attemptId), state);
      queue.getState().leaseRestored();
      // Answers left on this device by an earlier page load (offline, tab closed) go out now.
      await queue.getState().flush();
      await queryClient.invalidateQueries({ queryKey: ["attempts", attemptId, "items"] });
    },
  });
  const { mutate: open } = openMutation;

  useEffect(() => {
    open();
  }, [open]);

  const attemptKey = attemptQueryKey(attemptId);
  // Filled by the open call first, so it only fetches again when invalidated.
  const attemptQuery = useQuery({
    queryKey: attemptKey,
    queryFn: () => fetchAttempt(attemptId),
    enabled: openMutation.isSuccess,
  });
  const attempt = openMutation.isSuccess || openMutation.isPending ? attemptQuery.data : undefined;
  const openState: OpenState = attempt ? "open" : openMutation.isError ? "failed" : "opening";
  const position = attempt ? toNumber(attempt.currentPosition) : null;
  const total = attempt ? toNumber(attempt.totalItems) : 0;

  const itemQuery = useQuery({
    queryKey: itemQueryKey(attemptId, position ?? 0),
    queryFn: () => fetchItem(attemptId, position!),
    enabled: openMutation.isSuccess && position !== null,
  });
  const item = itemQuery.data;

  // Number new writes above anything the server already holds for this item.
  useEffect(() => {
    if (item) queue.getState().noteServerSeq(toNumber(item.position), toNumber(item.clientSeq));
  }, [item, queue]);

  // Prefetch the next item so moving on stays within the 1-second target (interfaces.md 5.4).
  useEffect(() => {
    if (item && position !== null && position < total) {
      void queryClient.prefetchQuery({
        queryKey: itemQueryKey(attemptId, position + 1),
        queryFn: () => fetchItem(attemptId, position + 1),
      });
    }
  }, [attemptId, item, position, queryClient, total]);

  // Send unsaved answers when the tab is hidden or closed, or the network comes back.
  useEffect(() => {
    const store = queue.getState();
    const onVisibility = () => {
      if (document.visibilityState === "hidden") void store.flush();
    };
    const onPageHide = () => void store.flush({ keepalive: true });
    const onOnline = () => void store.flush();
    document.addEventListener("visibilitychange", onVisibility);
    window.addEventListener("pagehide", onPageHide);
    window.addEventListener("online", onOnline);
    return () => {
      document.removeEventListener("visibilitychange", onVisibility);
      window.removeEventListener("pagehide", onPageHide);
      window.removeEventListener("online", onOnline);
      store.dispose();
    };
  }, [queue]);

  function updateAttempt(update: (state: AttemptState) => AttemptState) {
    queryClient.setQueryData<AttemptState>(attemptKey, (state) => (state ? update(state) : state));
  }

  function updateItem(itemPosition: number, update: (current: CandidateItem) => CandidateItem) {
    queryClient.setQueryData<CandidateItem>(itemQueryKey(attemptId, itemPosition), (current) =>
      current ? update(current) : current,
    );
  }

  function handleWriteError(error: unknown) {
    if (isApiError(error, "ATTEMPT_OPENED_ELSEWHERE")) queue.getState().markLeaseLost();
  }

  /** The answer to show: an unsent one from the queue, else the server's. */
  const response = position !== null && pending[position] ? pending[position].response : (item?.response ?? null);

  function answer(next: unknown) {
    if (position === null) return;
    queue.getState().enqueue(position, next);
    const state = next === null ? "seen" : "answered";
    updateItem(position, (current) => ({ ...current, response: next, state }));
    updateAttempt((current) => ({
      ...current,
      navigator: current.navigator.map((entry) =>
        toNumber(entry.position) === position ? { ...entry, state } : entry,
      ),
    }));
  }

  async function goTo(target: number) {
    if (navigating || position === null || target === position) return;
    setNavigating(true);
    setNavError(null);
    try {
      await queue.getState().flush();
      if (queue.getState().leaseLost) return;
      const result = await navigateTo(attemptId, clientId, target);
      updateAttempt((current) => ({ ...current, currentPosition: result.currentPosition }));
      // The item left may now be locked, and the navigator and progress have changed.
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: attemptKey }),
        queryClient.invalidateQueries({ queryKey: itemQueryKey(attemptId, position) }),
      ]);
    } catch (error) {
      handleWriteError(error);
      if (isApiError(error, "BACK_NAVIGATION_DISABLED")) setNavError("This test doesn't allow going back.");
      else if (!isApiError(error, "ATTEMPT_OPENED_ELSEWHERE")) setNavError("Could not move to that question. Please try again.");
    } finally {
      setNavigating(false);
    }
  }

  async function toggleFlag() {
    if (!item || position === null) return;
    const flagged = !item.flagged;
    try {
      await setFlagged(attemptId, clientId, position, flagged);
      updateItem(position, (current) => ({ ...current, flagged }));
      updateAttempt((current) => ({
        ...current,
        navigator: current.navigator.map((entry) =>
          toNumber(entry.position) === position ? { ...entry, flagged } : entry,
        ),
      }));
    } catch (error) {
      handleWriteError(error);
    }
  }

  function continueHere() {
    open();
  }

  return {
    openState,
    openError: openMutation.error,
    attempt,
    attemptError: attemptQuery.error,
    item,
    itemError: itemQuery.error,
    position,
    total,
    response,
    saveStatus,
    leaseLost,
    navError,
    busy: navigating || openMutation.isPending,
    answer,
    goTo,
    toggleFlag,
    continueHere,
  };
}
