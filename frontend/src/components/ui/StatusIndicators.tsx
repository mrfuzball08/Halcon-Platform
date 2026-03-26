"use client";

import { ORDER_STATUS_SEQUENCE, STATUS_LABELS } from "@/lib/types";
import type { OrderStatus } from "@/lib/types";

const STATUS_STYLES: Record<OrderStatus, string> = {
  ORDERED: "bg-status-ordered/10 text-status-ordered border-status-ordered/25",
  IN_PROCESS: "bg-status-in-process/10 text-status-in-process border-status-in-process/25",
  IN_ROUTE: "bg-status-in-route/10 text-status-in-route border-status-in-route/25",
  DELIVERED: "bg-status-delivered/10 text-status-delivered border-status-delivered/25",
};

export function StatusBadge({ status }: { status: OrderStatus }) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-1 text-[11px] uppercase tracking-wider font-bold rounded-md border shadow-sm ${STATUS_STYLES[status]}`}
    >
      <span className="w-1.5 h-1.5 rounded-full bg-current" />
      {STATUS_LABELS[status]}
    </span>
  );
}

export function StatusProgressBar({ status }: { status: OrderStatus }) {
  const currentIndex = ORDER_STATUS_SEQUENCE.indexOf(status);

  return (
    <div className="w-full pt-6 pb-12">
      <div className="relative flex justify-between items-center w-full">
        {/* Background Track */}
        <div className="absolute top-[18px] left-[18px] right-[18px] h-[3px] bg-surface-3 z-0 rounded-full" />
        
        {/* Active Fill Track */}
        <div className="absolute top-[18px] left-[18px] right-[18px] h-[3px] z-0">
          <div 
            className="h-full bg-accent-500 transition-all duration-700 ease-out shadow-[0_0_12px_rgba(16,163,127,0.6)] rounded-full" 
            style={{ width: `${(currentIndex / (ORDER_STATUS_SEQUENCE.length - 1)) * 100}%` }}
          />
        </div>

        {/* Nodes */}
        {ORDER_STATUS_SEQUENCE.map((s, i) => {
          const isCompleted = i <= currentIndex;
          const isCurrent = i === currentIndex;
          
          return (
            <div key={s} className="relative z-10 flex flex-col items-center">
               <div className={`
                 w-9 h-9 rounded-full flex items-center justify-center border-[3px] transition-all duration-500 bg-surface-0 box-content
                 ${isCompleted ? 'border-accent-500/30' : 'border-surface-2'}
               `}>
                 <div className={`
                   w-3.5 h-3.5 rounded-full transition-all duration-500
                   ${isCompleted ? 'bg-accent-500 shadow-[0_0_8px_rgba(16,163,127,0.9)]' : 'bg-surface-3'}
                 `} />
                 
                 {isCurrent && (
                   <div className="absolute inset-0 m-1.5 rounded-full border border-accent-500 animate-[ping_2.5s_ease-in-out_infinite]" />
                 )}
               </div>
               
               <div className="absolute top-14 w-32 text-center -ml-16 left-1/2">
                 <span className={`
                   text-[11px] font-bold uppercase tracking-widest transition-colors duration-300
                   ${isCurrent ? 'text-accent-400 drop-shadow-md' : isCompleted ? 'text-text-primary' : 'text-text-muted'}
                 `}>
                   {STATUS_LABELS[s]}
                 </span>
               </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
