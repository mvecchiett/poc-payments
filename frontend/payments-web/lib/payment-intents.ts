import { get, post } from "./api";
import type {
  CreatePaymentIntentRequest,
  PaymentIntent,
  PaymentIntentStatus,
} from "@/types/payment-intent";

/**
 * Obtiene todos los payment intents, con filtro opcional por status
 */
export async function getAllPaymentIntents(
  status?: PaymentIntentStatus
): Promise<PaymentIntent[]> {
  const queryParam = status ? `?status=${encodeURIComponent(status)}` : "";
  return get<PaymentIntent[]>(`/api/payment-intents${queryParam}`);
}

/**
 * Obtiene un payment intent por ID
 */
export async function getPaymentIntentById(id: string): Promise<PaymentIntent> {
  return get<PaymentIntent>(`/api/payment-intents/${encodeURIComponent(id)}`);
}

/**
 * Crea un nuevo payment intent
 */
export async function createPaymentIntent(
  request: CreatePaymentIntentRequest
): Promise<PaymentIntent> {
  return post<PaymentIntent>("/api/payment-intents", request);
}

/**
 * Confirma un payment intent (Created → PendingConfirmation)
 */
export async function confirmPaymentIntent(id: string): Promise<PaymentIntent> {
  return post<PaymentIntent>(
    `/api/payment-intents/${encodeURIComponent(id)}/confirm`,
    {}
  );
}

/**
 * Captura un payment intent (PendingConfirmation → Captured)
 */
export async function capturePaymentIntent(id: string): Promise<PaymentIntent> {
  return post<PaymentIntent>(
    `/api/payment-intents/${encodeURIComponent(id)}/capture`,
    {}
  );
}

/**
 * Revierte un payment intent (Created/PendingConfirmation → Reversed)
 */
export async function reversePaymentIntent(id: string): Promise<PaymentIntent> {
  return post<PaymentIntent>(
    `/api/payment-intents/${encodeURIComponent(id)}/reverse`,
    {}
  );
}
