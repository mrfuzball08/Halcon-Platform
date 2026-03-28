"use client";

import { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth";
import { ordersApi } from "@/lib/api";
import { StatusBadge } from "@/components/ui/StatusIndicators";
import { MOCK_DELETED_ORDERS } from "@/lib/mock-data";
import type { Order, OrderStatus } from "@/lib/types";
import { STATUS_LABELS, ORDER_STATUS_SEQUENCE } from "@/lib/types";

interface RecycleBinFilters {
  invoice?: string;
  customer?: string;
  status?: OrderStatus;
}

export default function RecycleBinPage() {
  const { hasRole, isMockMode } = useAuth();
  const [orders, setOrders] = useState<Order[]>(isMockMode ? MOCK_DELETED_ORDERS : []);
  const [loading, setLoading] = useState(!isMockMode);
  const [filters, setFilters] = useState<RecycleBinFilters>({});

  useEffect(() => {
    if (!isMockMode) {
      ordersApi.listDeleted().then(setOrders).catch(console.error).finally(() => setLoading(false));
    }
  }, [isMockMode]);

  // Client-side filtering (backend ListDeleted doesn't accept query params)
  const filteredOrders = orders.filter((o) => {
    if (filters.invoice && !o.invoiceNumber.toLowerCase().includes(filters.invoice.toLowerCase())) return false;
    if (filters.customer && !o.customerName.toLowerCase().includes(filters.customer.toLowerCase())) return false;
    if (filters.status && o.status !== filters.status) return false;
    return true;
  });

  if (!hasRole("ADMIN")) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="panel p-10 text-center max-w-sm">
          <p className="text-text-secondary text-sm">Access denied. Admin only.</p>
        </div>
      </div>
    );
  }

  async function handleRestore(id: number) {
    if (isMockMode) {
      setOrders((prev) => prev.filter((o) => o.id !== id));
    } else {
      await ordersApi.restore(id);
      setOrders((prev) => prev.filter((o) => o.id !== id));
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-text-primary">Recycle Bin</h1>
        <p className="text-[13px] text-text-muted mt-1">
          {filteredOrders.length} soft-deleted order{filteredOrders.length !== 1 ? "s" : ""}
        </p>
      </div>

      {/* Filters */}
      <div className="panel p-5">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label className="field-label">Invoice</label>
            <input
              type="text"
              placeholder="Search by invoice #..."
              value={filters.invoice ?? ""}
              onChange={(e) => setFilters((f) => ({ ...f, invoice: e.target.value || undefined }))}
              className="input-field"
            />
          </div>
          <div>
            <label className="field-label">Customer Name</label>
            <input
              type="text"
              placeholder="Search by customer name..."
              value={filters.customer ?? ""}
              onChange={(e) => setFilters((f) => ({ ...f, customer: e.target.value || undefined }))}
              className="input-field"
            />
          </div>
          <div>
            <label className="field-label">Status</label>
            <select
              value={filters.status ?? ""}
              onChange={(e) => setFilters((f) => ({ ...f, status: (e.target.value as OrderStatus) || undefined }))}
              className="input-field"
            >
              <option value="">All Statuses</option>
              {ORDER_STATUS_SEQUENCE.map((s) => (
                <option key={s} value={s}>{STATUS_LABELS[s]}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-24">
          <div className="w-8 h-8 rounded-full border-2 border-accent-500 border-t-transparent animate-spin" />
        </div>
      ) : filteredOrders.length === 0 ? (
        <div className="panel p-16 text-center">
          <svg className="w-12 h-12 mx-auto text-text-muted mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
          </svg>
          <p className="text-text-secondary text-sm">
            {orders.length === 0 ? "Recycle bin is empty" : "No deleted orders match your filters"}
          </p>
        </div>
      ) : (
        <div className="panel overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr>
                  <th className="table-header">Invoice</th>
                  <th className="table-header">Customer</th>
                  <th className="table-header hidden md:table-cell">Deleted</th>
                  <th className="table-header">Status</th>
                  <th className="table-header text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[rgba(255,255,255,0.05)]">
                {filteredOrders.map((order) => (
                  <tr key={order.id} className="table-row opacity-70 hover:opacity-100">
                    <td className="table-cell font-mono text-[13px] text-accent-400">{order.invoiceNumber}</td>
                    <td className="table-cell">
                      <p className="text-[14px] text-text-primary font-medium">{order.customerName}</p>
                      <p className="text-[12px] text-text-muted mt-0.5">{order.customerNumber}</p>
                    </td>
                    <td className="table-cell hidden md:table-cell">
                      <span className="text-[13px] text-text-secondary">
                        {new Date(order.updatedAt).toLocaleDateString("es-MX", { day: "2-digit", month: "short", year: "numeric" })}
                      </span>
                    </td>
                    <td className="table-cell"><StatusBadge status={order.status} /></td>
                    <td className="table-cell text-right">
                      <button onClick={() => handleRestore(order.id)}
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-[12px] font-semibold text-status-delivered bg-status-delivered/10 hover:bg-status-delivered/15 border border-status-delivered/20 transition-colors">
                        <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                          <path strokeLinecap="round" strokeLinejoin="round" d="M9 15L3 9m0 0l6-6M3 9h12a6 6 0 010 12h-3" />
                        </svg>
                        Restore
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
