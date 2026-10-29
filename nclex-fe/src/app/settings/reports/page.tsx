import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { MyReports } from "@/features/flags/components/MyReports";

export const metadata: Metadata = { title: "My reports | NCLEX Practice" };

export default function MyReportsPage() {
  return (
    <RequireAuth>
      <MyReports />
    </RequireAuth>
  );
}
