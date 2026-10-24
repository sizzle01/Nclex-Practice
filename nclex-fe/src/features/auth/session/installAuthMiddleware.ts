import { apiClient } from "@/lib/api/client";
import { authMiddleware } from "./authMiddleware";

let installed = false;

/** Adds the access-token middleware to the shared API client, once. */
export function installAuthMiddleware() {
  if (installed) return;
  apiClient.use(authMiddleware);
  installed = true;
}
