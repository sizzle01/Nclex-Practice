import type { UserRole } from "./api/adminApi";

// The role groups of api.md section 8 (A = author, R = reviewer, Ad = admin). The API enforces
// them; the console only uses them to show the actions a person can take.

/** Author, reviewer or admin: may open the console at all. */
export function isContentStaff(role: UserRole | undefined): boolean {
  return role === "author" || role === "reviewer" || role === "admin";
}

/** Writes questions, edits drafts, uploads media. */
export function canAuthor(role: UserRole | undefined): boolean {
  return role === "author" || role === "admin";
}

/** Approves, rejects, publishes and retires. */
export function canReview(role: UserRole | undefined): boolean {
  return role === "reviewer" || role === "admin";
}

/** Manages sets, banks and users. */
export function isAdmin(role: UserRole | undefined): boolean {
  return role === "admin";
}
