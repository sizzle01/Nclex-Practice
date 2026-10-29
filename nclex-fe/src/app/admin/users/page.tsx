import type { Metadata } from "next";
import { AdminGate } from "@/features/admin/components/AdminGate";
import { isAdmin } from "@/features/admin/roles";
import { UserAdmin } from "@/features/admin/users/UserAdmin";

export const metadata: Metadata = { title: "Users | Admin | NCLEX Practice" };

export default function AdminUsersPage() {
  return (
    <AdminGate allow={isAdmin}>
      <UserAdmin />
    </AdminGate>
  );
}
