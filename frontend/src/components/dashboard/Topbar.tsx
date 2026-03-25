"use client";

import { useAuth } from "@/lib/auth";
import { RoleBadge } from "@/components/ui/RoleBadge";

interface TopbarProps {
  onMenuToggle: () => void;
}

export function Topbar({ onMenuToggle }: TopbarProps) {
  const { user, logout, isMockMode } = useAuth();

  return (
    <header className="h-16 bg-surface-1 border-b border-[rgba(255,255,255,0.08)] flex items-center justify-between px-5 lg:px-8 sticky top-0 z-30">
      {/* Left */}
      <div className="flex items-center gap-3">
        <button
          onClick={onMenuToggle}
          className="btn-icon lg:hidden"
        >
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
          </svg>
        </button>

        {isMockMode && (
          <span className="hidden sm:inline-flex items-center gap-1.5 px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider rounded-md bg-amber-500/10 text-amber-400 border border-amber-500/20">
            <span className="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse" />
            Dev Mode
          </span>
        )}
      </div>

      {/* Right */}
      <div className="flex items-center gap-4">
        {user && (
          <>
            <RoleBadge role={user.role} />
            <div className="hidden sm:flex items-center gap-3">
              <span className="text-sm text-text-secondary">{user.username}</span>
              <div className="w-8 h-8 rounded-full bg-surface-3 flex items-center justify-center text-text-secondary text-xs font-semibold uppercase border border-[rgba(255,255,255,0.1)]">
                {user.username.charAt(0)}
              </div>
            </div>

            <div className="w-px h-6 bg-[rgba(255,255,255,0.08)] hidden sm:block" />

            <button
              onClick={logout}
              className="btn-icon btn-icon-danger"
              title="Sign out"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9" />
              </svg>
            </button>
          </>
        )}
      </div>
    </header>
  );
}
