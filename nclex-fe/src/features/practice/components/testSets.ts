import type { TestSetDetail, TestSetSummary } from "../api/practiceApi";

/** The three seeded sets as GET /test-sets returns them, for practice tests. */
export const sampleSet: TestSetSummary = {
  id: "f06a0000-0000-4000-8000-000000000001",
  title: "RN Sample Set",
  mode: "practice",
  revealMode: "tutor",
  questionCount: 10,
  timed: false,
  timeBudgetSec: null,
  perQuestionSec: null,
  passThreshold: null,
  isSample: true,
  accessTier: "free",
  locked: false,
  myLastAttempt: null,
};

export const practiceSet: TestSetSummary = {
  ...sampleSet,
  id: "f06a0000-0000-4000-8000-000000000002",
  title: "Comprehensive Practice 1",
  questionCount: 40,
  isSample: false,
};

export const challengeSet: TestSetSummary = {
  ...practiceSet,
  id: "f06a0000-0000-4000-8000-000000000003",
  title: "Comprehensive Challenge 1",
  mode: "challenge",
  revealMode: "exam",
  timed: true,
  timeBudgetSec: 4800,
  passThreshold: 0.65,
};

/** The RN bank's other kinds of set, for the catalog's sections and filter tabs. */
export const caseStudySet: TestSetSummary = {
  ...practiceSet,
  id: "c1e00000-0000-4000-8000-000000000022",
  title: "NGN case studies, set 1 of 2",
  questionCount: 30,
};

/** A timed block of whole case studies (F24): exam rules, NCLEX pace, no pass estimate. */
export const timedCaseBlockSet: TestSetSummary = {
  ...challengeSet,
  id: "c1e00000-0000-4000-8000-000000003001",
  title: "Timed case study block 1",
  questionCount: 60,
  timeBudgetSec: 7200,
  passThreshold: null,
};

export const untimedChallengeSet: TestSetSummary = {
  ...challengeSet,
  id: "c1e00000-0000-4000-8000-000000000028",
  title: "NCLEX-RN challenge 1 (untimed)",
  timed: false,
  timeBudgetSec: null,
};

export const adaptiveSet: TestSetSummary = {
  ...challengeSet,
  id: "c1e00000-0000-4000-8000-000000000099",
  title: "Adaptive exam 1",
  mode: "cat",
  questionCount: 150,
  timeBudgetSec: 18000,
  passThreshold: null,
  adaptive: { minItems: 85, maxItems: 150 },
};

/** GET /test-sets/{id} for the adaptive exam (F19). */
export const adaptiveDetail: TestSetDetail = {
  ...adaptiveSet,
  track: "RN",
  kind: "cat",
  description: "Works like the real NCLEX-RN.",
  pausePolicy: { maxPauses: null, maxPauseSec: null },
  allowBackNav: false,
};

/** GET /test-sets/{id} for the challenge set. */
export const challengeDetail: TestSetDetail = {
  ...challengeSet,
  track: "RN",
  kind: "prebuilt",
  description: "Exam conditions: 80 minutes, answers revealed at the end.",
  pausePolicy: { maxPauses: null, maxPauseSec: null },
  allowBackNav: true,
};
