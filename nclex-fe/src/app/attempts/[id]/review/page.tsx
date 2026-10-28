import type { Metadata } from "next";
import { Suspense } from "react";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import type { ReviewFilter } from "@/features/attempt/api/attemptApi";
import { ReviewPage } from "@/features/review/ReviewPage";

export const metadata: Metadata = { title: "Review | NCLEX Practice" };

const reviewFilters: readonly ReviewFilter[] = ["all", "incorrect", "flagged"];

// The attempt id and ?filter= are only known per request, so they are read inside Suspense.
export default function AttemptReviewPage({ params, searchParams }: PageProps<"/attempts/[id]/review">) {
  return (
    <Suspense>
      <ReviewRoute params={params} searchParams={searchParams} />
    </Suspense>
  );
}

async function ReviewRoute({ params, searchParams }: PageProps<"/attempts/[id]/review">) {
  const { id } = await params;
  const { filter } = await searchParams;
  // The results page links here with ?filter=incorrect; anything unknown shows every answer.
  const initialFilter = reviewFilters.find((f) => f === filter) ?? "all";
  return (
    <RequireAuth>
      <ReviewPage attemptId={id} initialFilter={initialFilter} />
    </RequireAuth>
  );
}
