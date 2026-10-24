import { apiClient } from "@/lib/api/client";
import { ApiError } from "@/lib/api/errors";
import type { ApiSchema } from "@/lib/api/types";

export type UserProfile = ApiSchema<"UserProfile">;
export type AuthResponse = ApiSchema<"AuthResponse">;
export type SignupRequest = ApiSchema<"SignupRequest">;
export type LoginRequest = ApiSchema<"LoginRequest">;
export type ExamTrack = ApiSchema<"ExamTrack">;

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
