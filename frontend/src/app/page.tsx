"use client";

import { useState } from "react";
import Link from "next/link";
import { publicApi } from "@/lib/api";
import { StatusProgressBar } from "@/components/ui/StatusIndicators";
import { MOCK_ORDERS } from "@/lib/mock-data";
import type { PublicOrderTrack } from "@/lib/types";

import { Logo } from "@/components/ui/Logo";

const IS_MOCK = process.env.NEXT_PUBLIC_MOCK_AUTH === "true";

export default function HomePage() {
  const [invoice, setInvoice] = useState("");
  const [customer, setCustomer] = useState("");
  const [result, setResult] = useState<PublicOrderTrack | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);

  async function handleTrack(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setResult(null);
    setLoading(true);
    setSearched(true);

    try {
      if (IS_MOCK) {
        await new Promise((r) => setTimeout(r, 600));
        const found = MOCK_ORDERS.find(
          (o) =>
            o.invoiceNumber.toLowerCase() === invoice.toLowerCase() &&
            o.customerNumber.toLowerCase() === customer.toLowerCase()
        );
        if (found) {
          setResult({
            invoiceNumber: found.invoiceNumber,
            customerName: found.customerName,
            status: found.status,
            deliveryPhotoUrl: found.deliveryPhotoUrl,
          });
        } else {
          setError("No order found. Please verify your invoice and customer number.");
        }
      } else {
        const data = await publicApi.trackOrder(invoice, customer);
        setResult(data);
      }
    } catch {
      setError("No order found. Please verify your invoice and customer number.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen flex flex-col bg-surface-0">
      {/* Header */}
      <header className="flex items-center justify-between px-6 lg:px-10 h-16 border-b border-[rgba(255,255,255,0.08)] bg-surface-1">
        <div className="flex items-center gap-3">
          <Logo className="w-8 h-8 text-accent-500" />
          <span className="text-[15px] font-semibold text-text-primary">Halcon Platform</span>
        </div>
        <Link href="/login" className="btn-secondary text-[13px]">
          Staff Login
        </Link>
      </header>

      {/* Main */}
      <main className="flex-1 flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-[440px] animate-fade-in">
          {/* Hero */}
          <div className="text-center mb-8">
            <h2 className="text-3xl font-semibold text-text-primary tracking-tight">Track Your Order</h2>
            <p className="text-text-muted mt-3 text-[15px] leading-relaxed">
              Enter your invoice and customer number to check the current delivery status
            </p>
          </div>

          {/* Search */}
          <div className="panel p-7">
            <form onSubmit={handleTrack} className="space-y-5">
              <div>
                <label htmlFor="track-invoice" className="field-label">Invoice Number</label>
                <input id="track-invoice" type="text" required value={invoice}
                  onChange={(e) => setInvoice(e.target.value)} placeholder="e.g. INV-001" className="input-field" />
              </div>
              <div>
                <label htmlFor="track-customer" className="field-label">Customer Number</label>
                <input id="track-customer" type="text" required value={customer}
                  onChange={(e) => setCustomer(e.target.value)} placeholder="e.g. CUST-100" className="input-field" />
              </div>
              <button type="submit" disabled={loading} className="btn-primary w-full mt-1">
                {loading ? (
                  <span className="flex items-center justify-center gap-2">
                    <span className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                    Searching...
                  </span>
                ) : (
                  "Track Order"
                )}
              </button>
            </form>

            {IS_MOCK && !searched && (
              <p className="text-[11px] text-text-muted mt-4 text-center">
                Try: INV-001 / CUST-100 &bull; INV-004 / CUST-103 (Delivered)
              </p>
            )}
          </div>

          {/* Results */}
          {searched && (
            <div className="mt-6 animate-fade-in">
              {error ? (
                <div className="panel p-8 text-center">
                  <svg className="w-10 h-10 mx-auto text-text-muted mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
                  </svg>
                  <p className="text-text-secondary text-sm">{error}</p>
                </div>
              ) : result ? (
                <div className="panel p-7 space-y-6">
                  <div>
                    <p className="text-[11px] text-text-muted uppercase tracking-widest font-semibold">Order</p>
                    <p className="text-xl font-semibold text-text-primary font-mono mt-1">{result.invoiceNumber}</p>
                    <p className="text-[14px] text-text-secondary mt-0.5">{result.customerName}</p>
                  </div>

                  <StatusProgressBar status={result.status} />

                  {result.status === "DELIVERED" && result.deliveryPhotoUrl && (
                    <div className="space-y-2">
                      <p className="field-label">Delivery Evidence</p>
                      <div className="rounded-xl overflow-hidden border border-[rgba(255,255,255,0.1)]">
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img src={result.deliveryPhotoUrl} alt="Delivery evidence" className="w-full h-48 object-cover" />
                      </div>
                    </div>
                  )}
                </div>
              ) : null}
            </div>
          )}
        </div>
      </main>

      {/* Footer */}
      <footer className="text-center py-5 border-t border-[rgba(255,255,255,0.08)]">
        <p className="text-[12px] text-text-muted">Halcon Platform &mdash; Construction Material Distribution</p>
      </footer>
    </div>
  );
}
