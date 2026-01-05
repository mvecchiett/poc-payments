export type PaymentIntentStatus =
  | "Created"
  | "PendingConfirmation"
  | "Captured"
  | "Reversed"
  | "Expired";

export type PaymentIntent = {
  id: string;
  amount: number;
  currency: string;
  description?: string | null;
  status: PaymentIntentStatus;
  createdAt: string;
  updatedAt: string;
  confirmedAt?: string | null;
  expiresAt?: string | null;
  capturedAt?: string | null;
  reversedAt?: string | null;
  expiredAt?: string | null;
};

export type CreatePaymentIntentRequest = {
  amount: number;
  currency: string;
  description?: string;
};

// ============================================
// HELPERS DE DOMINIO
// ============================================

/**
 * Estados en los que el intent puede recibir acciones
 */
export type ActionableStatus = Extract<
  PaymentIntentStatus,
  "Created" | "PendingConfirmation"
>;

/**
 * Estados finales (sin acciones posibles)
 */
export type FinalStatus = Extract<
  PaymentIntentStatus,
  "Captured" | "Reversed" | "Expired"
>;

/**
 * Tipo de acción disponible
 */
export type IntentAction = "confirm" | "capture" | "reverse";

/**
 * Type guard: verifica si un status es accionable
 */
export function isActionable(status: PaymentIntentStatus): status is ActionableStatus {
  return status === "Created" || status === "PendingConfirmation";
}

/**
 * Type guard: verifica si un status es final
 */
export function isFinal(status: PaymentIntentStatus): status is FinalStatus {
  return status === "Captured" || status === "Reversed" || status === "Expired";
}

/**
 * Retorna las acciones disponibles para un status dado
 */
export function getAvailableActions(status: PaymentIntentStatus): IntentAction[] {
  switch (status) {
    case "Created":
      return ["confirm", "reverse"];
    case "PendingConfirmation":
      return ["capture", "reverse"];
    default:
      return [];
  }
}

/**
 * Verifica si una acción específica está disponible para un status
 */
export function canPerformAction(
  status: PaymentIntentStatus,
  action: IntentAction
): boolean {
  return getAvailableActions(status).includes(action);
}
