"use client";

import type { PaymentIntent } from "@/types/payment-intent";
import { StatusBadge } from "./status-badge";
import { PaymentActions } from "./payment-actions";

type PaymentIntentRowProps = {
  intent: PaymentIntent;
  onUpdate: (updatedIntent: PaymentIntent) => void;
};

/**
 * Fila/card que muestra un payment intent con su información y acciones
 */
export function PaymentIntentRow({ intent, onUpdate }: PaymentIntentRowProps) {
  // ID corto para display (primeros 12 caracteres)
  const shortId = intent.id.substring(0, 20) + "...";

  // Formatear fecha
  function formatDate(isoDate: string | null | undefined): string {
    if (!isoDate) return "-";
    return new Date(isoDate).toLocaleString("es-AR", {
      dateStyle: "short",
      timeStyle: "short",
    });
  }

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
      {/* Header */}
      <div className="flex items-start justify-between gap-4 mb-3">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <span className="font-mono text-sm text-slate-600" title={intent.id}>
              {shortId}
            </span>
            <StatusBadge status={intent.status} />
          </div>
          {intent.description && (
            <p className="text-sm text-slate-600 truncate">
              {intent.description}
            </p>
          )}
        </div>

        <div className="text-right">
          <div className="text-lg font-semibold text-slate-900">
            {intent.amount.toLocaleString("es-AR")}
          </div>
          <div className="text-sm text-slate-500">{intent.currency}</div>
        </div>
      </div>

      {/* Timestamps */}
      <div className="grid grid-cols-2 gap-2 text-xs text-slate-500 mb-3">
        <div>
          <span className="font-medium">Creado:</span> {formatDate(intent.createdAt)}
        </div>
        {intent.confirmedAt && (
          <div>
            <span className="font-medium">Confirmado:</span> {formatDate(intent.confirmedAt)}
          </div>
        )}
        {intent.expiresAt && (
          <div className="text-amber-600">
            <span className="font-medium">Expira:</span> {formatDate(intent.expiresAt)}
          </div>
        )}
        {intent.capturedAt && (
          <div>
            <span className="font-medium">Capturado:</span> {formatDate(intent.capturedAt)}
          </div>
        )}
        {intent.reversedAt && (
          <div>
            <span className="font-medium">Revertido:</span> {formatDate(intent.reversedAt)}
          </div>
        )}
        {intent.expiredAt && (
          <div>
            <span className="font-medium">Expirado:</span> {formatDate(intent.expiredAt)}
          </div>
        )}
      </div>

      {/* Actions */}
      <PaymentActions intent={intent} onSuccess={onUpdate} />
    </div>
  );
}
