import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { DashboardPlaceholder } from "@/features/dashboard/components/DashboardPlaceholder";

export const metadata: Metadata = { title: "Dashboard | NCLEX Practice" };

export default function DashboardPage() {
  return (
    <RequireAuth>
      <DashboardPlaceholder />
    </RequireAuth>
  );
}
