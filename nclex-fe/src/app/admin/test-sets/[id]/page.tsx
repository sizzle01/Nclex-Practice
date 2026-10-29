import type { Metadata } from "next";
import { Suspense } from "react";
import { TestSetEditor } from "@/features/admin/catalog/TestSetEditor";
import { AdminGate } from "@/features/admin/components/AdminGate";
import { isAdmin } from "@/features/admin/roles";

export const metadata: Metadata = { title: "Test sets | Admin | NCLEX Practice" };

export default function AdminTestSetPage({ params }: PageProps<"/admin/test-sets/[id]">) {
  return (
    <Suspense>
      <TestSetRoute params={params} />
    </Suspense>
  );
}

async function TestSetRoute({ params }: Pick<PageProps<"/admin/test-sets/[id]">, "params">) {
  const { id } = await params;
  return (
    <AdminGate allow={isAdmin}>
      <TestSetEditor setId={id} />
    </AdminGate>
  );
}
