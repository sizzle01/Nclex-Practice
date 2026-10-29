"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useMe } from "@/features/auth/AuthProvider";
import { canReview, isAdmin } from "../roles";
import styles from "./admin.module.css";

/** The console's sections; reviewers see the queue, admins also see sets, banks and users. */
export function AdminNav() {
  const { data: me } = useMe();
  const pathname = usePathname();
  const links = [
    { href: "/admin/questions", label: "Questions", show: true },
    { href: "/admin/review", label: "Review queue", show: canReview(me?.role) },
    { href: "/admin/flags", label: "Reported questions", show: canReview(me?.role) },
    { href: "/admin/test-sets", label: "Test sets", show: isAdmin(me?.role) },
    { href: "/admin/banks", label: "Banks", show: isAdmin(me?.role) },
    { href: "/admin/users", label: "Users", show: isAdmin(me?.role) },
  ].filter((link) => link.show);

  return (
    <nav aria-label="Admin console" className={styles.nav}>
      <ul>
        {links.map((link) => (
          <li key={link.href}>
            <Link href={link.href} aria-current={pathname.startsWith(link.href) ? "page" : undefined}>
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}
