import { AxiosError } from "axios";
import type { ApiError } from "../types/auth";

interface ValidationProblemBody {
  title?: string;
  errors?: Record<string, string[]>;
}

interface SimpleErrorBody {
  error?: string;
}

type KnownErrorBody = ValidationProblemBody & SimpleErrorBody;

export function parseApiError(error: unknown): ApiError {
  if (!(error instanceof AxiosError)) {
    return { message: "Something went wrong. Please try again." };
  }

  const data = error.response?.data as KnownErrorBody | undefined;

  if (data?.errors) {
    const firstMessage = Object.values(data.errors)[0]?.[0];
    return {
      message: firstMessage ?? data.title ?? "Validation failed.",
      fieldErrors: data.errors,
    };
  }

  if (data?.error) {
    return { message: data.error };
  }

  if (error.response?.status === 401) {
    return { message: "Invalid email or password." };
  }

  return { message: "Something went wrong. Please try again." };
}
