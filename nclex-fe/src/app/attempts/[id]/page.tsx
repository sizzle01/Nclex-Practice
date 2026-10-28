import type { Metadata } from "next";
import { Suspense } from "react";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { AttemptPlayer } from "@/features/attempt/player/AttemptPlayer";

export const metadata: Metadata = { title: "Test | NCLEX Practice" };

// The attempt id and the path RequireAuth reads are only known per request, so both are read
// inside Suspense; the rest of the page can still be prerendered.
export default function AttemptPage({ params }: PageProps<"/attempts/[id]">) {
  return (
    <Suspense>
      <AttemptRoute params={params} />
    </Suspense>
  );
}

async function AttemptRoute({ params }: Pick<PageProps<"/attempts/[id]">, "params">) {
  const { id } = await params;
  return (
    <RequireAuth>
      <AttemptPlayer attemptId={id} />
    </RequireAuth>
  );
}
