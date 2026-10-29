import { useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  fetchFlagQueue,
  fetchMyFlags,
  type FlagStatus,
  reportIssue,
  type ReportIssueInput,
  updateFlag,
  type UpdateFlagRequest,
} from "./flagsApi";

export const flagKeys = {
  mine: ["flags", "mine"] as const,
  queues: ["flags", "queue"] as const,
  queue: (status: FlagStatus) => ["flags", "queue", status] as const,
};

/** Reports a question, then refreshes "My reports". */
export function useReportIssue() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: ReportIssueInput) => reportIssue(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: flagKeys.mine }),
  });
}

export function useMyFlags() {
  return useInfiniteQuery({
    queryKey: flagKeys.mine,
    queryFn: ({ pageParam }) => fetchMyFlags(pageParam),
    initialPageParam: null as string | null,
    getNextPageParam: (page) => page.nextCursor ?? undefined,
  });
}

export function useFlagQueue(status: FlagStatus) {
  return useInfiniteQuery({
    queryKey: flagKeys.queue(status),
    queryFn: ({ pageParam }) => fetchFlagQueue(status, pageParam),
    initialPageParam: null as string | null,
    getNextPageParam: (page) => page.nextCursor ?? undefined,
  });
}

/** Changes a report; every status tab is refreshed, because the report may have moved between them. */
export function useUpdateFlag() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateFlagRequest }) => updateFlag(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: flagKeys.queues }),
  });
}
