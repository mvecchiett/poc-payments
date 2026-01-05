import type { CreatePaymentIntentRequest, PaymentIntent } from "@/types/payment-intent";

const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE_URL?.trim() || "http://localhost:5000";

type ApiError = {
  error?: string;
  message?: string;
};

async function parseError(res: Response): Promise<string> {
  try {
    const data = (await res.json()) as ApiError;
    return data.error || data.message || `${res.status} ${res.statusText}`;
  } catch {
    return `${res.status} ${res.statusText}`;
  }
}

async function http<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${API_BASE}${path}`;
  const res = await fetch(url, {
    ...init,
    headers: {
      "content-type": "application/json",
      ...(init?.headers ?? {}),
    },
    // En Next App Router, en server components cachea por defecto.
    // Para POC, lo dejamos sin cache.
    cache: "no-store",
  });

  if (!res.ok) {
    throw new Error(await parseError(res));
  }

  // 204 No Content
  if (res.status === 204) return undefined as T;

  return (await res.json()) as T;
}

/** API calls (ajustá paths si tu controller difiere) */
export const paymentsApi = {
  createIntent: (body: CreatePaymentIntentRequest) =>
    http<PaymentIntent>("/api/payment-intents", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  getIntent: (id: string) =>
    http<PaymentIntent>(`/api/payment-intents/${encodeURIComponent(id)}`),

  confirmIntent: (id: string) =>
    http<PaymentIntent>(`/api/payment-intents/${encodeURIComponent(id)}/confirm`, {
      method: "POST",
      body: JSON.stringify({}),
    }),

  captureIntent: (id: string) =>
    http<PaymentIntent>(`/api/payment-intents/${encodeURIComponent(id)}/capture`, {
      method: "POST",
      body: JSON.stringify({}),
    }),

  reverseIntent: (id: string) =>
    http<PaymentIntent>(`/api/payment-intents/${encodeURIComponent(id)}/reverse`, {
      method: "POST",
      body: JSON.stringify({}),
    }),
};
