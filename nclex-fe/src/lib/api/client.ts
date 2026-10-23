import createClient from "openapi-fetch";
import type { paths } from "./schema";

/**
 * Typed client for the .NET API. Paths, parameters and response bodies come from the
 * generated schema (npm run gen:api), never written by hand (ADR-8).
 * Relative base URL: requests go to the Next.js server, which forwards them to the API.
 */
export const apiClient = createClient<paths>({ baseUrl: "" });
