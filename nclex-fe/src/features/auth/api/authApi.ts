import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type UserProfile = ApiSchema<"UserProfile">;
export type AuthResponse = ApiSchema<"AuthResponse">;
export type SignupRequest = ApiSchema<"SignupRequest">;
export type LoginRequest = ApiSchema<"LoginRequest">;
export type ExamTrack = ApiSchema<"ExamTrack">;
export type ResetPasswordRequest = ApiSchema<"ResetPasswordRequest">;

/** POST /auth/signup. The refresh cookie is set by the response; the body carries the access token. */
export async function signup(body: SignupRequest): Promise<AuthResponse> {
  const { data, error, response } = await apiClient.POST("/api/v1/auth/signup", { body });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** POST /auth/login. */
export async function login(body: LoginRequest): Promise<AuthResponse> {
  const { data, error, response } = await apiClient.POST("/api/v1/auth/login", { body });
  if (data) return data;
  throw ApiError.from(response, error);
}

/** POST /auth/logout: ends this device's session and clears the cookie. */
export async function logout(): Promise<void> {
  await apiClient.POST("/api/v1/auth/logout");
}

/** POST /auth/logout-all: ends the sessions on every device. */
export async function logoutAll(): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/auth/logout-all");
  if (!response.ok) throw ApiError.from(response, error);
}

/** GET /me. */
export async function fetchMe(): Promise<UserProfile> {
  const { data, error, response } = await apiClient.GET("/api/v1/me");
  if (data) return data;
  throw ApiError.from(response, error);
}

/** POST /auth/verify-email: uses the token from a verification link. Fails with INVALID_TOKEN. */
export async function verifyEmail(token: string): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/auth/verify-email", { body: { token } });
  if (!response.ok) throw ApiError.from(response, error);
}

/** POST /auth/verify-email/resend: emails a new link. Fails with RATE_LIMITED after 3 in an hour. */
export async function resendVerification(): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/auth/verify-email/resend");
  if (!response.ok) throw ApiError.from(response, error);
}

/** POST /auth/password/forgot: always succeeds for a valid email, whether or not it has an account. */
export async function forgotPassword(email: string): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/auth/password/forgot", { body: { email } });
  if (!response.ok) throw ApiError.from(response, error);
}

/** POST /auth/password/reset: sets a new password with the token from a reset link. */
export async function resetPassword(body: ResetPasswordRequest): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/auth/password/reset", { body });
  if (!response.ok) throw ApiError.from(response, error);
}

/** POST /auth/email/confirm-change: switches the account to its new email. Fails with INVALID_TOKEN or EMAIL_TAKEN. */
export async function confirmEmailChange(token: string): Promise<void> {
  const { error, response } = await apiClient.POST("/api/v1/auth/email/confirm-change", { body: { token } });
  if (!response.ok) throw ApiError.from(response, error);
}
