import { describe, expect, it } from "vitest";
import { exhibitTabs, parseExhibits, parseScenario } from "./exhibitTypes";

describe("exhibit parsing", () => {
  it("treats anything but a list as no exhibits", () => {
    expect(parseExhibits(null)).toEqual([]);
    expect(parseExhibits({ id: "x" })).toEqual([]);
  });

  it("reads a scenario's tabs and ignores a scenario in another shape", () => {
    const tabs = parseScenario({
      tabs: [{ id: "notes", title: "Notes", exhibits: [{ id: "n", kind: "text", title: "07:00" }] }],
    });

    expect(tabs).toHaveLength(1);
    expect(tabs[0].exhibits[0].id).toBe("n");
    expect(parseScenario({ notes: "free text" })).toEqual([]);
  });

  it("puts the case study's tabs before one tab per item exhibit", () => {
    const tabs = exhibitTabs(
      [{ id: "notes", title: "Notes", exhibits: [] }],
      [{ id: "ecg", kind: "image", title: "ECG strip", mediaId: "m" }],
    );

    expect(tabs.map((tab) => tab.title)).toEqual(["Notes", "ECG strip"]);
  });
});
