// ============================================================
// Halcon Platform — Shared TypeScript Types
// ============================================================

// --- Enums ---

export type UserRole = "ADMIN" | "SALES" | "PURCHASING" | "WAREHOUSE" | "ROUTE";

export type OrderStatus = "ORDERED" | "IN_PROCESS" | "IN_ROUTE" | "DELIVERED";

// --- Models ---

export interface User {
  id: number;
  authUserId?: string;
  username: string;
  email?: string;
  role: UserRole;
}

export interface UserCreatePayload {
  username: string;
  email?: string;
  password: string;
  role: UserRole;
}

export interface UserUpdatePayload {
  username?: string;
  email?: string;
  password?: string;
  role?: UserRole;
}

export interface Order {
  id: number;
  invoiceNumber: string;
  customerNumber: string;
  customerName: string;
  fiscalData: string;
  deliveryAddress: string;
  notes: string;
  status: OrderStatus;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
  loadingPhotoUrl: string | null;
  deliveryPhotoUrl: string | null;
}

export interface OrderCreatePayload {
  invoiceNumber: string;
  customerNumber: string;
  customerName: string;
  fiscalData: string;
  deliveryAddress: string;
  notes: string;
}

export interface OrderUpdatePayload {
  invoiceNumber?: string;
  customerNumber?: string;
  customerName?: string;
  fiscalData?: string;
  deliveryAddress?: string;
  notes?: string;
}

export interface OrderFilters {
  invoice?: string;
  customer?: string;
  date?: string;
  status?: OrderStatus;
}

// --- Auth ---

export interface LoginPayload {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  role: UserRole;
}

export interface DecodedToken {
  sub: string;
  username?: string;
  role?: string;
  email?: string;
  user_metadata?: {
    username?: string;
    role?: UserRole;
  };
  app_metadata?: {
    role?: string;
  };
  exp: number;
  iat?: number;
}

// --- Public ---

export interface PublicOrderTrack {
  invoiceNumber: string;
  customerName: string;
  status: OrderStatus;
  deliveryPhotoUrl: string | null;
}

// --- API ---

export interface ApiError {
  message: string;
  statusCode: number;
}

// --- UI Helpers ---

export const ORDER_STATUS_SEQUENCE: OrderStatus[] = [
  "ORDERED",
  "IN_PROCESS",
  "IN_ROUTE",
  "DELIVERED",
];

export const STATUS_LABELS: Record<OrderStatus, string> = {
  ORDERED: "Ordered",
  IN_PROCESS: "In Process",
  IN_ROUTE: "In Route",
  DELIVERED: "Delivered",
};

export const STATUS_COLORS: Record<OrderStatus, string> = {
  ORDERED: "#3B82F6",    // Blue
  IN_PROCESS: "#EAB308", // Yellow
  IN_ROUTE: "#F97316",   // Orange
  DELIVERED: "#22C55E",  // Green
};

export const ROLE_LABELS: Record<UserRole, string> = {
  ADMIN: "Admin",
  SALES: "Sales",
  PURCHASING: "Purchasing",
  WAREHOUSE: "Warehouse",
  ROUTE: "Route",
};
