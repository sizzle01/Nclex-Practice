import type { Metadata } from "next";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { CustomBuilder } from "@/features/custom-builder/components/CustomBuilder";

export const metadata: Metadata = { title: "Build a custom test | NCLEX Practice" };

export default function CustomBuilderPage() {
  return (
    <RequireAuth>
      <CustomBuilder />
    </RequireAuth>
  );
}
