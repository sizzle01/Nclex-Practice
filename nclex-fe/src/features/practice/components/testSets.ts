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

/** GET /test-sets/{id} for the challenge set. */
export const challengeDetail: TestSetDetail = {
  ...challengeSet,
  track: "RN",
  kind: "prebuilt",
  description: "Exam conditions: 80 minutes, answers revealed at the end.",
  pausePolicy: { maxPauses: null, maxPauseSec: null },
  allowBackNav: true,
};
