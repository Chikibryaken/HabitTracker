import { create } from "zustand";
import { decodeJwtPayload } from "../api/jwt";
import type { AuthResponse, User } from "../types/auth";

const ACCESS_TOKEN_KEY = "habittracker.accessToken";
const REFRESH_TOKEN_KEY = "habittracker.refreshToken";
const USER_KEY = "habittracker.user";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: User | null;
  isAuthenticated: boolean;
  setAuth: (response: AuthResponse, email: string) => void;
  clearAuth: () => void;
}

function loadStoredUser(): User | null {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as User;
  } catch {
    return null;
  }
}

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: localStorage.getItem(ACCESS_TOKEN_KEY),
  refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY),
  user: loadStoredUser(),
  isAuthenticated: localStorage.getItem(ACCESS_TOKEN_KEY) !== null,

  setAuth: (response, email) => {
    const payload = decodeJwtPayload(response.accessToken);
    const user: User = { id: payload.sub, email };

    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));

    set({
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      user,
      isAuthenticated: true,
    });
  },

  clearAuth: () => {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);

    set({
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
    });
  },
}));
