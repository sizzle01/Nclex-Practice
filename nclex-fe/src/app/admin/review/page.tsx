import type { Metadata } from "next";
import { AdminGate } from "@/features/admin/components/AdminGate";
import { QuestionList } from "@/features/admin/questions/QuestionList";

export const metadata: Metadata = { title: "Review queue | Admin | NCLEX Practice" };

export default function ReviewQueuePage() {
  return (
    <AdminGate allow="reviewer">
      <QuestionList title="Review queue" fixedStatus="in_review" intro="Versions waiting for review, oldest first. You can't review your own." />
    </AdminGate>
  );
}
