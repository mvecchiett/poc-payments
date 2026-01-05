import type { PaymentIntentStatus } from "@/types/payment-intent";

type StatusBadgeProps = {
  status: PaymentIntentStatus;
};

/**
 * Badge visual para el status de un payment intent
 * Colores semánticos según el estado
 */
export function StatusBadge({ status }: StatusBadgeProps) {
  const baseClasses =
    "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset";

  const statusStyles: Record<PaymentIntentStatus, string> = {
    Created: "bg-slate-50 text-slate-700 ring-slate-600/20",
    PendingConfirmation: "bg-amber-50 text-amber-800 ring-amber-600/30",
    Captured: "bg-emerald-50 text-emerald-700 ring-emerald-600/20",
    Reversed: "bg-blue-50 text-blue-700 ring-blue-600/20",
    Expired: "bg-rose-50 text-rose-700 ring-rose-600/20",
  };

  return (
    <span className={`${baseClasses} ${statusStyles[status]}`}>
      {status}
    </span>
  );
}
