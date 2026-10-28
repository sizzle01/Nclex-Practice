import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { ItemGallery } from "@/features/player/gallery/ItemGallery";

export const metadata: Metadata = { title: "Item renderers | NCLEX Practice" };

/** The renderer gallery is a development tool; production builds answer 404. */
export default function DevItemsPage() {
  if (process.env.NODE_ENV === "production") notFound();
  return <ItemGallery />;
}
