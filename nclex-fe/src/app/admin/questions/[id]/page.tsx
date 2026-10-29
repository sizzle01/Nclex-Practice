import type { Metadata } from "next";
import { Suspense } from "react";
import { QuestionEditor } from "@/features/admin/editor/QuestionEditor";

export const metadata: Metadata = { title: "Question | Admin | NCLEX Practice" };

// The id is only known per request, so it is read inside Suspense (as on the attempt pages).
export default function AdminQuestionPage({ params, searchParams }: PageProps<"/admin/questions/[id]">) {
  return (
    <Suspense>
      <QuestionRoute params={params} searchParams={searchParams} />
    </Suspense>
  );
}

async function QuestionRoute({ params, searchParams }: Pick<PageProps<"/admin/questions/[id]">, "params" | "searchParams">) {
  const [{ id }, { version }] = await Promise.all([params, searchParams]);
  return <QuestionEditor questionId={id} initialVersionId={typeof version === "string" ? version : undefined} />;
}
