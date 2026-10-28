import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { NotificationPreferencesForm } from "@/features/notifications/components/NotificationPreferencesForm";

export const metadata: Metadata = { title: "Notification settings | NCLEX Practice" };

export default function NotificationSettingsPage() {
  return (
    <RequireAuth>
      <NotificationPreferencesForm />
    </RequireAuth>
  );
}
