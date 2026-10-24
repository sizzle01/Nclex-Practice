import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import { refreshSession } from "./refreshSession";
import { useSessionStore } from "./sessionStore";

vi.mock("@/lib/api/client", () => ({ apiClient: { POST: vi.fn() } }));
const postMock = vi.mocked(apiClient.POST);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return { data: ok ? body : undefined, error: ok ? undefined : body, response: new Response(null, { status }) } as never;
}

describe("refreshSession", () => {
  beforeEach(() => {
    postMock.mockReset();
    useSessionStore.setState({ status: "unknown", accessToken: null, expiresAt: null });
  });

  it("stores the new access token in memory and sends the CSRF header", async () => {
    postMock.mockResolvedValue(reply(200, { accessToken: "new-token", accessTokenExpiresAt: "2026-10-24T15:15:00Z" }));

    await expect(refreshSession()).resolves.toBe(true);

    expect(useSessionStore.getState()).toMatchObject({ status: "authenticated", accessToken: "new-token" });
    expect(postMock).toHaveBeenCalledWith("/api/v1/auth/refresh", {
      params: { header: { "X-Requested-With": "fetch" } },
    });
  });

  it("sends one request when called several times at once, because refresh tokens are single-use", async () => {
    postMock.mockResolvedValue(reply(200, { accessToken: "new-token", accessTokenExpiresAt: "2026-10-24T15:15:00Z" }));

    await Promise.all([refreshSession(), refreshSession(), refreshSession()]);

    expect(postMock).toHaveBeenCalledTimes(1);
  });

  it("marks the user signed out when the API rejects the refresh cookie", async () => {
    useSessionStore.setState({ status: "authenticated", accessToken: "old", expiresAt: 0 });
    postMock.mockResolvedValue(reply(401, { code: "UNAUTHENTICATED" }));

    await expect(refreshSession()).resolves.toBe(false);

    expect(useSessionStore.getState()).toMatchObject({ status: "anonymous", accessToken: null });
  });

  it("never stores the token in localStorage", async () => {
    postMock.mockResolvedValue(reply(200, { accessToken: "new-token", accessTokenExpiresAt: "2026-10-24T15:15:00Z" }));

    await refreshSession();

    expect(JSON.stringify({ ...window.localStorage })).not.toContain("new-token");
  });
});
