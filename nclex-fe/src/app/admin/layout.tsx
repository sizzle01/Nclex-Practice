import { Suspense } from "react";
import { AdminShell } from "@/features/admin/components/AdminShell";

// Every /admin page needs a signed-in member of content staff (api.md section 8).
// The shell reads the URL (RequireAuth's return path, the nav's active link). With cacheComponents
// on, dynamic routes such as /admin/banks/[id] may only read it inside a Suspense boundary.
export default function AdminLayout({ children }: LayoutProps<"/admin">) {
  return (
    <Suspense>
      <AdminShell>{children}</AdminShell>
    </Suspense>
  );
}
