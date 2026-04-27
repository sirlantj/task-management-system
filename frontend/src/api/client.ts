import type { Task, CreateTaskPayload, UpdateTaskPayload } from '../types/task';

export interface AuthResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
}

export interface MeResponse {
  id: string;
  name: string;
  email: string;
}

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

export const TOKEN_KEY = 'auth_token';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = localStorage.getItem(TOKEN_KEY);

  const response = await fetch(`/api${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options?.headers,
    },
  });

  if (response.status === 204) return undefined as T;

  const data: unknown = await response.json().catch(() => null);

  if (!response.ok) {
    const message =
      data && typeof data === 'object' && 'error' in data
        ? String((data as { error: unknown }).error)
        : 'An unexpected error occurred.';
    throw new ApiError(response.status, message);
  }

  return data as T;
}

export const authApi = {
  register: (name: string, email: string, password: string) =>
    request<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ name, email, password }),
    }),

  login: (email: string, password: string) =>
    request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  me: () => request<MeResponse>('/auth/me'),
};

export const tasksApi = {
  getAll: () => request<Task[]>('/tasks'),

  getById: (id: string) => request<Task>(`/tasks/${id}`),

  create: (payload: CreateTaskPayload) =>
    request<Task>('/tasks', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateTaskPayload) =>
    request<Task>(`/tasks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }),

  delete: (id: string) => request<void>(`/tasks/${id}`, { method: 'DELETE' }),
};
