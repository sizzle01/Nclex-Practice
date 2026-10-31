import type { Metadata } from "next";
import { AdminGate } from "@/features/admin/components/AdminGate";
import { canReview } from "@/features/admin/roles";
import { SubtopicAdmin } from "@/features/admin/subtopics/SubtopicAdmin";

export const metadata: Metadata = { title: "Subtopics | Admin | NCLEX Practice" };

export default function AdminSubtopicsPage() {
  return (
    <AdminGate allow={canReview}>
      <SubtopicAdmin />
    </AdminGate>
  );
}
