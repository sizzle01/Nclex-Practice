import { AdminShell } from "@/features/admin/components/AdminShell";

// Every /admin page needs a signed-in member of content staff (api.md section 8).
export default function AdminLayout({ children }: LayoutProps<"/admin">) {
  return <AdminShell>{children}</AdminShell>;
}
