import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { type ApiError, isApiError } from "@/lib/api/errors";

// Messages for the per-field codes in a VALIDATION_FAILED body (api.md section 1).
const fieldCodeMessages: Record<string, string> = {
  REQUIRED: "This field is required.",
  INVALID_FORMAT: "This doesn't look right. Check the format.",
  INVALID_VALUE: "Choose one of the options.",
  TOO_SHORT: "This is too short.",
  TOO_LONG: "This is too long.",
};

const fallbackMessage = "Something went wrong. Please try again.";

/**
 * Shows an API error on the form: per-field validation errors go next to their fields; anything
 * else becomes the form-level message that is returned (null when every error was placed).
 */
export function applyServerErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fields: readonly Path<T>[],
): string | null {
  if (!isApiError(error)) return fallbackMessage;
  if (error.code !== "VALIDATION_FAILED") return error.message || fallbackMessage;

  const unplaced = placeFieldErrors(error, setError, fields);
  return unplaced ? fallbackMessage : null;
}

function placeFieldErrors<T extends FieldValues>(
  error: ApiError,
  setError: UseFormSetError<T>,
  fields: readonly Path<T>[],
): boolean {
  let unplaced = false;
  for (const { field, code } of error.fieldErrors) {
    if ((fields as readonly string[]).includes(field)) {
      setError(field as Path<T>, { type: "server", message: fieldCodeMessages[code] ?? fallbackMessage });
    } else {
      unplaced = true;
    }
  }
  return unplaced;
}
