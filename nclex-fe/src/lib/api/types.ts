import type { components } from "./schema";

/** Shorthand for a schema type from the generated OpenAPI document, e.g. ApiSchema<"ReadinessResponse">. */
export type ApiSchema<Name extends keyof components["schemas"]> = components["schemas"][Name];
