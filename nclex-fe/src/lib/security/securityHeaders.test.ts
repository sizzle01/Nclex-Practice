import { describe, expect, it } from "vitest";
import { contentSecurityPolicy, securityHeaders } from "./securityHeaders";

function header(isDev: boolean, key: string) {
  return securityHeaders(isDev).find((h) => h.key === key)?.value;
}

describe("securityHeaders", () => {
  it("forbids framing, plugins and other origins in production", () => {
    const csp = contentSecurityPolicy(false);

    expect(csp).toContain("frame-ancestors 'none'");
    expect(csp).toContain("object-src 'none'");
    expect(csp).toContain("default-src 'self'");
    expect(csp).toContain("connect-src 'self'");
    expect(csp).not.toContain("unsafe-eval");
    expect(csp).not.toContain("ws:");
  });

  it("allows eval and the reload socket only in development", () => {
    const csp = contentSecurityPolicy(true);

    expect(csp).toContain("'unsafe-eval'");
    expect(csp).toContain("connect-src 'self' ws:");
    expect(csp).not.toContain("upgrade-insecure-requests");
  });

  it("sends HSTS in production only", () => {
    expect(header(false, "Strict-Transport-Security")).toBe("max-age=31536000; includeSubDomains");
    expect(header(true, "Strict-Transport-Security")).toBeUndefined();
  });

  it("always sends the anti-sniffing and anti-framing headers", () => {
    for (const isDev of [true, false]) {
      expect(header(isDev, "X-Content-Type-Options")).toBe("nosniff");
      expect(header(isDev, "X-Frame-Options")).toBe("DENY");
    }
  });
});
