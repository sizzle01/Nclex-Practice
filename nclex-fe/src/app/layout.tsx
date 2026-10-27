import type { Metadata } from "next";
import { Inter } from "next/font/google";
import { TopBar } from "@/components/app-shell/TopBar";
import { themeScript } from "@/features/theme/themeScript";
import { Providers } from "./providers";
import "./globals.css";

// next/font self-hosts Inter at build time, so the browser never calls Google. Exposed as a CSS
// variable that globals.css puts first in --font-sans.
const inter = Inter({ subsets: ["latin"], display: "swap", variable: "--font-inter" });

export const metadata: Metadata = {
  title: "NCLEX Practice",
  description: "NCLEX-RN and NCLEX-PN practice questions and mock exams.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    // suppressHydrationWarning: the head script may add data-theme to <html> before React hydrates.
    <html lang="en" className={inter.variable} suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
      </head>
      <body>
        <Providers>
          <TopBar />
          <main>{children}</main>
        </Providers>
      </body>
    </html>
  );
}
