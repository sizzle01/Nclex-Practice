import type { Metadata } from "next";
import { Suspense } from "react";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { PracticeCatalog } from "@/features/practice/components/PracticeCatalog";

export const metadata: Metadata = { title: "Practice | NCLEX Practice" };

// Suspense: the catalog reads ?set= from the URL, which is only known when the page is requested.
export default function PracticePage() {
  return (
    <RequireAuth>
      <Suspense>
        <PracticeCatalog />
      </Suspense>
    </RequireAuth>
  );
}
