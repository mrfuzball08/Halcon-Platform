"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth";
import { ordersApi } from "@/lib/api";
import type { OrderCreatePayload } from "@/lib/types";

export default function NewOrderPage() {
  const { hasRole, isMockMode } = useAuth();
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<OrderCreatePayload>({
    invoiceNumber: "",
    customerNumber: "",
    customerName: "",
    fiscalData: "",
    deliveryAddress: "",
    notes: "",
  });

  if (!hasRole("SALES", "ADMIN")) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="panel p-10 text-center max-w-sm">
          <p className="text-text-secondary text-sm">Access denied. Only Sales can create orders.</p>
        </div>
      </div>
    );
  }

  function updateField(field: keyof OrderCreatePayload, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      if (isMockMode) {
        await new Promise((r) => setTimeout(r, 500));
        router.push("/dashboard/orders");
      } else {
        await ordersApi.create(form);
        router.push("/dashboard/orders");
      }
    } catch (err: unknown) {
      const apiErr = err as { message?: string };
      setError(apiErr?.message || "Failed to create order");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-text-primary">New Order</h1>
        <p className="text-[13px] text-text-muted mt-1">
          Create a new order. Status will be set to &ldquo;Ordered&rdquo; automatically.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="panel p-7 space-y-6">
        {error && (
          <div className="px-4 py-3 rounded-xl bg-danger-500/8 border border-danger-500/20 text-[13px] text-danger-500">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
          <div>
            <label htmlFor="invoiceNumber" className="field-label">Invoice Number *</label>
            <input id="invoiceNumber" type="text" required value={form.invoiceNumber}
              onChange={(e) => updateField("invoiceNumber", e.target.value)} placeholder="INV-001" className="input-field" />
          </div>
          <div>
            <label htmlFor="customerNumber" className="field-label">Customer Number *</label>
            <input id="customerNumber" type="text" required value={form.customerNumber}
              onChange={(e) => updateField("customerNumber", e.target.value)} placeholder="CUST-100" className="input-field" />
          </div>
        </div>

        <div>
          <label htmlFor="customerName" className="field-label">Customer / Company Name *</label>
          <input id="customerName" type="text" required value={form.customerName}
            onChange={(e) => updateField("customerName", e.target.value)} placeholder="Constructora del Norte S.A." className="input-field" />
        </div>

        <div>
          <label htmlFor="fiscalData" className="field-label">Fiscal Data</label>
          <textarea id="fiscalData" value={form.fiscalData} onChange={(e) => updateField("fiscalData", e.target.value)}
            placeholder="RFC, domicilio fiscal..." rows={3} className="input-field resize-none" />
        </div>

        <div>
          <label htmlFor="deliveryAddress" className="field-label">Delivery Address *</label>
          <textarea id="deliveryAddress" required value={form.deliveryAddress}
            onChange={(e) => updateField("deliveryAddress", e.target.value)} placeholder="Full delivery address..." rows={2} className="input-field resize-none" />
        </div>

        <div>
          <label htmlFor="notes" className="field-label">Notes / Observations</label>
          <textarea id="notes" value={form.notes} onChange={(e) => updateField("notes", e.target.value)}
            placeholder="Special instructions, schedules..." rows={3} className="input-field resize-none" />
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-[rgba(255,255,255,0.08)]">
          <button type="button" onClick={() => router.back()} className="btn-secondary">Cancel</button>
          <button type="submit" disabled={isSubmitting} className="btn-primary">
            {isSubmitting ? (
              <span className="flex items-center gap-2">
                <span className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                Creating...
              </span>
            ) : (
              "Create Order"
            )}
          </button>
        </div>
      </form>
    </div>
  );
}
