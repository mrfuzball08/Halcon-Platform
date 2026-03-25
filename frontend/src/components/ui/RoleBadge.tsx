"use client";

import { ROLE_LABELS } from "@/lib/types";
import type { UserRole } from "@/lib/types";

const ROLE_STYLES: Record<UserRole, string> = {
  ADMIN: "bg-purple-500/15 text-purple-400 border-purple-500/30",
  SALES: "bg-blue-500/15 text-blue-400 border-blue-500/30",
  PURCHASING: "bg-amber-500/15 text-amber-400 border-amber-500/30",
  WAREHOUSE: "bg-teal-500/15 text-teal-400 border-teal-500/30",
  ROUTE: "bg-orange-500/15 text-orange-400 border-orange-500/30",
};

export function RoleBadge({ role }: { role: UserRole }) {
  return (
    <span
      className={`inline-flex items-center px-2.5 py-1 text-xs font-semibold rounded-full border ${ROLE_STYLES[role]}`}
    >
      {ROLE_LABELS[role]}
    </span>
  );
}
