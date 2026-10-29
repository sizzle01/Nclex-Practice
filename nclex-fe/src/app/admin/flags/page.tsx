import type { Metadata } from "next";
import { ModerationQueue } from "@/features/flags/components/ModerationQueue";
import { RequireModerator } from "@/features/flags/components/RequireModerator";

export const metadata: Metadata = { title: "Reported questions | NCLEX Practice" };

export default function ModerationQueuePage() {
  return (
    <RequireModerator>
      <ModerationQueue />
    </RequireModerator>
  );
}
