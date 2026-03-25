"use client";

import { useState, useEffect } from "react";

interface ToastProps {
  message: string;
  type?: "success" | "error" | "warning" | "info";
  onClose: () => void;
  duration?: number;
}

const TOAST_STYLES = {
  success: "bg-status-delivered/10 border-status-delivered/25 text-status-delivered",
  error: "bg-danger-500/10 border-danger-500/25 text-danger-500",
  warning: "bg-status-in-process/10 border-status-in-process/25 text-status-in-process",
  info: "bg-accent-500/10 border-accent-500/25 text-accent-400",
};

export function Toast({ message, type = "info", onClose, duration = 4000 }: ToastProps) {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    requestAnimationFrame(() => setIsVisible(true));
    const timer = setTimeout(() => {
      setIsVisible(false);
      setTimeout(onClose, 200);
    }, duration);
    return () => clearTimeout(timer);
  }, [duration, onClose]);

  return (
    <div
      className={`
        fixed bottom-5 right-5 z-50 flex items-center gap-3 px-4 py-3 rounded-lg border
        shadow-lg transition-all duration-200
        ${TOAST_STYLES[type]}
        ${isVisible ? "translate-y-0 opacity-100" : "translate-y-3 opacity-0"}
      `}
    >
      <span className="text-sm font-medium">{message}</span>
      <button onClick={() => { setIsVisible(false); setTimeout(onClose, 200); }} className="opacity-60 hover:opacity-100">
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
          <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>
  );
}

interface ToastMessage {
  id: number;
  message: string;
  type: "success" | "error" | "warning" | "info";
}

let nextId = 0;

export function useToast() {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  function show(message: string, type: ToastMessage["type"] = "info") {
    const id = nextId++;
    setToasts((prev) => [...prev, { id, message, type }]);
  }

  function dismiss(id: number) {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }

  function ToastContainer() {
    return (
      <div className="fixed bottom-5 right-5 z-50 flex flex-col gap-2">
        {toasts.map((t) => (
          <Toast key={t.id} message={t.message} type={t.type} onClose={() => dismiss(t.id)} />
        ))}
      </div>
    );
  }

  return { show, ToastContainer };
}
