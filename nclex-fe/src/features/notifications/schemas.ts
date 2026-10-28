import { z } from "zod";

/** The categories in the order the API returns them (database enum notification_category). */
export const categories = ["results", "reminders", "engagement", "content", "security"] as const;

export const preferencesSchema = z.object({
  preferences: z
    .array(
      z.object({
        category: z.enum(categories),
        inApp: z.boolean(),
        email: z.boolean(),
        locked: z.boolean(),
      }),
    )
    // "Replace preferences" (api.md section 6): every category, each once.
    .length(categories.length),
});

export type PreferencesFormValues = z.infer<typeof preferencesSchema>;
