import { apiClient } from "./client";
import type { AuthResponse, UserProfile } from "../types/auth";

export interface Credentials {
  email: string;
  password: string;
}

export async function registerRequest(credentials: Credentials): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>("/auth/register", credentials);
  return response.data;
}

export async function loginRequest(credentials: Credentials): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>("/auth/login", credentials);
  return response.data;
}

export async function logoutRequest(refreshToken: string): Promise<void> {
  await apiClient.post("/auth/logout", { refreshToken });
}

export async function getMe(): Promise<UserProfile> {
  const response = await apiClient.get<UserProfile>("/auth/me");
  return response.data;
}
