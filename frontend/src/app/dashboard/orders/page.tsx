"use client";

import { useState } from "react";
import Link from "next/link";
import { StatusBadge } from "@/components/ui/StatusIndicators";
import { useAuth } from "@/lib/auth";
import { MOCK_ORDERS } from "@/lib/mock-data";
import { ordersApi } from "@/lib/api";
import type { Order, OrderStatus, OrderFilters } from "@/lib/types";
import { STATUS_LABELS, ORDER_STATUS_SEQUENCE } from "@/lib/types";

export default function OrdersPage() {
  const { hasRole, isMockMode } = useAuth();
  const [orders, setOrders] = useState<Order[]>(isMockMode ? MOCK_ORDERS : []);
  const [loading, setLoading] = useState(!isMockMode);
  const [filters, setFilters] = useState<OrderFilters>({});

  useState(() => {
    if (!isMockMode) {
      ordersApi.list(filters).then(setOrders).catch(console.error).finally(() => setLoading(false));
    }
  });

  const filteredOrders = isMockMode
    ? orders.filter((o) => {
        if (filters.invoice && !o.invoiceNumber.toLowerCase().includes(filters.invoice.toLowerCase())) return false;
        if (filters.customer && !o.customerNumber.toLowerCase().includes(filters.customer.toLowerCase()) && !o.customerName.toLowerCase().includes(filters.customer.toLowerCase())) return false;
        if (filters.status && o.status !== filters.status) return false;
        return true;
      })
    : orders;

  async function handleDelete(id: number) {
    if (!confirm("Are you sure you want to delete this order?")) return;
    if (isMockMode) {
      setOrders((prev) => prev.filter((o) => o.id !== id));
    } else {
      await ordersApi.delete(id);
      setOrders((prev) => prev.filter((o) => o.id !== id));
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-text-primary">Active Orders</h1>
          <p className="text-[13px] text-text-muted mt-1">
            Manage and track {filteredOrders.length} active order{filteredOrders.length !== 1 ? "s" : ""}
          </p>
        </div>
        {hasRole("SALES") && (
          <Link href="/dashboard/orders/new" className="btn-primary">
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            New Order
          </Link>
        )}
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
            <label className="field-label">Customer</label>
            <input
              type="text"
              placeholder="Search by customer..."
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

      {/* Table */}
      {loading ? (
        <div className="flex items-center justify-center py-24">
          <div className="w-8 h-8 rounded-full border-2 border-accent-500 border-t-transparent animate-spin" />
        </div>
      ) : filteredOrders.length === 0 ? (
        <div className="panel p-16 text-center">
          <svg className="w-12 h-12 mx-auto text-text-muted mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <p className="text-text-secondary text-sm">No orders found matching your filters</p>
        </div>
      ) : (
        <div className="panel overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr>
                  <th className="table-header">Invoice</th>
                  <th className="table-header">Customer</th>
                  <th className="table-header hidden md:table-cell">Date</th>
                  <th className="table-header">Status</th>
                  <th className="table-header text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[rgba(255,255,255,0.05)]">
                {filteredOrders.map((order) => (
                  <tr key={order.id} className="table-row">
                    <td className="table-cell">
                      <span className="font-mono text-[13px] text-accent-400 font-medium">{order.invoiceNumber}</span>
                    </td>
                    <td className="table-cell">
                      <p className="text-[14px] text-text-primary font-medium">{order.customerName}</p>
                      <p className="text-[12px] text-text-muted mt-0.5">{order.customerNumber}</p>
                    </td>
                    <td className="table-cell hidden md:table-cell">
                      <span className="text-[13px] text-text-secondary">
                        {new Date(order.createdAt).toLocaleDateString("es-MX", { day: "2-digit", month: "short", year: "numeric" })}
                      </span>
                    </td>
                    <td className="table-cell">
                      <StatusBadge status={order.status} />
                    </td>
                    <td className="table-cell text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Link href={`/dashboard/orders/${order.id}`} className="btn-icon" title="View details">
                          <svg className="w-[18px] h-[18px]" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                            <path strokeLinecap="round" strokeLinejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z" />
                            <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                          </svg>
                        </Link>
                        {hasRole("ADMIN", "SALES") && (
                          <button onClick={() => handleDelete(order.id)} className="btn-icon btn-icon-danger" title="Delete">
                            <svg className="w-[18px] h-[18px]" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                              <path strokeLinecap="round" strokeLinejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                            </svg>
                          </button>
                        )}
                      </div>
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
