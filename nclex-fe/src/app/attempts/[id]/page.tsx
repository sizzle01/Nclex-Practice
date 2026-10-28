import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { AttemptPlayer } from "@/features/attempt/player/AttemptPlayer";

export const metadata: Metadata = { title: "Test | NCLEX Practice" };

export default async function AttemptPage({ params }: PageProps<"/attempts/[id]">) {
  const { id } = await params;
  return (
    <RequireAuth>
      <AttemptPlayer attemptId={id} />
    </RequireAuth>
  );
}
