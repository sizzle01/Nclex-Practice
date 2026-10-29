import type { NextConfig } from "next";
import { securityHeaders } from "./src/lib/security/securityHeaders";

// Where the .NET API listens. The browser never calls it directly: requests go to this Next.js
// server, which forwards them (rewrites below), so the app and API share one origin and no CORS
// setup is needed.
const apiUrl = process.env.API_URL ?? "http://localhost:5080";

const nextConfig: NextConfig = {
  cacheComponents: true,
  partialPrefetching: true,
  // A self-contained server in .next/standalone, which the Docker image runs (architecture.md section 8).
  output: "standalone",
  poweredByHeader: false,
  async headers() {
    return [
      {
        // Pages only: /api responses come from the API, which sets its own headers.
        source: "/((?!api/).*)",
        headers: securityHeaders(process.env.NODE_ENV === "development"),
      },
    ];
  },
  async rewrites() {
    return [
      { source: "/api/:path*", destination: `${apiUrl}/api/:path*` },
      { source: "/healthz", destination: `${apiUrl}/healthz` },
      { source: "/readyz", destination: `${apiUrl}/readyz` },
    ];
  },
};

export default nextConfig;
