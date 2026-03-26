// ============================================================
// Halcon Platform — API Client
// ============================================================
// Centralised HTTP client for the .NET backend API.
//
// SWITCHING FROM MOCK TO REAL API:
// 1. Set NEXT_PUBLIC_API_URL to your .NET API base URL (e.g. http://localhost:8080/api)
// 2. Set NEXT_PUBLIC_MOCK_AUTH=false (or remove it)
// 3. That's it — all endpoints are already wired up.
// ============================================================

import type {
  LoginPayload,
  LoginResponse,
  User,
  UserCreatePayload,
  UserUpdatePayload,
  Order,
  OrderCreatePayload,
  OrderUpdatePayload,
  OrderFilters,
  PublicOrderTrack,
} from "./types";

const TOKEN_STORAGE_KEY = "halcon_token";
const RAW_API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080/api";
const API_BASE = RAW_API_BASE.replace(/\/+$/, "");

// --- Helpers ---

function getAuthHeaders(): Record<string, string> {
  if (typeof window === "undefined") return {};
  const token = localStorage.getItem(TOKEN_STORAGE_KEY);
  return token ? { Authorization: `Bearer ${token}` } : {};
}

function normalizeUserCreatePayload(payload: UserCreatePayload): UserCreatePayload & { email: string } {
  if (payload.email?.trim()) {
    return { ...payload, email: payload.email.trim() };
  }

  const sanitizedLocalPart = payload.username
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9._-]+/g, "-")
    .replace(/^-+|-+$/g, "");

  const fallbackEmail = `${sanitizedLocalPart || "user"}@halcon.local`;
  return { ...payload, email: fallbackEmail };
}

function parseApiError(statusCode: number, bodyText: string): { message: string; statusCode: number } {
  if (!bodyText) {
    return {
      message: `Request failed with status ${statusCode}.`,
      statusCode,
    };
  }

  try {
    const parsed = JSON.parse(bodyText) as {
      message?: string;
      Message?: string;
      statusCode?: number;
      StatusCode?: number;
    };

    return {
      message: parsed.message ?? parsed.Message ?? bodyText,
      statusCode: parsed.statusCode ?? parsed.StatusCode ?? statusCode,
    };
  } catch {
    return {
      message: bodyText,
      statusCode,
    };
  }
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE}${endpoint}`;

  const headers: Record<string, string> = {
    Accept: "application/json",
    ...getAuthHeaders(),
    ...(options.headers as Record<string, string>),
  };

  // Don't set Content-Type for FormData (browser sets boundary automatically)
  if (!(options.body instanceof FormData)) {
    headers["Content-Type"] = "application/json";
  }

  const res = await fetch(url, { ...options, headers });

  if (!res.ok) {
    const errorBody = await res.text().catch(() => "");
    throw parseApiError(res.status, errorBody);
  }

  // Handle 204 No Content
  if (res.status === 204) return undefined as T;

  return res.json();
}

// --- Auth ---

export const authApi = {
  login: (payload: LoginPayload) =>
    request<LoginResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  seed: (seedToken: string) =>
    request<void>("/auth/seed", {
      method: "POST",
      headers: {
        "X-Seed-Token": seedToken,
      },
    }),
};

// --- Users ---

export const usersApi = {
  list: () => request<User[]>("/users"),

  get: (id: number) => request<User>(`/users/${id}`),

  create: (payload: UserCreatePayload) =>
    request<User>("/users", {
      method: "POST",
      body: JSON.stringify(normalizeUserCreatePayload(payload)),
    }),

  update: (id: number, payload: UserUpdatePayload) =>
    request<User>(`/users/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: number) =>
    request<void>(`/users/${id}`, { method: "DELETE" }),
};

// --- Orders ---

export const ordersApi = {
  list: (filters?: OrderFilters) => {
    const params = new URLSearchParams();
    if (filters?.invoice) params.set("invoice", filters.invoice);
    if (filters?.customer) params.set("customer", filters.customer);
    if (filters?.date) params.set("date", filters.date);
    if (filters?.status) params.set("status", filters.status);
    const qs = params.toString();
    return request<Order[]>(`/orders${qs ? `?${qs}` : ""}`);
  },

  get: (id: number) => request<Order>(`/orders/${id}`),

  create: (payload: OrderCreatePayload) =>
    request<Order>("/orders", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: number, payload: OrderUpdatePayload) =>
    request<Order>(`/orders/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  updateStatus: (id: number, status: Order["status"]) =>
    request<Order>(`/orders/${id}/status`, {
      method: "PATCH",
      body: JSON.stringify({ status }),
    }),

  delete: (id: number) =>
    request<void>(`/orders/${id}`, { method: "DELETE" }),

  restore: (id: number) =>
    request<Order>(`/orders/${id}/restore`, { method: "PATCH" }),

  listDeleted: () => request<Order[]>("/orders/deleted"),

  uploadLoadingPhoto: (id: number, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return request<Order>(`/orders/${id}/photos/loading`, {
      method: "POST",
      body: form,
    });
  },

  uploadDeliveryPhoto: (id: number, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return request<Order>(`/orders/${id}/photos/delivery`, {
      method: "POST",
      body: form,
    });
  },
};

// --- Public ---

export const publicApi = {
  trackOrder: (invoice: string, customer: string) =>
    request<PublicOrderTrack>(
      `/public/orders/track?invoice=${encodeURIComponent(invoice)}&customer=${encodeURIComponent(customer)}`
    ),
};
