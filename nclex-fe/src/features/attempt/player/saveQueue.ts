import { createStore, type StoreApi } from "zustand/vanilla";
import { isApiError } from "@/lib/api/errors";

export type SaveStatus = "idle" | "saving" | "saved" | "offline" | "error";

/** One answer waiting to reach the server. */
export interface PendingWrite {
  response: unknown;
  clientSeq: number;
}

export interface SaveQueueState {
  /** Sent as X-Attempt-Client; a fresh one per page load (architecture.md 5.2). */
  clientId: string;
  status: SaveStatus;
  /** At most one write per position: a newer answer replaces an unsent older one. */
  pending: Record<number, PendingWrite>;
  /** Another device took the lease; writes stop until the player opens the attempt again. */
  leaseLost: boolean;
  /** Queues an answer; it is sent after a short pause in typing or clicking. */
  enqueue(position: number, response: unknown): void;
  /** Sends everything pending now. Call before navigating, and when the page is hidden. */
  flush(options?: { keepalive?: boolean }): Promise<void>;
  /** The server's clientSeq for an item, so the next write is numbered above it. */
  noteServerSeq(position: number, clientSeq: number): void;
  /** Another request (navigate, mark) found the lease gone: stop writing until it is taken back. */
  markLeaseLost(): void;
  /** Call after the lease is taken back ("Continue here"). */
  leaseRestored(): void;
  /** Stops timers when the player unmounts. */
  dispose(): void;
}

export type SendWrite = (position: number, write: PendingWrite, options: { keepalive: boolean }) => Promise<void>;

export interface SaveQueueOptions {
  clientId: string;
  /** Sends one write; throws ApiError for an error response and anything else for a network failure. */
  send: SendWrite;
  /** Where pending writes are mirrored; null when storage is unavailable (private mode, tests). */
  storage: Storage | null;
  storageKey: string;
  debounceMs?: number;
  firstRetryMs?: number;
  maxRetryMs?: number;
}

/**
 * The player's client-side autosave queue (interfaces.md 5.4, architecture.md 5.2):
 * - debounced 750 ms and coalesced per position, so clicking through options sends one write;
 * - every write carries a rising clientSeq, so the server can ignore late retries;
 * - on a network failure it keeps the writes, mirrors them to localStorage, and retries with
 *   exponential backoff capped at 5 s.
 *
 * Only answers are stored on the device, never tokens.
 */
export function createSaveQueue(options: SaveQueueOptions): StoreApi<SaveQueueState> {
  const { send, storage, storageKey, debounceMs = 750, firstRetryMs = 500, maxRetryMs = 5000 } = options;

  // Writes an earlier page load could not send (offline, tab closed) are picked up straight away,
  // so a new answer can never overwrite them in storage. They are sent on the first flush, after
  // the player has opened the attempt.
  const restored = readSaved(storage, storageKey);
  let lastSeq = Math.max(0, ...Object.values(restored).map((write) => write.clientSeq));
  let debounceTimer: ReturnType<typeof setTimeout> | undefined;
  let retryTimer: ReturnType<typeof setTimeout> | undefined;
  let retryDelay = firstRetryMs;
  let inFlight: Promise<void> | null = null;

  // One counter for the page load, kept above every clientSeq the server has reported, so a new
  // device's first write to an item is never mistaken for an old one.
  function nextSeq() {
    lastSeq += 1;
    return lastSeq;
  }

  function persist(pending: Record<number, PendingWrite>) {
    try {
      if (Object.keys(pending).length === 0) storage?.removeItem(storageKey);
      else storage?.setItem(storageKey, JSON.stringify(pending));
    } catch {
      // Storage full or blocked: the writes are still in memory and will be sent.
    }
  }

  function clearTimers() {
    clearTimeout(debounceTimer);
    clearTimeout(retryTimer);
    debounceTimer = undefined;
    retryTimer = undefined;
  }

  const store = createStore<SaveQueueState>()((set, get) => {
    function removeIfUnchanged(position: number, sentSeq: number) {
      const current = get().pending[position];
      if (current?.clientSeq !== sentSeq) return; // a newer answer arrived while this one was in flight
      const rest = { ...get().pending };
      delete rest[position];
      set({ pending: rest });
      persist(rest);
    }

    function scheduleRetry() {
      clearTimeout(retryTimer);
      retryTimer = setTimeout(() => void get().flush(), retryDelay);
      retryDelay = Math.min(retryDelay * 2, maxRetryMs);
    }

    async function sendAll(keepalive: boolean) {
      const writes = Object.entries(get().pending).map(([position, write]) => [Number(position), write] as const);
      if (writes.length === 0) return;

      set({ status: "saving" });
      for (const [position, write] of writes) {
        try {
          await send(position, write, { keepalive });
          removeIfUnchanged(position, write.clientSeq);
        } catch (error) {
          if (isApiError(error, "ATTEMPT_OPENED_ELSEWHERE")) {
            // Keep the writes: after "Continue here" they are sent from this device.
            set({ status: "error", leaseLost: true });
            return;
          }
          if (isApiError(error)) {
            // The server refused this answer (item locked, attempt no longer active...): retrying
            // would get the same answer, so drop it.
            removeIfUnchanged(position, write.clientSeq);
            set({ status: "error" });
            continue;
          }
          set({ status: "offline" });
          scheduleRetry();
          return;
        }
      }

      retryDelay = firstRetryMs;
      if (get().status === "saving") set({ status: "saved" });
    }

    return {
      clientId: options.clientId,
      status: "idle",
      pending: restored,
      leaseLost: false,

      enqueue(position, response) {
        const pending = { ...get().pending, [position]: { response, clientSeq: nextSeq() } };
        set({ pending });
        persist(pending);
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => void get().flush(), debounceMs);
      },

      async flush({ keepalive = false } = {}) {
        clearTimers();
        if (get().leaseLost) return;
        // One send at a time keeps writes to the same item in order.
        while (inFlight) await inFlight;
        inFlight = sendAll(keepalive);
        try {
          await inFlight;
        } finally {
          inFlight = null;
        }
      },

      noteServerSeq(_position, clientSeq) {
        lastSeq = Math.max(lastSeq, clientSeq);
      },

      markLeaseLost() {
        clearTimers();
        set({ leaseLost: true });
      },

      leaseRestored() {
        set({ leaseLost: false, status: "idle" });
      },

      dispose() {
        clearTimers();
      },
    };
  });

  return store;
}

function readSaved(storage: Storage | null, storageKey: string): Record<number, PendingWrite> {
  try {
    const saved: unknown = JSON.parse(storage?.getItem(storageKey) ?? "{}");
    return typeof saved === "object" && saved !== null ? (saved as Record<number, PendingWrite>) : {};
  } catch {
    return {};
  }
}

/** The localStorage key holding an attempt's unsent answers. */
export function pendingStorageKey(attemptId: string) {
  return `nclex.attempt.${attemptId}.pending`;
}

/** A fresh id per page load, sent as X-Attempt-Client. */
export function newClientId() {
  return `c-${crypto.randomUUID()}`;
}

/** localStorage when the browser allows it; null otherwise. */
export function browserStorage(): Storage | null {
  try {
    return typeof window === "undefined" ? null : window.localStorage;
  } catch {
    return null;
  }
}
