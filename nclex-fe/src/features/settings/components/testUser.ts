import type { UserProfile } from "@/features/auth/api/authApi";

/** A signed-in user for settings tests. */
export const testUser: UserProfile = {
  id: "0192f0a0-0000-7000-8000-000000000001",
  email: "ada@example.com",
  emailVerified: true,
  role: "candidate",
  track: "RN",
  planTier: "free",
  displayName: "Ada",
  timezone: "UTC",
  targetExamDate: null,
};
