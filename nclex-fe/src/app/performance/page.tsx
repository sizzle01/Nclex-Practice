import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { PerformancePage } from "@/features/performance/components/PerformancePage";

export const metadata: Metadata = { title: "Performance | NCLEX Practice" };

export default function PerformanceRoute() {
  return (
    <RequireAuth>
      <PerformancePage />
    </RequireAuth>
  );
}
