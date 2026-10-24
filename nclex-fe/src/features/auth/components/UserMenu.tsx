"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useId, useRef, useState } from "react";
import { useAuth, useMe } from "../AuthProvider";
import styles from "./UserMenu.module.css";

/**
 * Top-bar account controls. Signed out: log in / sign up links. Signed in: a button that opens a
 * small menu with "Log out" and "Log out on all devices". It is a disclosure (button plus list of
 * buttons) rather than an ARIA menu, so Tab and Enter work as everywhere else; Escape closes it.
 */
export function UserMenu() {
  const { status, signOut, signOutEverywhere } = useAuth();
  const { data: me } = useMe();
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [busy, setBusy] = useState(false);
  const menuId = useId();
  const containerRef = useRef<HTMLDivElement>(null);
  const toggleRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (!open) return;
    function closeOnOutsideClick(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) setOpen(false);
    }
    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
        toggleRef.current?.focus();
      }
    }
    document.addEventListener("mousedown", closeOnOutsideClick);
    document.addEventListener("keydown", closeOnEscape);
    return () => {
      document.removeEventListener("mousedown", closeOnOutsideClick);
      document.removeEventListener("keydown", closeOnEscape);
    };
  }, [open]);

  if (status === "unknown") return null;

  if (status === "anonymous") {
    return (
      <nav aria-label="Account" className={styles.links}>
        <Link href="/login">Log in</Link>
        <Link href="/signup">Sign up</Link>
      </nav>
    );
  }

  async function run(action: () => Promise<void>) {
    setBusy(true);
    try {
      await action();
    } finally {
      setBusy(false);
      setOpen(false);
      router.push("/login");
    }
  }

  return (
    <div ref={containerRef} className={styles.container}>
      <button
        ref={toggleRef}
        type="button"
        className={styles.toggle}
        aria-expanded={open}
        aria-controls={menuId}
        onClick={() => setOpen((value) => !value)}
      >
        {me?.displayName ?? me?.email ?? "Account"} <span aria-hidden="true">▾</span>
      </button>
      {open && (
        <ul id={menuId} className={styles.menu}>
          <li>
            <Link href="/dashboard" onClick={() => setOpen(false)}>
              Dashboard
            </Link>
          </li>
          <li>
            <button type="button" disabled={busy} onClick={() => run(signOut)}>
              Log out
            </button>
          </li>
          <li>
            <button type="button" disabled={busy} onClick={() => run(signOutEverywhere)}>
              Log out on all devices
            </button>
          </li>
        </ul>
      )}
    </div>
  );
}
