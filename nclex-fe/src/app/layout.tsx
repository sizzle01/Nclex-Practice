import type { Metadata } from "next";
import { TopBar } from "@/components/app-shell/TopBar";
import { Providers } from "./providers";
import "./globals.css";

export const metadata: Metadata = {
  title: "NCLEX Practice",
  description: "NCLEX-RN and NCLEX-PN practice questions and mock exams.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en">
      <body>
        <Providers>
          <TopBar />
          <main>{children}</main>
        </Providers>
      </body>
    </html>
  );
}
