// Security headers for every page the Next.js server returns (architecture.md section 6).
// The API sets its own, stricter headers on /api responses (Nclex.Api SecurityHeadersMiddleware).

export type Header = { key: string; value: string };

/**
 * The Content-Security-Policy for pages.
 *
 * Scripts allow 'unsafe-inline' because the app uses Cache Components (partial prerendering):
 * the prerendered shell is built before any request exists, so it cannot carry a per-request
 * nonce, and Next.js's own inline scripts differ per page, so hashes cannot list them either.
 * Everything else is locked to this origin: the API, media and the notification stream are all
 * served through the same-origin rewrites in next.config.ts.
 */
export function contentSecurityPolicy(isDev: boolean): string {
  const directives = [
    "default-src 'self'",
    // React in development uses eval for its error overlay; production never does.
    `script-src 'self' 'unsafe-inline'${isDev ? " 'unsafe-eval'" : ""}`,
    "style-src 'self' 'unsafe-inline'",
    "img-src 'self' blob: data:",
    "media-src 'self' blob:",
    "font-src 'self'",
    // next dev reloads over a WebSocket on the same host.
    `connect-src 'self'${isDev ? " ws:" : ""}`,
    "object-src 'none'",
    "base-uri 'self'",
    "form-action 'self'",
    "frame-ancestors 'none'",
  ];
  // No upgrade-insecure-requests: HSTS already moves production to HTTPS, and the directive would
  // break the plain-http compose stack the end-to-end tests run against.
  return directives.join("; ");
}

export function securityHeaders(isDev: boolean): Header[] {
  const headers: Header[] = [
    { key: "Content-Security-Policy", value: contentSecurityPolicy(isDev) },
    { key: "X-Content-Type-Options", value: "nosniff" },
    { key: "X-Frame-Options", value: "DENY" },
    { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
    // The player never needs the camera, microphone or location.
    { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
  ];
  // Locally the app runs on plain http://localhost, where HSTS would only get in the way.
  if (!isDev) {
    headers.push({ key: "Strict-Transport-Security", value: "max-age=31536000; includeSubDomains" });
  }
  return headers;
}
