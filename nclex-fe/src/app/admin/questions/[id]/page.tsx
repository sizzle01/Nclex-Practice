import type { Metadata } from "next";
import { Suspense } from "react";
import { QuestionEditor } from "@/features/admin/editor/QuestionEditor";

export const metadata: Metadata = { title: "Question | Admin | NCLEX Practice" };

// The id is only known per request, so it is read inside Suspense (as on the attempt pages).
export default function AdminQuestionPage({ params }: PageProps<"/admin/questions/[id]">) {
  return (
    <Suspense>
      <QuestionRoute params={params} />
    </Suspense>
  );
}

async function QuestionRoute({ params }: Pick<PageProps<"/admin/questions/[id]">, "params">) {
  const { id } = await params;
  return <QuestionEditor questionId={id} />;
}
