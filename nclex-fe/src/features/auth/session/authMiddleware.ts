import type { Middleware } from "openapi-fetch";
import { refreshSession } from "./refreshSession";
import { getAccessToken } from "./sessionStore";

// Auth endpoints authenticate with the refresh cookie, not the access token, and must never
// trigger a refresh themselves (that would loop). logout-all is the one that needs the token.
function usesAccessToken(schemaPath: string) {
  return !schemaPath.startsWith("/api/v1/auth/") || schemaPath === "/api/v1/auth/logout-all";
}

// Copies of outgoing requests, kept until the response arrives, so a request can be sent again
// after a refresh (a Request body can only be read once).
const retryCopies = new Map<string, Request>();

/**
 * Adds `Authorization: Bearer <token>` to API calls. If one comes back 401 (the token expired,
 * for example after the laptop slept), refreshes the session once and repeats the request.
 */
export const authMiddleware: Middleware = {
  onRequest({ request, schemaPath, id }) {
    if (!usesAccessToken(schemaPath)) return request;

    const token = getAccessToken();
    if (token) request.headers.set("Authorization", `Bearer ${token}`);
    retryCopies.set(id, request.clone());
    return request;
  },

  async onResponse({ response, id }) {
    const retry = retryCopies.get(id);
    retryCopies.delete(id);
    if (response.status !== 401 || !retry) return response;

    if (!(await refreshSession())) return response;
    retry.headers.set("Authorization", `Bearer ${getAccessToken()}`);
    return fetch(retry);
  },

  onError({ id }) {
    retryCopies.delete(id);
  },
};
