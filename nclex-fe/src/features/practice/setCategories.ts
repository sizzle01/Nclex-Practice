import type { TestSetSummary } from "./api/practiceApi";

/** The tabs above the catalog. "all" shows every set; the others narrow it down. */
export const filters = ["all", "practice", "timed", "untimed", "adaptive", "topics"] as const;
export type SetFilter = (typeof filters)[number];

export const filterLabels: Record<SetFilter, string> = {
  all: "All",
  practice: "Practice",
  timed: "Timed",
  untimed: "Untimed",
  adaptive: "Adaptive",
  topics: "Topics",
};

/** Reads ?filter= from the URL; anything unknown falls back to "all" so an old link still works. */
export function parseFilter(value: string | null): SetFilter {
  return filters.find((filter) => filter === value) ?? "all";
}

/** A topic test (F27): one review-book subtopic. Topic tests appear only under the Topics tab. */
export function isTopicTest(set: TestSetSummary): boolean {
  return Boolean(set.topicSection);
}

const filterRules: Record<SetFilter, (set: TestSetSummary) => boolean> = {
  all: (set) => !isTopicTest(set),
  // Practice means "learn as you go": tutor sets, the sample and the case studies.
  practice: (set) => set.mode === "practice" && !isTopicTest(set),
  timed: (set) => set.timed && !isTopicTest(set),
  untimed: (set) => !set.timed && !isTopicTest(set),
  adaptive: (set) => set.mode === "cat",
  topics: isTopicTest,
};

/** The sets a tab shows, in the API's order. */
export function filterSets(sets: TestSetSummary[], filter: SetFilter): TestSetSummary[] {
  return sets.filter(filterRules[filter]);
}

export type SectionId =
  | "start"
  | "practice"
  | "caseStudies"
  | "timedCaseBlocks"
  | "timedExams"
  | "untimedExams"
  | "adaptive";

export interface SetSection {
  /** A fixed section id, or "topic-<n>" for a review-book section in the Topics tab. */
  id: SectionId | `topic-${number}`;
  title: string;
  description: string;
  sets: TestSetSummary[];
}

/** Section headings, in the order a candidate is expected to work through them. */
const sectionInfo: Array<Omit<SetSection, "sets">> = [
  { id: "start", title: "Start here", description: "A short sample to get used to the question types." },
  { id: "practice", title: "Practice sets", description: "Untimed, with the answer and rationale after each question." },
  { id: "caseStudies", title: "Case studies", description: "Unfolding NGN case studies, kept whole and in order." },
  {
    id: "timedCaseBlocks",
    title: "Timed case-study blocks",
    description: "Ten whole case studies against the NCLEX clock, to build stamina for the second half of the exam.",
  },
  { id: "timedExams", title: "Mock exams: timed", description: "Exam conditions against the clock, results at the end." },
  { id: "untimedExams", title: "Mock exams: untimed", description: "The same exams with no time limit, results at the end." },
  { id: "adaptive", title: "Adaptive exams", description: "Like the real NCLEX: the exam stops once your result is clear." },
];

// Nothing in the API marks a set as a case study, so the curated titles ("NGN case studies, set 1
// of 2", "Timed case study block 1") are the signal. A test_sets category column would replace this if titles ever change.
const caseStudyTitle = /case stud/i;

/** The one section a set belongs to. Order matters: the sample and adaptive sets win over the rest. */
export function sectionOf(set: TestSetSummary): SectionId {
  if (set.isSample) return "start";
  if (set.mode === "cat") return "adaptive";
  // Timed blocks of whole cases (F24) are challenges too, but they get their own section.
  if (set.mode === "challenge" && set.timed && caseStudyTitle.test(set.title)) return "timedCaseBlocks";
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
  if (filter === "topics") return groupTopicSets(shown);
  return sectionInfo
    .map((info) => ({ ...info, sets: shown.filter((set) => sectionOf(set) === info.id) }))
    .filter((section) => section.sets.length > 0);
}

/** The review book's sections, in the book's order (content/rn-bank/book_topics.py). */
export const topicSectionOrder = [
  "Fundamental",
  "Holistic health assessment",
  "Pharmacology",
  "Mental health",
  "Maternity",
  "Pediatrics",
  "Med-Surg",
];

const topicSectionDescriptions: Record<string, string> = {
  Fundamental: "ABGs, blood groups, EKG, dosage calculation, IV fluids, electrolytes and labs.",
  "Holistic health assessment": "Head-to-toe assessment and cranial nerves.",
  Pharmacology: "Each drug class, tested inside real clinical situations.",
  "Mental health": "Mental health disorders and their medications.",
  Maternity: "Pregnancy, labor and delivery, postpartum and the newborn.",
  Pediatrics: "Growth and development, childhood disorders and vaccines.",
  "Med-Surg": "Adult medical-surgical disorders, Med-Surg 1 and 2.",
};

/**
 * Topic tests grouped under their review-book sections in book order (F27). Each test is untimed, serves only
 * questions on its subtopic, and gives the result at the end. Sections not in the known list come last.
 */
export function groupTopicSets(sets: TestSetSummary[]): SetSection[] {
  const names = [...new Set(sets.map((set) => set.topicSection ?? ""))].filter(Boolean);
  const rank = (name: string) => {
    const i = topicSectionOrder.indexOf(name);
    return i === -1 ? topicSectionOrder.length : i;
  };
  return names
    .sort((a, b) => rank(a) - rank(b))
    .map((name, i) => ({
      id: `topic-${i}` as const,
      title: name,
      description: topicSectionDescriptions[name] ?? "Topic tests.",
      sets: sets.filter((set) => set.topicSection === name),
    }));
}
