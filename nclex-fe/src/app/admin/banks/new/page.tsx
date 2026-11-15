import type { Metadata } from "next";
import { BankEditor } from "@/features/admin/catalog/BankEditor";
import { AdminGate } from "@/features/admin/components/AdminGate";

export const metadata: Metadata = { title: "New | Banks | Admin | NCLEX Practice" };

export default function NewBankPage() {
  return (
    <AdminGate allow="admin">
      <BankEditor />
    </AdminGate>
  );
}
