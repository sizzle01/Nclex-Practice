"use client";

import type { ReactNode } from "react";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { AdminGate } from "./AdminGate";
import styles from "./admin.module.css";
import { AdminNav } from "./AdminNav";

/** The admin console frame: sign-in, the staff check and the section links (interfaces.md 5.2). */
export function AdminShell({ children }: { children: ReactNode }) {
  return (
    <RequireAuth>
      <AdminGate>
        <div className={styles.layout}>
          <AdminNav />
          {children}
        </div>
      </AdminGate>
    </RequireAuth>
  );
}
