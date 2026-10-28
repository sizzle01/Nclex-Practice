/**
 * The generated client types integers as `number | string`, because the .NET JSON options also
 * accept numbers written as strings. The API always sends numbers; this narrows the type.
 */
export function toNumber(value: number | string): number {
  return typeof value === "number" ? value : Number(value);
}
