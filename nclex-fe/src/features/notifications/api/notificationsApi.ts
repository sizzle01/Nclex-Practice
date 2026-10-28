import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";
import { toNumber } from "@/lib/format/toNumber";

export type NotificationView = ApiSchema<"NotificationView">;
export type NotificationPage = ApiSchema<"NotificationPage">;
export type NotificationCategory = ApiSchema<"NotificationCategory">;
export type PreferenceView = ApiSchema<"PreferenceView">;
export type NotificationPreferences = ApiSchema<"NotificationPreferencesResponse">;
export type UpdatePreferencesRequest = ApiSchema<"UpdatePreferencesRequest">;
export type StreamTicket = ApiSchema<"StreamTicketResponse">;

/** GET /notifications: one page of the feed, newest first. */
export async function fetchNotifications(options: {
  cursor: string | null;
  unreadOnly: boolean;
  limit: number;
}): Promise<NotificationPage> {
  const { data, error, response } = await apiClient.GET("/api/v1/notifications", {
    params: { query: { cursor: options.cursor ?? undefined, unreadOnly: options.unreadOnly, limit: options.limit } },
  });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** GET /notifications/unread-count: the bell's badge. */
export async function fetchUnreadCount(): Promise<number> {
  const { data, error, response } = await apiClient.GET("/api/v1/notifications/unread-count");
  if (data) return toNumber(data.count);
  throw ApiError.from(response, error);
}

/** POST /notifications/{id}/read. Reading one twice is harmless. */
export async function markNotificationRead(id: string): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/notifications/{id}/read", { params: { path: { id } } });
  if (!response.ok) throw ApiError.from(response, error);
}

/** POST /notifications/read-all. */
export async function markAllNotificationsRead(): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/notifications/read-all");
  if (!response.ok) throw ApiError.from(response, error);
}

/**
 * POST /notifications/stream-ticket: a 30-second, single-use ticket for the SSE stream, because
 * EventSource cannot send the Authorization header. Fails with 503 when live updates are down.
 */
export async function fetchStreamTicket(): Promise<StreamTicket> {
  const { data, error, response } = await apiClient.POST("/api/v1/notifications/stream-ticket");
  if (data) return data;
  throw ApiError.from(response, error);
}

/** The stream's URL for a ticket. Not in the generated client: EventSource opens it directly. */
export function streamUrl(ticket: string): string {
  return `/api/v1/notifications/stream?ticket=${encodeURIComponent(ticket)}`;
}

/** GET /me/notification-preferences. */
export async function fetchPreferences(): Promise<NotificationPreferences> {
  const { data, error, response } = await apiClient.GET("/api/v1/me/notification-preferences");
  if (data) return data;
  throw ApiError.from(response, error);
}

/** PUT /me/notification-preferences: every category, each once. Security must stay on. */
export async function replacePreferences(body: UpdatePreferencesRequest): Promise<NotificationPreferences> {
  const { data, error, response } = await apiClient.PUT("/api/v1/me/notification-preferences", { body });
  if (data) return data;
  throw ApiError.from(response, error);
}
