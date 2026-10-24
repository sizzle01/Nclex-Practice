import { apiClient } from "@/lib/api/client";
import { clearSession, setSession, useSessionStore } from "./sessionStore";

const refreshLockName = "nclex-auth-refresh";
let inFlight: Promise<boolean> | null = null;

/**
 * Asks the API for a new access token using the HttpOnly refresh cookie (POST /auth/refresh).
 * Resolves true when signed in, false when there is no valid session.
 *
 * Refresh tokens are single-use: the API revokes a whole session if one is presented twice
 * (architecture.md section 6). So concurrent callers in this tab share one request, and a Web Lock
 * makes other tabs wait their turn; by then the browser holds the new cookie, so they never
 * present the old one.
 */
export function refreshSession(): Promise<boolean> {
  inFlight ??= runExclusively(requestNewAccessToken).finally(() => {
    inFlight = null;
  });
  return inFlight;
}

async function requestNewAccessToken(): Promise<boolean> {
  try {
    const { data } = await apiClient.POST("/api/v1/auth/refresh", {
      params: { header: { "X-Requested-With": "fetch" } },
    });
    if (data) {
      setSession(data.accessToken, data.accessTokenExpiresAt);
      return true;
    }
    clearSession();
    return false;
  } catch {
    // The API could not be reached. On first load that means "not signed in" for now; later on,
    // keep the current token: it may still be valid, and the next request will try again.
    if (useSessionStore.getState().status === "unknown") clearSession();
    return false;
  }
}

async function runExclusively(task: () => Promise<boolean>): Promise<boolean> {
  if (typeof navigator !== "undefined" && navigator.locks) {
    return await navigator.locks.request(refreshLockName, task);
  }
  return task();
}
