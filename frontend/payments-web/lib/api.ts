import { ApiError, ApiErrorResponse, ConflictError, NotFoundError } from "@/types/api-error";

/**
 * Base URL de la API desde variables de entorno
 */
const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.trim() || "http://localhost:5000";

/**
 * Parsea el error de la respuesta HTTP
 */
async function parseErrorMessage(res: Response): Promise<string> {
  try {
    const data = (await res.json()) as ApiErrorResponse;
    return data.error || data.message || `${res.status} ${res.statusText}`;
  } catch {
    return `${res.status} ${res.statusText}`;
  }
}

/**
 * Crea el error apropiado según el status code
 */
async function createError(res: Response): Promise<ApiError> {
  const message = await parseErrorMessage(res);

  switch (res.status) {
    case 409:
      return new ConflictError(message);
    case 404:
      return new NotFoundError(message);
    default:
      return new ApiError(message, res.status);
  }
}

/**
 * Opciones base para fetch
 */
function getBaseOptions(init?: RequestInit): RequestInit {
  return {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
    // No cachear en App Router (datos en tiempo real)
    cache: "no-store",
  };
}

/**
 * Wrapper genérico para llamadas HTTP
 */
async function http<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${API_BASE_URL}${path}`;
  const options = getBaseOptions(init);

  const res = await fetch(url, options);

  // Manejar errores
  if (!res.ok) {
    throw await createError(res);
  }

  // 204 No Content
  if (res.status === 204) {
    return undefined as T;
  }

  // Parsear JSON
  return (await res.json()) as T;
}

/**
 * GET request
 */
export async function get<T>(path: string): Promise<T> {
  return http<T>(path, { method: "GET" });
}

/**
 * POST request
 */
export async function post<T>(path: string, body?: unknown): Promise<T> {
  return http<T>(path, {
    method: "POST",
    body: body ? JSON.stringify(body) : undefined,
  });
}

/**
 * PUT request
 */
export async function put<T>(path: string, body?: unknown): Promise<T> {
  return http<T>(path, {
    method: "PUT",
    body: body ? JSON.stringify(body) : undefined,
  });
}

/**
 * DELETE request
 */
export async function del<T>(path: string): Promise<T> {
  return http<T>(path, { method: "DELETE" });
}
