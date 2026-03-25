"use client";

// ============================================================
// Halcon Platform — Auth Provider
// ============================================================
// Provides JWT-based authentication context with a mock mode
// for development while the .NET backend is not yet available.
//
// SWITCHING FROM MOCK TO REAL API:
// 1. Set NEXT_PUBLIC_MOCK_AUTH=false (or remove the env var)
// 2. Set NEXT_PUBLIC_API_URL to the real .NET API base URL
// 3. Restart the dev server / rebuild Docker image
// ============================================================

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";
import type { User, UserRole, DecodedToken, LoginPayload } from "./types";
import { authApi } from "./api";

// --- Mock Data ---

const MOCK_USERS: Record<string, { password: string; user: User }> = {
  admin: {
    password: "admin",
    user: { id: 1, username: "admin", role: "ADMIN" },
  },
  sales: {
    password: "sales",
    user: { id: 2, username: "sales", role: "SALES" },
  },
  purchasing: {
    password: "purchasing",
    user: { id: 3, username: "purchasing", role: "PURCHASING" },
  },
  warehouse: {
    password: "warehouse",
    user: { id: 4, username: "warehouse", role: "WAREHOUSE" },
  },
  route: {
    password: "route",
    user: { id: 5, username: "route", role: "ROUTE" },
  },
};

const IS_MOCK = process.env.NEXT_PUBLIC_MOCK_AUTH === "true";

// --- Context ---

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isMockMode: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  logout: () => void;
  hasRole: (...roles: UserRole[]) => boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

// --- Helpers ---

function decodeJwt(token: string): DecodedToken | null {
  try {
    const payload = token.split(".")[1];
    const decoded = JSON.parse(atob(payload));
    return decoded;
  } catch {
    return null;
  }
}

function isTokenExpired(decoded: DecodedToken): boolean {
  return decoded.exp * 1000 < Date.now();
}

// --- Provider ---

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Restore session on mount
  useEffect(() => {
    if (IS_MOCK) {
      const stored = localStorage.getItem("halcon_mock_user");
      if (stored) {
        try {
          setUser(JSON.parse(stored));
        } catch {
          localStorage.removeItem("halcon_mock_user");
        }
      }
    } else {
      const token = localStorage.getItem("halcon_token");
      if (token) {
        const decoded = decodeJwt(token);
        if (decoded && !isTokenExpired(decoded)) {
          setUser({
            id: parseInt(decoded.sub),
            username: decoded.username,
            role: decoded.role,
          });
        } else {
          localStorage.removeItem("halcon_token");
        }
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (payload: LoginPayload) => {
    if (IS_MOCK) {
      const mockEntry = MOCK_USERS[payload.username];
      if (!mockEntry || mockEntry.password !== payload.password) {
        throw { message: "Invalid credentials", statusCode: 401 };
      }
      setUser(mockEntry.user);
      localStorage.setItem("halcon_mock_user", JSON.stringify(mockEntry.user));
      return;
    }

    const { token } = await authApi.login(payload);
    localStorage.setItem("halcon_token", token);
    const decoded = decodeJwt(token);
    if (!decoded) throw { message: "Invalid token received", statusCode: 500 };
    setUser({
      id: parseInt(decoded.sub),
      username: decoded.username,
      role: decoded.role,
    });
  }, []);

  const logout = useCallback(() => {
    setUser(null);
    localStorage.removeItem("halcon_token");
    localStorage.removeItem("halcon_mock_user");
  }, []);

  const hasRole = useCallback(
    (...roles: UserRole[]) => {
      if (!user) return false;
      return roles.includes(user.role);
    },
    [user]
  );

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        isMockMode: IS_MOCK,
        login,
        logout,
        hasRole,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

// --- Hook ---

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
