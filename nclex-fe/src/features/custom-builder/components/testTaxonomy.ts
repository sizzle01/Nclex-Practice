import type { Taxonomy } from "../api/builderApi";

/** A trimmed RN taxonomy for builder tests: one category with subcategories and one without. */
export const testTaxonomy: Taxonomy = {
  clientNeeds: [
    {
      id: 1,
      code: "SECE",
      name: "Safe & Effective Care Environment",
      weightMin: null,
      weightMax: null,
      children: [
        { id: 2, code: "SECE.MOC", name: "Management of Care", weightMin: 15, weightMax: 21, children: null },
        { id: 3, code: "SECE.SIC", name: "Safety and Infection Control", weightMin: 10, weightMax: 16, children: null },
      ],
    },
    { id: 5, code: "PSI", name: "Psychosocial Integrity", weightMin: 6, weightMax: 12, children: null },
  ],
  bodySystems: [
    { id: 1, slug: "cardiovascular", name: "Cardiovascular" },
    { id: 2, slug: "respiratory", name: "Respiratory" },
  ],
  itemTypes: [
    { type: "multiple_choice", label: "Multiple Choice", available: true },
    { type: "sata", label: "Select All That Apply", available: true },
    { type: "bowtie", label: "Bowtie", available: false },
  ],
  cjmmSkills: [],
};
