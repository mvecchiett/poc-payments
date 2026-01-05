import type { PaymentIntentStatus } from "@/types/payment-intent";

export function StatusBadge({ status }: { status: PaymentIntentStatus }) {
  const base =
    "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset";

  const map: Record<PaymentIntentStatus, string> = {
    Created: "bg-slate-50 text-slate-700 ring-slate-200",
    PendingConfirmation: "bg-amber-50 text-amber-700 ring-amber-200",
    Captured: "bg-emerald-50 text-emerald-700 ring-emerald-200",
    Reversed: "bg-blue-50 text-blue-700 ring-blue-200",
    Expired: "bg-rose-50 text-rose-700 ring-rose-200",
  };

  return <span className={`${base} ${map[status]}`}>{status}</span>;
}
