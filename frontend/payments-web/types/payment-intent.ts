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

  createdAt: string; // ISO
  confirmedAt?: string | null; // ISO
  capturedAt?: string | null; // ISO
  reversedAt?: string | null; // ISO
  expiredAt?: string | null; // ISO
  expiresAt?: string | null; // ISO (deadline de expiración)
};

export type CreatePaymentIntentRequest = {
  amount: number;
  currency: string;
  description?: string;
};
