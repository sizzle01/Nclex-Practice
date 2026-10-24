/** One entry of `errors[]` in a VALIDATION_FAILED body (api.md section 1). */
export interface FieldError {
  field: string;
  code: string;
}

/**
 * An error response from the API, parsed from its ProblemDetails body (api.md section 1).
 * `code` is the machine-readable error code, e.g. "EMAIL_TAKEN".
 */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string | undefined,
    readonly fieldErrors: FieldError[],
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }

  /** Builds an ApiError from an openapi-fetch result's `response` and `error` body. */
  static from(response: Response, body: unknown): ApiError {
    const problem = (typeof body === "object" && body !== null ? body : {}) as {
      code?: unknown;
      title?: unknown;
      errors?: unknown;
    };
    const code = typeof problem.code === "string" ? problem.code : undefined;
    const title = typeof problem.title === "string" ? problem.title : `HTTP ${response.status}`;
    const fieldErrors = Array.isArray(problem.errors) ? (problem.errors as FieldError[]) : [];
    return new ApiError(response.status, code, fieldErrors, title);
  }
}

export function isApiError(error: unknown, code?: string): error is ApiError {
  return error instanceof ApiError && (code === undefined || error.code === code);
}
