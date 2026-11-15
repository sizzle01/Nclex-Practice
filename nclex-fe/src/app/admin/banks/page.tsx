import type { Metadata } from "next";
import { BankList } from "@/features/admin/catalog/CatalogLists";
import { AdminGate } from "@/features/admin/components/AdminGate";

export const metadata: Metadata = { title: "Banks | Admin | NCLEX Practice" };

export default function AdminBanksPage() {
  return (
    <AdminGate allow="admin">
      <BankList />
    </AdminGate>
  );
}
