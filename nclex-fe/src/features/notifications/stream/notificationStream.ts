import { fetchStreamTicket, type NotificationView, streamUrl } from "../api/notificationsApi";

export interface StreamHandlers {
  onNotification(notification: NotificationView): void;
  onUnreadCount(count: number): void;
  /** True once events flow, false whenever the stream is down (the caller then polls). */
  onConnectedChange(connected: boolean): void;
}

/** Reconnect delays: 1 s, doubling up to 30 s, so an outage does not flood the API. */
export function reconnectDelayMs(failures: number): number {
  return Math.min(30_000, 1_000 * 2 ** Math.max(0, failures - 1));
}

/**
 * Opens the notification SSE stream (api.md section 6): gets a ticket, opens an EventSource and
 * reconnects with backoff whenever it drops or the ticket request fails. A ticket works once, so
 * every reconnect asks for a new one. Returns a function that closes the stream for good.
 */
export function connectNotificationStream(handlers: StreamHandlers): () => void {
  let source: EventSource | null = null;
  let retryTimer: ReturnType<typeof setTimeout> | null = null;
  let failures = 0;
  let closed = false;

  function scheduleReconnect() {
    handlers.onConnectedChange(false);
    if (closed) return;
    failures += 1;
    retryTimer = setTimeout(open, reconnectDelayMs(failures));
  }

  async function open() {
    retryTimer = null;
    let ticket: string;
    try {
      ticket = (await fetchStreamTicket()).ticket;
    } catch {
      scheduleReconnect();
      return;
    }
    if (closed) return;

    source = new EventSource(streamUrl(ticket));
    source.addEventListener("unread_count", (event) => {
      failures = 0;
      handlers.onConnectedChange(true);
      handlers.onUnreadCount(parse<{ count: number }>(event).count);
    });
    source.addEventListener("notification", (event) => handlers.onNotification(parse<NotificationView>(event)));
    source.onerror = () => {
      // EventSource would retry with the same, now used, ticket; start over with a new one instead.
      source?.close();
      source = null;
      scheduleReconnect();
    };
  }

  void open();

  return () => {
    closed = true;
    if (retryTimer) clearTimeout(retryTimer);
    source?.close();
    source = null;
  };
}

function parse<T>(event: Event): T {
  return JSON.parse((event as MessageEvent<string>).data) as T;
}
