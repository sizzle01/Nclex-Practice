import type { TestSetSummary } from "./api/practiceApi";

/** The tabs above the catalog. "all" shows every set; the others narrow it down. */
export const filters = ["all", "practice", "timed", "untimed", "adaptive"] as const;
export type SetFilter = (typeof filters)[number];

export const filterLabels: Record<SetFilter, string> = {
  all: "All",
  practice: "Practice",
  timed: "Timed",
  untimed: "Untimed",
  adaptive: "Adaptive",
};

/** Reads ?filter= from the URL; anything unknown falls back to "all" so an old link still works. */
export function parseFilter(value: string | null): SetFilter {
  return filters.find((filter) => filter === value) ?? "all";
}

const filterRules: Record<SetFilter, (set: TestSetSummary) => boolean> = {
  all: () => true,
  // Practice means "learn as you go": tutor sets, the sample and the case studies.
  practice: (set) => set.mode === "practice",
  timed: (set) => set.timed,
  untimed: (set) => !set.timed,
  adaptive: (set) => set.mode === "cat",
};

/** The sets a tab shows, in the API's order. */
export function filterSets(sets: TestSetSummary[], filter: SetFilter): TestSetSummary[] {
  return sets.filter(filterRules[filter]);
}

export type SectionId = "start" | "practice" | "caseStudies" | "timedExams" | "untimedExams" | "adaptive";

export interface SetSection {
  id: SectionId;
  title: string;
  description: string;
  sets: TestSetSummary[];
}

/** Section headings, in the order a candidate is expected to work through them. */
const sectionInfo: Array<Omit<SetSection, "sets">> = [
  { id: "start", title: "Start here", description: "A short sample to get used to the question types." },
  { id: "practice", title: "Practice sets", description: "Untimed, with the answer and rationale after each question." },
  { id: "caseStudies", title: "Case studies", description: "Unfolding NGN case studies, kept whole and in order." },
  { id: "timedExams", title: "Mock exams: timed", description: "Exam conditions against the clock, results at the end." },
  { id: "untimedExams", title: "Mock exams: untimed", description: "The same exams with no time limit, results at the end." },
  { id: "adaptive", title: "Adaptive exams", description: "Like the real NCLEX: the exam stops once your result is clear." },
];

// Nothing in the API marks a set as a case study, so the curated titles ("NGN case studies, set 1
// of 2") are the signal. A test_sets category column would replace this if titles ever change.
const caseStudyTitle = /case stud/i;

/** The one section a set belongs to. Order matters: the sample and adaptive sets win over the rest. */
export function sectionOf(set: TestSetSummary): SectionId {
  if (set.isSample) return "start";
  if (set.mode === "cat") return "adaptive";
  if (set.mode === "challenge") return set.timed ? "timedExams" : "untimedExams";
  if (caseStudyTitle.test(set.title)) return "caseStudies";
  return "practice";
}

/**
 * The sets that pass the filter, grouped into sections. Empty sections are left out, and sets keep
 * the API's order inside their section.
 */
export function groupSets(sets: TestSetSummary[], filter: SetFilter): SetSection[] {
  const shown = filterSets(sets, filter);
  return sectionInfo
    .map((info) => ({ ...info, sets: shown.filter((set) => sectionOf(set) === info.id) }))
    .filter((section) => section.sets.length > 0);
}
