"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { createContext, type ReactNode, use, useEffect, useMemo } from "react";
import { type AuthResponse, fetchMe, logout, logoutAll } from "./api/authApi";
import { installAuthMiddleware } from "./session/installAuthMiddleware";
import { refreshSession } from "./session/refreshSession";
import { clearSession, type SessionStatus, setSession, useSessionStore } from "./session/sessionStore";

// Refresh this long before the access token expires, so requests never race its expiry.
const refreshMarginMs = 60_000;
const minimumRefreshDelayMs = 5_000;

export const meQueryKey = ["me"] as const;

installAuthMiddleware();

export interface AuthContextValue {
  status: SessionStatus;
  /** Call with the body of a successful signup or login. */
  signIn: (response: AuthResponse) => void;
  /** Log out on this device. */
  signOut: () => Promise<void>;
  /** Log out on every device. */
  signOutEverywhere: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

/**
 * Keeps the user signed in across page loads: on load it trades the refresh cookie for an access
 * token, and it refreshes again shortly before each token expires (interfaces.md 5.1).
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const status = useSessionStore((s) => s.status);
  const expiresAt = useSessionStore((s) => s.expiresAt);

  useEffect(() => {
    if (useSessionStore.getState().status === "unknown") void refreshSession();
  }, []);

  useEffect(() => {
    if (expiresAt === null) return;
    const delay = Math.max(expiresAt - Date.now() - refreshMarginMs, minimumRefreshDelayMs);
    const timer = setTimeout(() => void refreshSession(), delay);
    return () => clearTimeout(timer);
  }, [expiresAt]);

  useEffect(() => {
    // Signed out (or the session was revoked elsewhere): drop every cached response, which may be
    // the previous user's data.
    if (status === "anonymous") queryClient.clear();
  }, [status, queryClient]);

  const value = useMemo<AuthContextValue>(() => {
    async function endSession(callApi: () => Promise<void>) {
      try {
        await callApi();
      } finally {
        clearSession();
      }
    }

    return {
      status,
      signIn(response) {
        queryClient.setQueryData(meQueryKey, response.user);
        setSession(response.accessToken, response.accessTokenExpiresAt);
      },
      signOut: () => endSession(logout),
      signOutEverywhere: () => endSession(logoutAll),
    };
  }, [status, queryClient]);

  return <AuthContext value={value}>{children}</AuthContext>;
}

export function useAuth(): AuthContextValue {
  const context = use(AuthContext);
  if (!context) throw new Error("useAuth must be used inside <AuthProvider>.");
  return context;
}

/** The signed-in user's profile (GET /me), cached by TanStack Query. */
export function useMe() {
  const { status } = useAuth();
  return useQuery({ queryKey: meQueryKey, queryFn: fetchMe, enabled: status === "authenticated" });
}
