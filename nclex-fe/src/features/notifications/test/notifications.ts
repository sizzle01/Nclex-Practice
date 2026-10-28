import type { NotificationView } from "../api/notificationsApi";

/** A notification as the API sends it; override what the test cares about. */
export function aNotification(overrides: Partial<NotificationView> = {}): NotificationView {
  return {
    id: "n-1",
    type: "result_fail",
    category: "results",
    title: "You didn't reach the passing threshold this time",
    body: "You scored 58%. Here's what to focus on: Pharmacology, Risk Reduction.",
    data: { scorePct: 0.58, weakest: ["PI.PPT"], deepLink: "/attempts/a-1/results#breakdown" },
    relatedAttemptId: "a-1",
    readAt: null,
    createdAt: "2026-10-28T09:00:00Z",
    ...overrides,
  };
}
