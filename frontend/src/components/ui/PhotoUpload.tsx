"use client";

import { useRef, useState } from "react";

interface PhotoUploadProps {
  label: string;
  existingUrl?: string | null;
  onUpload: (file: File) => void;
  disabled?: boolean;
}

export function PhotoUpload({ label, existingUrl, onUpload, disabled }: PhotoUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<string | null>(existingUrl ?? null);
  const [isDragOver, setIsDragOver] = useState(false);

  function handleFile(file: File) {
    if (!file.type.startsWith("image/")) return;
    setPreview(URL.createObjectURL(file));
    onUpload(file);
  }

  return (
    <div className="space-y-2">
      <label className="text-sm font-medium text-text-secondary">{label}</label>

      {preview ? (
        <div className="relative group rounded-lg overflow-hidden border border-border">
          <img src={preview} alt={label} className="w-full h-48 object-cover" />
          {!disabled && (
            <button
              onClick={() => {
                setPreview(null);
                if (inputRef.current) inputRef.current.value = "";
              }}
              className="absolute top-2 right-2 p-1.5 rounded bg-surface-0/80 text-text-secondary
                         hover:bg-danger-500 hover:text-white transition-all opacity-0 group-hover:opacity-100"
            >
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          )}
        </div>
      ) : (
        <div
          className={`
            relative flex flex-col items-center justify-center h-48 rounded-lg border-2 border-dashed
            transition-colors cursor-pointer
            ${isDragOver ? "border-accent-500 bg-accent-500/5" : "border-border hover:border-border-hover"}
            ${disabled ? "opacity-50 cursor-not-allowed" : ""}
          `}
          onDragOver={(e) => { e.preventDefault(); if (!disabled) setIsDragOver(true); }}
          onDragLeave={() => setIsDragOver(false)}
          onDrop={(e) => {
            e.preventDefault();
            setIsDragOver(false);
            if (!disabled && e.dataTransfer.files[0]) handleFile(e.dataTransfer.files[0]);
          }}
          onClick={() => !disabled && inputRef.current?.click()}
        >
          <svg className="w-8 h-8 text-text-muted mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5" />
          </svg>
          <p className="text-sm text-text-secondary">
            Drop image here or <span className="text-accent-400 font-medium">browse</span>
          </p>
          <p className="text-xs text-text-muted mt-1">PNG, JPG up to 10MB</p>
        </div>
      )}

      <input
        ref={inputRef}
        type="file"
        accept="image/*"
        className="hidden"
        disabled={disabled}
        onChange={(e) => { if (e.target.files?.[0]) handleFile(e.target.files[0]); }}
      />
    </div>
  );
}
