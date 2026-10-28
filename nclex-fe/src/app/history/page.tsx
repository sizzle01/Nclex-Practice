import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { HistoryPage } from "@/features/history/components/HistoryPage";

export const metadata: Metadata = { title: "History | NCLEX Practice" };

export default function HistoryRoute() {
  return (
    <RequireAuth>
      <HistoryPage />
    </RequireAuth>
  );
}
