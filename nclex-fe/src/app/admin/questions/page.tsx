import type { Metadata } from "next";
import { QuestionList } from "@/features/admin/questions/QuestionList";

export const metadata: Metadata = { title: "Questions | Admin | NCLEX Practice" };

export default function AdminQuestionsPage() {
  return <QuestionList title="Questions" />;
}
