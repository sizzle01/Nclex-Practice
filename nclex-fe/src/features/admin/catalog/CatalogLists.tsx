"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import ui from "@/components/ui/ui.module.css";
import { toNumber } from "@/lib/format/toNumber";
import { adminKeys, listBanks, listTestSets } from "../api/adminApi";
import styles from "../components/admin.module.css";

/** /admin/test-sets: curated sets, published or not. */
export function TestSetList() {
  const { data, isPending, isError } = useQuery({ queryKey: adminKeys.testSets, queryFn: listTestSets });
  return (
    <section aria-labelledby="sets-heading">
      <div className={styles.toolbar}>
        <h1 id="sets-heading">Curated test sets</h1>
        <Link href="/admin/test-sets/new" className={ui.button}>
          New set
        </Link>
      </div>
      {isPending ? (
        <p role="status">Loading sets…</p>
      ) : isError ? (
        <p role="alert">The sets could not be loaded.</p>
      ) : (
        <div className={styles.tableWrap}>
          <table className={styles.table} aria-labelledby="sets-heading">
            <thead>
              <tr>
                <th scope="col">Title</th>
                <th scope="col">Track</th>
                <th scope="col">Mode</th>
                <th scope="col">Questions</th>
                <th scope="col">Access</th>
                <th scope="col">Published</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((set) => (
                <tr key={set.id}>
                  <td>
                    <Link href={`/admin/test-sets/${set.id}`}>{set.title}</Link>
                    {set.isSample && <span className={styles.status}> Sample</span>}
                  </td>
                  <td>{set.track}</td>
                  <td>{set.mode}</td>
                  <td>{toNumber(set.questionCount)}</td>
                  <td>{set.accessTier}</td>
                  <td>{set.isPublished ? "Yes" : "No"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

/** /admin/banks: question banks, published or not. */
export function BankList() {
  const { data, isPending, isError } = useQuery({ queryKey: adminKeys.banks, queryFn: listBanks });
  return (
    <section aria-labelledby="banks-heading">
      <div className={styles.toolbar}>
        <h1 id="banks-heading">Question banks</h1>
        <Link href="/admin/banks/new" className={ui.button}>
          New bank
        </Link>
      </div>
      {isPending ? (
        <p role="status">Loading banks…</p>
      ) : isError ? (
        <p role="alert">The banks could not be loaded.</p>
      ) : (
        <div className={styles.tableWrap}>
          <table className={styles.table} aria-labelledby="banks-heading">
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Track</th>
                <th scope="col">Questions</th>
                <th scope="col">Access</th>
                <th scope="col">Published</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((bank) => (
                <tr key={bank.id}>
                  <td>
                    <Link href={`/admin/banks/${bank.id}`}>{bank.name}</Link>
                  </td>
                  <td>{bank.track}</td>
                  <td>{toNumber(bank.questionCount)}</td>
                  <td>{bank.accessTier}</td>
                  <td>{bank.isPublished ? "Yes" : "No"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
