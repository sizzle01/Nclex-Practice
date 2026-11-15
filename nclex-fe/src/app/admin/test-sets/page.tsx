import type { Metadata } from "next";
import { TestSetList } from "@/features/admin/catalog/CatalogLists";
import { AdminGate } from "@/features/admin/components/AdminGate";

export const metadata: Metadata = { title: "Test sets | Admin | NCLEX Practice" };

export default function AdminTestSetsPage() {
  return (
    <AdminGate allow="admin">
      <TestSetList />
    </AdminGate>
  );
}
