"use client";

import { useState, useEffect, use } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth";
import { ordersApi } from "@/lib/api";
import { StatusBadge, StatusProgressBar } from "@/components/ui/StatusIndicators";
import { PhotoUpload } from "@/components/ui/PhotoUpload";
import { Modal } from "@/components/ui/Modal";
import { MOCK_ORDERS } from "@/lib/mock-data";
import type { Order, OrderStatus, OrderUpdatePayload } from "@/lib/types";
import { ORDER_STATUS_SEQUENCE, STATUS_LABELS } from "@/lib/types";

export default function OrderDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { hasRole, isMockMode } = useAuth();
  const router = useRouter();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [editForm, setEditForm] = useState<OrderUpdatePayload>({});

  useEffect(() => {
    if (isMockMode) {
      const found = MOCK_ORDERS.find((o) => o.id === parseInt(id));
      setOrder(found ?? null);
      setLoading(false);
    } else {
      ordersApi.get(parseInt(id)).then(setOrder).catch(console.error).finally(() => setLoading(false));
    }
  }, [id, isMockMode]);

  async function handleStatusAdvance() {
    if (!order) return;
    const currentIdx = ORDER_STATUS_SEQUENCE.indexOf(order.status);
    const nextStatus = ORDER_STATUS_SEQUENCE[currentIdx + 1];
    if (!nextStatus) return;

    if (nextStatus === "DELIVERED" && !order.deliveryPhotoUrl) {
      alert("Delivery photo must be uploaded before marking as Delivered.");
      return;
    }

    if (isMockMode) {
      setOrder({ ...order, status: nextStatus, updatedAt: new Date().toISOString() });
    } else {
      const updated = await ordersApi.updateStatus(order.id, nextStatus);
      setOrder(updated);
    }
  }

  async function handleEditSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!order) return;
    setIsSubmitting(true);
    
    try {
      if (isMockMode) {
        setOrder({ ...order, ...editForm, updatedAt: new Date().toISOString() });
      } else {
        const updated = await ordersApi.update(order.id, editForm);
        setOrder(updated);
      }
      setIsEditing(false);
    } catch (err) {
      alert("Failed to update order");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handlePhotoUpload(type: "loading" | "delivery", file: File) {
    if (!order) return;
    if (isMockMode) {
      const url = URL.createObjectURL(file);
      setOrder({
        ...order,
        ...(type === "loading" ? { loadingPhotoUrl: url } : { deliveryPhotoUrl: url }),
      });
    } else {
      const updated = type === "loading"
        ? await ordersApi.uploadLoadingPhoto(order.id, file)
        : await ordersApi.uploadDeliveryPhoto(order.id, file);
      setOrder(updated);
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <div className="w-8 h-8 rounded-full border-2 border-accent-500 border-t-transparent animate-spin" />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="flex flex-col items-center justify-center py-24 gap-4">
        <p className="text-text-secondary text-sm">Order not found</p>
        <button onClick={() => router.back()} className="btn-secondary">Go Back</button>
      </div>
    );
  }

  const currentStatusIdx = ORDER_STATUS_SEQUENCE.indexOf(order.status);
  const nextStatus: OrderStatus | undefined = ORDER_STATUS_SEQUENCE[currentStatusIdx + 1];
  const canAdvanceStatus =
    hasRole("ADMIN", "PURCHASING") ||
    (hasRole("WAREHOUSE") && (order.status === "ORDERED" || order.status === "IN_PROCESS")) ||
    (hasRole("ROUTE") && order.status === "IN_ROUTE");

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <button onClick={() => router.back()} className="btn-icon">
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
            </svg>
          </button>
          <div>
            <h1 className="text-2xl font-semibold text-text-primary font-mono">{order.invoiceNumber}</h1>
            <p className="text-[13px] text-text-muted mt-0.5">{order.customerName}</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <StatusBadge status={order.status} />
          {hasRole("SALES", "ADMIN") && (
            <button
              onClick={() => {
                setEditForm({
                  invoiceNumber: order.invoiceNumber,
                  customerNumber: order.customerNumber,
                  customerName: order.customerName,
                  fiscalData: order.fiscalData,
                  deliveryAddress: order.deliveryAddress,
                  notes: order.notes,
                });
                setIsEditing(true);
              }}
              className="btn-secondary px-3 py-1.5 text-xs"
            >
              <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
              </svg>
              Edit
            </button>
          )}
        </div>
      </div>

      {/* Progress Bar */}
      <div className="panel p-7">
        <StatusProgressBar status={order.status} />
      </div>

      {/* Details Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="panel p-7 space-y-5">
          <h2 className="text-[15px] font-semibold text-text-primary">Order Information</h2>
          <div className="space-y-4">
            <InfoRow label="Invoice #" value={order.invoiceNumber} mono />
            <InfoRow label="Customer #" value={order.customerNumber} mono />
            <InfoRow label="Customer Name" value={order.customerName} />
            <InfoRow label="Delivery Address" value={order.deliveryAddress} />
            <InfoRow label="Created" value={new Date(order.createdAt).toLocaleString("es-MX")} />
            <InfoRow label="Last Updated" value={new Date(order.updatedAt).toLocaleString("es-MX")} />
          </div>
        </div>

        <div className="space-y-6">
          {order.fiscalData && (
            <div className="panel p-7 space-y-3">
              <h2 className="text-[15px] font-semibold text-text-primary">Fiscal Data</h2>
              <pre className="text-[13px] text-text-secondary whitespace-pre-wrap font-mono bg-surface-0 rounded-xl p-4 border border-[rgba(255,255,255,0.1)]">
                {order.fiscalData}
              </pre>
            </div>
          )}
          {order.notes && (
            <div className="panel p-7 space-y-3">
              <h2 className="text-[15px] font-semibold text-text-primary">Notes</h2>
              <p className="text-[14px] text-text-secondary leading-relaxed">{order.notes}</p>
            </div>
          )}
        </div>
      </div>

      {/* Photo Evidence */}
      <div className="panel p-7 space-y-5">
        <h2 className="text-[15px] font-semibold text-text-primary">Photo Evidence</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
          <PhotoUpload label="Loading Photo" existingUrl={order.loadingPhotoUrl}
            onUpload={(file) => handlePhotoUpload("loading", file)}
            disabled={!hasRole("ROUTE") || order.status === "DELIVERED"} />
          <PhotoUpload label="Delivery Photo" existingUrl={order.deliveryPhotoUrl}
            onUpload={(file) => handlePhotoUpload("delivery", file)}
            disabled={!hasRole("ROUTE") || order.status === "DELIVERED"} />
        </div>
      </div>

      {/* Status Transition */}
      {canAdvanceStatus && nextStatus && (
        <div className="panel p-7">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div>
              <h2 className="text-[15px] font-semibold text-text-primary">Advance Status</h2>
              <p className="text-[13px] text-text-muted mt-1.5">
                Move from <strong className="text-text-secondary">{STATUS_LABELS[order.status]}</strong> to{" "}
                <strong className="text-text-secondary">{STATUS_LABELS[nextStatus]}</strong>
              </p>
              {nextStatus === "DELIVERED" && !order.deliveryPhotoUrl && (
                <p className="text-[12px] text-status-in-process mt-2">⚠ Upload delivery photo before advancing.</p>
              )}
            </div>
            <button onClick={handleStatusAdvance}
              disabled={nextStatus === "DELIVERED" && !order.deliveryPhotoUrl} className="btn-primary shrink-0">
              Advance to {STATUS_LABELS[nextStatus]}
            </button>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      <Modal isOpen={isEditing} onClose={() => setIsEditing(false)} title="Edit Order Information">
        <form onSubmit={handleEditSubmit} className="space-y-5">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="field-label">Invoice Number</label>
              <input type="text" required value={editForm.invoiceNumber ?? ""}
                onChange={(e) => setEditForm(f => ({ ...f, invoiceNumber: e.target.value }))} className="input-field" />
            </div>
            <div>
              <label className="field-label">Customer Number</label>
              <input type="text" required value={editForm.customerNumber ?? ""}
                onChange={(e) => setEditForm(f => ({ ...f, customerNumber: e.target.value }))} className="input-field" />
            </div>
          </div>
          <div>
            <label className="field-label">Customer / Company Name</label>
            <input type="text" required value={editForm.customerName ?? ""}
              onChange={(e) => setEditForm(f => ({ ...f, customerName: e.target.value }))} className="input-field" />
          </div>
          <div>
            <label className="field-label">Fiscal Data</label>
            <textarea value={editForm.fiscalData ?? ""} rows={2}
              onChange={(e) => setEditForm(f => ({ ...f, fiscalData: e.target.value }))} className="input-field resize-none" />
          </div>
          <div>
            <label className="field-label">Delivery Address</label>
            <textarea required value={editForm.deliveryAddress ?? ""} rows={2}
              onChange={(e) => setEditForm(f => ({ ...f, deliveryAddress: e.target.value }))} className="input-field resize-none" />
          </div>
          <div>
            <label className="field-label">Notes</label>
            <textarea value={editForm.notes ?? ""} rows={2}
              onChange={(e) => setEditForm(f => ({ ...f, notes: e.target.value }))} className="input-field resize-none" />
          </div>

          <div className="flex justify-end gap-3 pt-3 border-t border-[rgba(255,255,255,0.08)]">
            <button type="button" onClick={() => setIsEditing(false)} className="btn-secondary">Cancel</button>
            <button type="submit" disabled={isSubmitting} className="btn-primary">
              {isSubmitting ? "Saving..." : "Save Changes"}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

function InfoRow({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-start gap-1 sm:gap-0">
      <span className="text-[11px] font-semibold text-text-muted uppercase tracking-wider sm:w-36 shrink-0 sm:pt-0.5">{label}</span>
      <span className={`text-[14px] text-text-primary leading-relaxed ${mono ? "font-mono" : ""}`}>{value}</span>
    </div>
  );
}
