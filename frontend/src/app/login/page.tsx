"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth";
import { Logo } from "@/components/ui/Logo";

export default function LoginPage() {
  const { login, isAuthenticated, isMockMode } = useAuth();
  const router = useRouter();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (isAuthenticated) {
      router.replace("/dashboard/orders");
    }
  }, [isAuthenticated, router]);

  if (isAuthenticated) return null;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await login({ username, password });
      router.push("/dashboard/orders");
    } catch (err: unknown) {
      const apiErr = err as { message?: string };
      setError(apiErr?.message || "Invalid credentials");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-surface-0">
      <div className="w-full max-w-[380px] animate-fade-in">
        {/* Logo */}
        <div className="text-center mb-8">
          <Logo className="w-14 h-14 text-accent-500 mx-auto mb-5" />
          <h1 className="text-2xl font-semibold text-text-primary tracking-tight">Welcome back</h1>
          <p className="text-[14px] text-text-muted mt-1.5">Sign in to Halcon Platform</p>
        </div>

        {/* Form */}
        <div className="panel p-7">
          <form onSubmit={handleSubmit} className="space-y-5">
            {error && (
              <div className="px-4 py-3 rounded-xl bg-danger-500/8 border border-danger-500/20 text-[13px] text-danger-500 animate-fade-in">
                {error}
              </div>
            )}

            <div>
              <label htmlFor="login-username" className="field-label">Username</label>
              <input
                id="login-username"
                type="text"
                required
                autoFocus
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Enter your username"
                className="input-field"
              />
            </div>

            <div>
              <label htmlFor="login-password" className="field-label">Password</label>
              <input
                id="login-password"
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter your password"
                className="input-field"
              />
            </div>

            <button type="submit" disabled={isLoading} className="btn-primary w-full mt-1">
              {isLoading ? (
                <span className="flex items-center justify-center gap-2">
                  <span className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                  Signing in...
                </span>
              ) : (
                "Sign In"
              )}
            </button>
          </form>

          {/* Mock Credentials */}
          {isMockMode && (
            <div className="mt-6 pt-5 border-t border-[rgba(255,255,255,0.08)]">
              <p className="text-[11px] font-semibold text-text-muted mb-3 uppercase tracking-widest">Dev Credentials</p>
              <div className="grid grid-cols-2 gap-2">
                {[
                  { user: "admin", role: "Admin" },
                  { user: "sales", role: "Sales" },
                  { user: "warehouse", role: "Warehouse" },
                  { user: "route", role: "Route" },
                ].map(({ user, role }) => (
                  <button
                    key={user}
                    type="button"
                    onClick={() => { setUsername(user); setPassword(user); }}
                    className="px-3 py-2.5 rounded-xl bg-surface-0 border border-[rgba(255,255,255,0.12)] hover:border-[rgba(255,255,255,0.2)] text-[12px] text-text-secondary hover:text-text-primary transition-all text-left"
                  >
                    <span className="font-medium">{role}</span>
                    <span className="text-text-muted block text-[11px] mt-0.5">{user} / {user}</span>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="text-center mt-6">
          <a href="/" className="text-[12px] text-text-muted hover:text-accent-400 transition-colors">
            Track an order (public portal) &rarr;
          </a>
        </div>
      </div>
    </div>
  );
}
