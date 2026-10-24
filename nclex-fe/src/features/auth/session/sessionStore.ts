import { create } from "zustand";

/**
 * - `unknown`: the app has just loaded and has not yet asked the API whether a session exists.
 * - `authenticated`: we hold a valid access token.
 * - `anonymous`: no session (never signed in, logged out, or the refresh token was rejected).
 */
export type SessionStatus = "unknown" | "authenticated" | "anonymous";

interface SessionState {
  status: SessionStatus;
  /** The access token lives only here, in memory: never in localStorage (interfaces.md 5.1). */
  accessToken: string | null;
  /** When the access token expires, in epoch milliseconds. */
  expiresAt: number | null;
}

/**
 * The current session. A Zustand store rather than React state because non-React code (the API
 * client middleware and the refresh routine) must read and update it too.
 */
export const useSessionStore = create<SessionState>()(() => ({
  status: "unknown",
  accessToken: null,
  expiresAt: null,
}));

export function setSession(accessToken: string, accessTokenExpiresAt: string) {
  useSessionStore.setState({
    status: "authenticated",
    accessToken,
    expiresAt: Date.parse(accessTokenExpiresAt),
  });
}

export function clearSession() {
  useSessionStore.setState({ status: "anonymous", accessToken: null, expiresAt: null });
}

export function getAccessToken(): string | null {
  return useSessionStore.getState().accessToken;
}
