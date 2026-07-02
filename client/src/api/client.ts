import axios, { AxiosError } from "axios";
import type { AxiosResponse, InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "../store/authStore";
import type { AuthResponse } from "../types/auth";

const API_BASE_URL = import.meta.env.VITE_API_URL ?? "/api";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
});

apiClient.interceptors.request.use((config) => {
  const { accessToken } = useAuthStore.getState();
  if (accessToken) {
    config.headers.set("Authorization", `Bearer ${accessToken}`);
  }
  return config;
});

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

type RefreshSubscriber = (accessToken: string | null) => void;

let isRefreshing = false;
let refreshSubscribers: RefreshSubscriber[] = [];

function subscribeTokenRefresh(callback: RefreshSubscriber): void {
  refreshSubscribers.push(callback);
}

function notifySubscribers(accessToken: string | null): void {
  refreshSubscribers.forEach((callback) => callback(accessToken));
  refreshSubscribers = [];
}

function redirectToLogin(): void {
  useAuthStore.getState().clearAuth();
  window.location.href = "/login";
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined;

    if (error.response?.status !== 401 || !originalRequest || originalRequest._retry) {
      return Promise.reject(error);
    }

    const { refreshToken, user } = useAuthStore.getState();

    if (!refreshToken) {
      redirectToLogin();
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    if (isRefreshing) {
      return new Promise<AxiosResponse>((resolve, reject) => {
        subscribeTokenRefresh((accessToken) => {
          if (accessToken) {
            originalRequest.headers.set("Authorization", `Bearer ${accessToken}`);
            resolve(apiClient(originalRequest));
          } else {
            reject(error);
          }
        });
      });
    }

    isRefreshing = true;

    try {
      const { data } = await axios.post<AuthResponse>(`${API_BASE_URL}/auth/refresh`, { refreshToken });

      useAuthStore.getState().setAuth(data, user?.email ?? "");
      notifySubscribers(data.accessToken);

      originalRequest.headers.set("Authorization", `Bearer ${data.accessToken}`);
      return apiClient(originalRequest);
    } catch (refreshError) {
      notifySubscribers(null);
      redirectToLogin();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);
