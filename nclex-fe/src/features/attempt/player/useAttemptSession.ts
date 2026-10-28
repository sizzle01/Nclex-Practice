"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
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
  type PauseReason,
  pauseAttempt,
  resumeAttempt,
  saveResponse,
  setFlagged,
  submitAttempt as postSubmit,
  submitItem as postSubmitItem,
} from "../api/attemptApi";
import { createAttemptClock, remainingMs } from "./attemptClock";
import { browserStorage, createSaveQueue, newClientId, pendingStorageKey } from "./saveQueue";

/** The server accepts writes this long after the deadline (architecture.md 5.1); the timeout check waits it out. */
const serverGraceMs = 2_500;

/** "ended": the attempt is submitted, by the candidate or because the time ran out. */
export type OpenState = "opening" | "open" | "failed" | "ended";

/**
 * Everything the player needs for one attempt: opening it with a fresh client id (the device
 * lease), the current item, autosave through the save queue, navigation and marks, the
 * server-synced timers, pause and resume, tutor-mode item submits and the final submit
 * (interfaces.md 5.4, PlayerController). Components only render what this returns.
 */
export function useAttemptSession(attemptId: string) {
  const queryClient = useQueryClient();
  const router = useRouter();
  const [clientId] = useState(newClientId);
  // Set once any call reports ATTEMPT_TIME_EXPIRED: the server has finalized the attempt.
  const [timeExpired, setTimeExpired] = useState(false);
  const [queue] = useState(() =>
    createSaveQueue({
      clientId,
      send: async (position, write, { keepalive }) => {
        try {
          await saveResponse(attemptId, clientId, position, write, keepalive);
        } catch (error) {
          if (isApiError(error, "ATTEMPT_TIME_EXPIRED")) setTimeExpired(true);
          throw error;
        }
      },
      storage: browserStorage(),
      storageKey: pendingStorageKey(attemptId),
    }),
  );
  const [clock] = useState(() => createAttemptClock());
  const offsetMs = useStore(clock, (s) => s.offsetMs);
  const saveStatus = useStore(queue, (s) => s.status);
  const leaseLost = useStore(queue, (s) => s.leaseLost);
  const pending = useStore(queue, (s) => s.pending);

  const [navError, setNavError] = useState<string | null>(null);
  const [navigating, setNavigating] = useState(false);
  const [pauseError, setPauseError] = useState<string | null>(null);
  const [pausing, setPausing] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

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
  const openExpired = isApiError(openMutation.error, "ATTEMPT_TIME_EXPIRED");
  const [submitted, setSubmitted] = useState(false);
  const ended = submitted || timeExpired || openExpired || attempt?.status === "submitted";
  const openState: OpenState = ended ? "ended" : attempt ? "open" : openMutation.isError ? "failed" : "opening";
  const position = attempt ? toNumber(attempt.currentPosition) : null;
  const total = attempt ? toNumber(attempt.totalItems) : 0;
  const paused = attempt?.status === "paused";

  // A finished attempt belongs on its results page (architecture.md 5.5).
  const resultsPath = `/attempts/${attemptId}/results`;
  useEffect(() => {
    if (ended) router.push(resultsPath);
  }, [ended]); // eslint-disable-line react-hooks/exhaustive-deps

    // Every attempt response carries serverNow; the clock keeps the offset (architecture.md 5.1).
  const serverNow = attempt?.timer.serverNow;
  useEffect(() => {
    if (serverNow) clock.getState().sync(serverNow);
  }, [clock, serverNow]);

  const examTimer = attempt?.timer.exam ?? null;
  const itemTimer = attempt?.timer.item ?? null;
  const ticking = !ended && !paused && Boolean(examTimer?.running || itemTimer?.running);
  const now = useNow(ticking);
  const examMs = examTimer ? remainingMs(examTimer.deadlineAt, toNumber(examTimer.remainingMs), offsetMs, now) : null;
  const itemMs = itemTimer ? remainingMs(itemTimer.deadlineAt, toNumber(itemTimer.remainingMs), offsetMs, now) : null;
  const examTimeUp = examTimer?.running === true && examMs === 0;
  const itemTimeUp = itemTimer?.running === true && itemMs === 0;

  // Paused content is hidden, and the server refuses it with 423 ATTEMPT_PAUSED (PRD 5.4).
  const itemQuery = useQuery({
    queryKey: itemQueryKey(attemptId, position ?? 0),
    queryFn: () => fetchItem(attemptId, position!),
    enabled: openMutation.isSuccess && position !== null && !paused && !ended,
  });
  const item = itemQuery.data;

  // Number new writes above anything the server already holds for this item.
  useEffect(() => {
    if (item) queue.getState().noteServerSeq(toNumber(item.position), toNumber(item.clientSeq));
  }, [item, queue]);

  // Prefetch the next item so moving on stays within the 1-second target (interfaces.md 5.4).
  useEffect(() => {
    if (item && !paused && position !== null && position < total) {
      void queryClient.prefetchQuery({
        queryKey: itemQueryKey(attemptId, position + 1),
        queryFn: () => fetchItem(attemptId, position + 1),
      });
    }
  }, [attemptId, item, paused, position, queryClient, total]);

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
    if (isApiError(error, "ATTEMPT_TIME_EXPIRED")) setTimeExpired(true);
  }

  // At zero the input is already disabled. Saves sent in the last moment still land within the
  // server's grace, so wait that out, then submit (interfaces.md 5.4). If the server has already
  // finalized the attempt it answers ATTEMPT_TIME_EXPIRED, and either way the player goes to results.
  const timeoutCheck = useRef(false);
  useEffect(() => {
    if (!examTimeUp || timeoutCheck.current || position === null) return;
    timeoutCheck.current = true;
    const timer = setTimeout(async () => {
      try {
        await submitAttempt();
      } finally {
        timeoutCheck.current = false;
      }
    }, serverGraceMs);
    return () => clearTimeout(timer);
  }, [examTimeUp]); // eslint-disable-line react-hooks/exhaustive-deps

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

  /** A break shows the overlay; Save & Exit leaves for the dashboard. Both freeze the timers. */
  async function pause(reason: PauseReason) {
    if (pausing) return;
    setPausing(true);
    setPauseError(null);
    try {
      await queue.getState().flush();
      if (queue.getState().leaseLost) return;
      const result = await pauseAttempt(attemptId, clientId, reason);
      updateAttempt((current) => ({ ...current, status: result.status, timer: result.timer }));
      if (reason === "save_exit") router.push("/dashboard");
    } catch (error) {
      handleWriteError(error);
      if (isApiError(error, "PAUSE_LIMIT_REACHED")) setPauseError("You have used all the pauses this test allows.");
      else if (!isApiError(error, "ATTEMPT_OPENED_ELSEWHERE") && !isApiError(error, "ATTEMPT_TIME_EXPIRED"))
        setPauseError("Could not pause. Please try again.");
    } finally {
      setPausing(false);
    }
  }

  async function resume() {
    if (pausing) return;
    setPausing(true);
    setPauseError(null);
    try {
      const state = await resumeAttempt(attemptId, clientId);
      queryClient.setQueryData(attemptKey, state);
      queue.getState().leaseRestored();
      await queryClient.invalidateQueries({ queryKey: ["attempts", attemptId, "items"] });
    } catch (error) {
      handleWriteError(error);
      if (!isApiError(error, "ATTEMPT_TIME_EXPIRED")) setPauseError("Could not resume. Please try again.");
    } finally {
      setPausing(false);
    }
  }

  function continueHere() {
    open();
  }

  /**
   * Tutor mode's Submit on the current item: send its answer, then lock and grade it. The reveal
   * comes back with the call, so the rationale shows without another request.
   */
  async function submitItem() {
    if (submitting || position === null) return;
    setSubmitting(true);
    setSubmitError(null);
    try {
      await queue.getState().flush();
      if (queue.getState().leaseLost) return;
      const reveal = await postSubmitItem(attemptId, clientId, position);
      updateItem(position, (current) => ({ ...current, state: "locked", reveal }));
      // Only the navigator and progress changed; the item already holds its reveal, so it is not refetched.
      await queryClient.invalidateQueries({ queryKey: attemptKey, exact: true });
    } catch (error) {
      handleWriteError(error);
      if (!isApiError(error, "ATTEMPT_OPENED_ELSEWHERE") && !isApiError(error, "ATTEMPT_TIME_EXPIRED"))
        setSubmitError("Could not submit this answer. Please try again.");
    } finally {
      setSubmitting(false);
    }
  }

  /**
   * The final Submit (interfaces.md 5.4): flush the save queue, send whatever it still holds (for
   * example while offline) as final responses, then go to the results page.
   */
  async function submitAttempt() {
    if (submitting) return;
    setSubmitting(true);
    setSubmitError(null);
    try {
      await queue.getState().flush();
      if (queue.getState().leaseLost) return;
      const finalResponses = Object.entries(queue.getState().pending).map(([itemPosition, write]) => ({
        position: Number(itemPosition),
        response: write.response ?? null,
        clientSeq: write.clientSeq,
      }));
      await postSubmit(attemptId, clientId, finalResponses);
      queue.getState().clear();
      setSubmitted(true);
    } catch (error) {
      handleWriteError(error);
      if (isApiError(error, "ATTEMPT_TIME_EXPIRED")) queue.getState().clear();
      else if (!isApiError(error, "ATTEMPT_OPENED_ELSEWHERE")) setSubmitError("Could not submit the test. Please try again.");
    } finally {
      setSubmitting(false);
    }
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
    busy: navigating || pausing || submitting || openMutation.isPending,
    submitting,
    submitError,
    /** True when the attempt ended because its time ran out, not by the candidate's Submit. */
    endedByTime: timeExpired || openExpired,
    resultsPath,
    paused,
    pauseError,
    examMs,
    itemMs,
    examTimeUp,
    itemTimeUp,
    answer,
    goTo,
    toggleFlag,
    pause,
    resume,
    continueHere,
    submitItem,
    submitAttempt,
  };
}

/** Date.now(), refreshed every second while `active`, so the countdowns re-render. */
function useNow(active: boolean) {
  const [now, setNow] = useState(() => Date.now());
  useEffect(() => {
    if (!active) return;
    const tick = () => setNow(Date.now());
    tick();
    const interval = setInterval(tick, 1000);
    return () => clearInterval(interval);
  }, [active]);
  return now;
}
