"use client";

import { useState } from "react";
import type { PaymentIntent, IntentAction } from "@/types/payment-intent";
import { getAvailableActions } from "@/types/payment-intent";
import {
  confirmPaymentIntent,
  capturePaymentIntent,
  reversePaymentIntent,
} from "@/lib/payment-intents";
import { ApiError } from "@/types/api-error";

type PaymentActionsProps = {
  intent: PaymentIntent;
  onSuccess: (updatedIntent: PaymentIntent) => void;
};

/**
 * Botones de acción contextuales según el estado del payment intent
 * - Maneja loading states por acción
 * - Muestra errores inline (especialmente 409 Conflict)
 * - Disabled mientras hay una acción en curso
 */
export function PaymentActions({ intent, onSuccess }: PaymentActionsProps) {
  const [loading, setLoading] = useState<IntentAction | null>(null);
  const [error, setError] = useState<ApiError | null>(null);

  const availableActions = getAvailableActions(intent.status);

  if (availableActions.length === 0) {
    return (
      <div className="text-sm text-slate-500 italic">
        Sin acciones disponibles
      </div>
    );
  }

  async function handleAction(
    action: IntentAction,
    fn: () => Promise<PaymentIntent>
  ) {
    setLoading(action);
    setError(null);

    try {
      const updated = await fn();
      onSuccess(updated);
    } catch (e) {
      if (e instanceof ApiError) {
        setError(e);
      } else {
        setError(new ApiError(String(e), 500));
      }
    } finally {
      setLoading(null);
    }
  }

  const isDisabled = loading !== null;

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap gap-2">
        {availableActions.includes("confirm") && (
          <button
            onClick={() => handleAction("confirm", () => confirmPaymentIntent(intent.id))}
            disabled={isDisabled}
            className="rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {loading === "confirm" ? "Confirmando..." : "Confirmar"}
          </button>
        )}

        {availableActions.includes("capture") && (
          <button
            onClick={() => handleAction("capture", () => capturePaymentIntent(intent.id))}
            disabled={isDisabled}
            className="rounded-lg border border-emerald-300 bg-emerald-50 px-3 py-1.5 text-sm font-medium text-emerald-700 hover:bg-emerald-100 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {loading === "capture" ? "Capturando..." : "Capturar"}
          </button>
        )}

        {availableActions.includes("reverse") && (
          <button
            onClick={() => handleAction("reverse", () => reversePaymentIntent(intent.id))}
            disabled={isDisabled}
            className="rounded-lg border border-blue-300 bg-blue-50 px-3 py-1.5 text-sm font-medium text-blue-700 hover:bg-blue-100 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {loading === "reverse" ? "Revirtiendo..." : "Revertir"}
          </button>
        )}
      </div>

      {error && (
        <div
          className={`text-sm ${
            error.isConflict
              ? "text-amber-700"
              : "text-rose-700"
          }`}
        >
          {error.message}
        </div>
      )}
    </div>
  );
}
