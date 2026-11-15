import type { Metadata } from "next";
import { AdminGate } from "@/features/admin/components/AdminGate";
import { NewQuestion } from "@/features/admin/editor/NewQuestion";

export const metadata: Metadata = { title: "New question | Admin | NCLEX Practice" };

export default function NewQuestionPage() {
  return (
    <AdminGate allow="author">
      <NewQuestion />
    </AdminGate>
  );
}
