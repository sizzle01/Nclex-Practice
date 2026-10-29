import type { Metadata } from "next";
import { Suspense } from "react";
import { BankEditor } from "@/features/admin/catalog/BankEditor";
import { AdminGate } from "@/features/admin/components/AdminGate";
import { isAdmin } from "@/features/admin/roles";

export const metadata: Metadata = { title: "Banks | Admin | NCLEX Practice" };

export default function AdminBankPage({ params }: PageProps<"/admin/banks/[id]">) {
  return (
    <Suspense>
      <BankRoute params={params} />
    </Suspense>
  );
}

async function BankRoute({ params }: Pick<PageProps<"/admin/banks/[id]">, "params">) {
  const { id } = await params;
  return (
    <AdminGate allow={isAdmin}>
      <BankEditor bankId={id} />
    </AdminGate>
  );
}
