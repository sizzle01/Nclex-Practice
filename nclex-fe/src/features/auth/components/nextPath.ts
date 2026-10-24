const defaultPath = "/dashboard";

/**
 * Where to go after signing in: the `?next=` path the user was bounced from, if it is a path on
 * this site. Anything else (e.g. "https://evil.example" or "//evil.example") is ignored, so the
 * login page cannot be used to redirect people to another site.
 */
export function nextPathFrom(search: string): string {
  const next = new URLSearchParams(search).get("next");
  if (next && next.startsWith("/") && !next.startsWith("//") && !next.startsWith("/\\")) return next;
  return defaultPath;
}
