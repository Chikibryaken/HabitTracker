export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface User {
  id: string;
  email: string;
}

export interface ApiError {
  message: string;
  fieldErrors?: Record<string, string[]>;
}

export interface UserProfile {
  id: string;
  email: string;
  createdAt: string;
  habitCount: number;
}
