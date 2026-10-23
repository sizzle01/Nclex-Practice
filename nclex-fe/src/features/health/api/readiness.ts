import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiSchema } from "@/lib/api/types";

export type Readiness = ApiSchema<"ReadinessResponse">;
export type HealthState = ApiSchema<"HealthState">;

/**
 * GET /readyz. A 503 is still a useful answer (it says which dependency is down),
 * so it is returned as data; only network failures and unexpected statuses throw.
 */
export async function fetchReadiness(): Promise<Readiness> {
  const { data, error, response } = await apiClient.GET("/readyz");
  if (data) return data;
  if (response.status === 503 && error) return error;
  throw new Error(`Readiness check failed with HTTP ${response.status}`);
}

export const readinessQueryKey = ["health", "readiness"] as const;

export function useReadiness() {
  return useQuery({
    queryKey: readinessQueryKey,
    queryFn: fetchReadiness,
    retry: false,
  });
}
