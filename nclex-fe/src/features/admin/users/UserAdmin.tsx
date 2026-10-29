"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import ui from "@/components/ui/ui.module.css";
import { useMe } from "@/features/auth/AuthProvider";
import { useDebouncedValue } from "@/features/custom-builder/useDebouncedValue";
import { isApiError } from "@/lib/api/errors";
import { adminKeys, type AdminUserView, listUsers, updateUser, type UpdateAdminUserRequest, type UserRole } from "../api/adminApi";
import styles from "../components/admin.module.css";

const roles: Array<{ role: NonNullable<UserRole>; label: string }> = [
  { role: "candidate", label: "Candidate" },
  { role: "educator", label: "Educator" },
  { role: "author", label: "Author" },
  { role: "reviewer", label: "Reviewer" },
  { role: "admin", label: "Admin" },
];

/**
 * /admin/users: find an account and change its role or plan, or unlock it (api.md section 8).
 * A role change signs that person out, so they sign in again with the new role.
 */
export function UserAdmin() {
  const [text, setText] = useState("");
  const q = useDebouncedValue(text.trim(), 300);
  const { data, isPending, isError } = useQuery({ queryKey: adminKeys.users(q), queryFn: () => listUsers(q) });

  return (
    <section aria-labelledby="users-heading">
      <h1 id="users-heading">Users</h1>
      <div className={styles.filters} role="search" aria-label="Find users">
        <label>
          Email or name
          <input type="search" value={text} onChange={(event) => setText(event.target.value)} />
        </label>
      </div>
      {isPending ? (
        <p role="status">Loading users…</p>
      ) : isError ? (
        <p role="alert">The users could not be loaded.</p>
      ) : data.items.length === 0 ? (
        <p role="status">No users match.</p>
      ) : (
        <div className={styles.tableWrap}>
          <table className={styles.table} aria-labelledby="users-heading">
            <thead>
              <tr>
                <th scope="col">Account</th>
                <th scope="col">Role</th>
                <th scope="col">Plan</th>
                <th scope="col">Status</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((user) => (
                <UserRow key={user.id} user={user} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

function UserRow({ user }: { user: AdminUserView }) {
  const queryClient = useQueryClient();
  const { data: me } = useMe();
  const isMe = me?.id === user.id;
  const locked = user.lockedUntil !== null && new Date(user.lockedUntil) > new Date();
  const mutation = useMutation({
    mutationFn: (body: UpdateAdminUserRequest) => updateUser(user.id, body),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["admin", "users"] }),
  });
  const name = user.displayName ? `${user.displayName} (${user.email})` : user.email;

  return (
    <tr>
      <td>
        {name}
        {!user.emailVerified && <span className={styles.status}> Unverified</span>}
      </td>
      <td>
        <select
          aria-label={`Role of ${user.email}`}
          value={user.role ?? "candidate"}
          // An admin can't change their own role, or they could lock everyone out of the console.
          disabled={isMe || mutation.isPending}
          onChange={(event) => mutation.mutate({ role: event.target.value as UserRole, planTier: null, unlock: null })}
        >
          {roles.map((r) => (
            <option key={r.role} value={r.role}>
              {r.label}
            </option>
          ))}
        </select>
      </td>
      <td>
        <select
          aria-label={`Plan of ${user.email}`}
          value={user.planTier ?? "free"}
          disabled={mutation.isPending}
          onChange={(event) => mutation.mutate({ role: null, planTier: event.target.value as "free" | "premium", unlock: null })}
        >
          <option value="free">Free</option>
          <option value="premium">Premium</option>
        </select>
      </td>
      <td>
        {locked ? (
          <button type="button" className={ui.buttonSecondary} disabled={mutation.isPending} onClick={() => mutation.mutate({ role: null, planTier: null, unlock: true })}>
            Unlock {user.email}
          </button>
        ) : (
          "Active"
        )}
        {mutation.isError && (
          <p role="alert" className={styles.status}>
            {isApiError(mutation.error) ? mutation.error.message : "Not saved."}
          </p>
        )}
      </td>
    </tr>
  );
}
