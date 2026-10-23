import type { Metadata } from "next";
import { HealthStatus } from "@/features/health/components/HealthStatus";

export const metadata: Metadata = { title: "System status | NCLEX Practice" };

export default function HealthPage() {
  return <HealthStatus />;
}
