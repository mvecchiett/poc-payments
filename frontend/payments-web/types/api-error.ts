/**
 * Estructura de error retornada por la API
 */
export type ApiErrorResponse = {
  error?: string;
  message?: string;
};

/**
 * Error enriquecido con información de HTTP status
 */
export class ApiError extends Error {
  public readonly statusCode: number;
  public readonly isConflict: boolean;
  public readonly isNotFound: boolean;
  public readonly isBadRequest: boolean;

  constructor(message: string, statusCode: number) {
    super(message);
    this.name = "ApiError";
    this.statusCode = statusCode;
    this.isConflict = statusCode === 409;
    this.isNotFound = statusCode === 404;
    this.isBadRequest = statusCode === 400;
  }
}

/**
 * Error específico para conflictos de estado (409)
 */
export class ConflictError extends ApiError {
  constructor(message: string) {
    super(message, 409);
    this.name = "ConflictError";
  }
}

/**
 * Error específico para recursos no encontrados (404)
 */
export class NotFoundError extends ApiError {
  constructor(message: string) {
    super(message, 404);
    this.name = "NotFoundError";
  }
}
