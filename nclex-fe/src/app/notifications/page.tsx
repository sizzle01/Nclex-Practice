import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { NotificationsHistory } from "@/features/notifications/components/NotificationsHistory";

export const metadata: Metadata = { title: "Notifications | NCLEX Practice" };

export default function NotificationsPage() {
  return (
    <RequireAuth>
      <NotificationsHistory />
    </RequireAuth>
  );
}
