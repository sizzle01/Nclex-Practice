import type { Metadata } from "next";
import { BankList } from "@/features/admin/catalog/CatalogLists";
import { AdminGate } from "@/features/admin/components/AdminGate";
import { isAdmin } from "@/features/admin/roles";

export const metadata: Metadata = { title: "Banks | Admin | NCLEX Practice" };

export default function AdminBanksPage() {
  return (
    <AdminGate allow={isAdmin}>
      <BankList />
    </AdminGate>
  );
}
