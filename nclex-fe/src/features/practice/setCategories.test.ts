import { describe, expect, it } from "vitest";
import {
  adaptiveSet,
  caseStudySet,
  challengeSet,
  practiceSet,
  sampleSet,
  timedCaseBlockSet,
  topicAbgSet,
  topicInsulinSet,
  topicPreeclampsiaSet,
  untimedChallengeSet,
} from "./components/testSets";
import { filterSets, groupSets, parseFilter, sectionOf } from "./setCategories";

const catalog = [untimedChallengeSet, challengeSet, caseStudySet, practiceSet, sampleSet];

function sectionTitles(sections: ReturnType<typeof groupSets>) {
  return sections.map((section) => [section.title, section.sets.map((set) => set.title)]);
}

describe("sectionOf", () => {
  it("puts each kind of set in its own section", () => {
    expect(sectionOf(sampleSet)).toBe("start");
    expect(sectionOf(practiceSet)).toBe("practice");
    expect(sectionOf(caseStudySet)).toBe("caseStudies");
    expect(sectionOf(challengeSet)).toBe("timedExams");
    expect(sectionOf(untimedChallengeSet)).toBe("untimedExams");
    expect(sectionOf(adaptiveSet)).toBe("adaptive");
    expect(sectionOf(timedCaseBlockSet)).toBe("timedCaseBlocks");
  });

  it("keeps an untimed case-study challenge with the untimed mock exams", () => {
    expect(sectionOf({ ...timedCaseBlockSet, timed: false, timeBudgetSec: null })).toBe("untimedExams");
  });

  it("keeps a sample set in Start here even when it is a case study", () => {
    expect(sectionOf({ ...caseStudySet, isSample: true })).toBe("start");
  });

  it("keeps an adaptive exam in Adaptive whether or not it is timed", () => {
    expect(sectionOf({ ...adaptiveSet, timed: false })).toBe("adaptive");
  });
});

describe("groupSets", () => {
  it("orders the sections from sample to mock exams and leaves out empty ones", () => {
    expect(sectionTitles(groupSets(catalog, "all"))).toEqual([
      ["Start here", ["RN Sample Set"]],
      ["Practice sets", ["Comprehensive Practice 1"]],
      ["Case studies", ["NGN case studies, set 1 of 2"]],
      ["Mock exams: timed", ["Comprehensive Challenge 1"]],
      ["Mock exams: untimed", ["NCLEX-RN challenge 1 (untimed)"]],
    ]);
  });

  it("puts timed case-study blocks between the case studies and the timed mock exams", () => {
    const ids = groupSets([...catalog, timedCaseBlockSet], "all").map((section) => section.id);
    expect(ids).toEqual(["start", "practice", "caseStudies", "timedCaseBlocks", "timedExams", "untimedExams"]);
    expect(filterSets([timedCaseBlockSet], "timed")).toEqual([timedCaseBlockSet]);
  });

  it("keeps the API's order inside a section", () => {
    const second = { ...practiceSet, id: "p2", title: "Comprehensive Practice 2" };
    const [, practice] = groupSets([sampleSet, second, practiceSet], "all");
    expect(practice.sets.map((set) => set.title)).toEqual(["Comprehensive Practice 2", "Comprehensive Practice 1"]);
  });

  it("shows only practice-mode sets under Practice", () => {
    expect(groupSets(catalog, "practice").map((section) => section.id)).toEqual(["start", "practice", "caseStudies"]);
  });

  it("splits timed and untimed sets", () => {
    expect(filterSets(catalog, "timed")).toEqual([challengeSet]);
    expect(filterSets(catalog, "untimed")).toEqual([untimedChallengeSet, caseStudySet, practiceSet, sampleSet]);
  });

  it("has nothing under Adaptive until adaptive exams are published", () => {
    expect(groupSets(catalog, "adaptive")).toEqual([]);
    expect(filterSets([...catalog, adaptiveSet], "adaptive")).toEqual([adaptiveSet]);
  });
});

describe("parseFilter", () => {
  it("reads a known filter and falls back to all", () => {
    expect(parseFilter("timed")).toBe("timed");
    expect(parseFilter(null)).toBe("all");
    expect(parseFilter("nonsense")).toBe("all");
  });
});

describe("topic tests (F27)", () => {
  const withTopics = [...catalog, topicPreeclampsiaSet, topicInsulinSet, topicAbgSet];

  it("shows topic tests only under the Topics tab", () => {
    expect(filterSets(withTopics, "topics")).toEqual([topicPreeclampsiaSet, topicInsulinSet, topicAbgSet]);
    expect(filterSets(withTopics, "all")).toEqual(catalog);
    expect(filterSets(withTopics, "practice")).not.toContain(topicAbgSet);
    expect(filterSets(withTopics, "untimed")).not.toContain(topicAbgSet);
  });

  it("groups topic tests under their review-book sections in book order", () => {
    const sections = groupSets(withTopics, "topics");

    expect(sections.map((section) => [section.title, section.sets.map((set) => set.title)])).toEqual([
      ["Fundamental", ["ABG"]],
      ["Pharmacology", ["Endocrine: insulin"]],
      ["Maternity", ["Preeclampsia"]],
    ]);
  });

  it("reads the topics tab from the URL", () => {
    expect(parseFilter("topics")).toBe("topics");
  });
});
