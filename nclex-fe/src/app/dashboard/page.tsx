import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { Dashboard } from "@/features/dashboard/components/Dashboard";

export const metadata: Metadata = { title: "Dashboard | NCLEX Practice" };

export default function DashboardPage() {
  return (
    <RequireAuth>
      <Dashboard />
    </RequireAuth>
  );
}
