import type { Metadata } from "next";
import { Suspense } from "react";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { ResultsPage } from "@/features/results/ResultsPage";

export const metadata: Metadata = { title: "Results | NCLEX Practice" };

// As on the player page, the attempt id is only known per request, so it is read inside Suspense.
export default function AttemptResultsPage({ params }: PageProps<"/attempts/[id]/results">) {
  return (
    <Suspense>
      <ResultsRoute params={params} />
    </Suspense>
  );
}

async function ResultsRoute({ params }: Pick<PageProps<"/attempts/[id]/results">, "params">) {
  const { id } = await params;
  return (
    <RequireAuth>
      <ResultsPage attemptId={id} />
    </RequireAuth>
  );
}
