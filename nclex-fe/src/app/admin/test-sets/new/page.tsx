import type { Metadata } from "next";
import { TestSetEditor } from "@/features/admin/catalog/TestSetEditor";
import { AdminGate } from "@/features/admin/components/AdminGate";

export const metadata: Metadata = { title: "New | Test sets | Admin | NCLEX Practice" };

export default function NewTestSetPage() {
  return (
    <AdminGate allow="admin">
      <TestSetEditor />
    </AdminGate>
  );
}
