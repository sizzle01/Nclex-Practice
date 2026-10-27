import type { UserProfile } from "@/features/auth/api/authApi";
import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type UpdateMeRequest = ApiSchema<"UpdateMeRequest">;
export type ChangePasswordRequest = ApiSchema<"ChangePasswordRequest">;
export type ChangeEmailRequest = ApiSchema<"ChangeEmailRequest">;
export type DeleteAccountRequest = ApiSchema<"DeleteAccountRequest">;

/** PATCH /me: omitted fields stay as they are. Returns the updated profile. */
export async function updateMe(body: UpdateMeRequest): Promise<UserProfile> {
  const { data, error, response } = await apiClient.PATCH("/api/v1/me", { body });
  if (data) return data;
  throw ApiError.from(response, error);
}

/**
 * POST /me/password. This device stays signed in; the others are signed out.
 * A wrong current password fails with VALIDATION_FAILED on currentPassword; too many lock the account (423).
 */
export async function changePassword(body: ChangePasswordRequest): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/me/password", { body });
  if (!response.ok) throw ApiError.from(response, error);
}

/** POST /me/email: emails a confirmation link to the new address. Nothing changes until it is used. */
export async function requestEmailChange(body: ChangeEmailRequest): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/me/email", { body });
  if (!response.ok) throw ApiError.from(response, error);
}

/** DELETE /me: deletes the account and ends every session, this one included. */
export async function deleteAccount(body: DeleteAccountRequest): Promise<void> {
  const { error, response } = await apiClient.DELETE("/api/v1/me", { body });
  if (!response.ok) throw ApiError.from(response, error);
}
