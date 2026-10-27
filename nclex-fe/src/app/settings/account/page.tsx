import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { AccountSettings } from "@/features/settings/components/AccountSettings";

export const metadata: Metadata = { title: "Account settings | NCLEX Practice" };

export default function AccountSettingsPage() {
  return (
    <RequireAuth>
      <AccountSettings />
    </RequireAuth>
  );
}
