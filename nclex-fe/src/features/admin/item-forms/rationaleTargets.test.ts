import { describe, expect, it } from "vitest";
import { alignPerOption, rationaleTargets } from "./rationaleTargets";
import { emptyItem } from "./templates";

describe("rationaleTargets", () => {
  it("asks for a why per option", () => {
    expect(rationaleTargets("sata", emptyItem("sata").content).map((t) => t.id)).toEqual(["a", "b", "c", "d", "e"]);
  });

  it("uses row:column ids for matrix cells", () => {
    const ids = rationaleTargets("matrix_mc", emptyItem("matrix_mc").content).map((t) => t.id);
    expect(ids).toEqual(["r1:c1", "r1:c2", "r2:c1", "r2:c2", "r3:c1", "r3:c2"]);
  });

  it("uses blank:option ids for drop-downs", () => {
    expect(rationaleTargets("dropdown_cloze", emptyItem("dropdown_cloze").content).map((t) => t.id)).toContain("b2:c");
  });

  it("asks only about selectable highlight segments", () => {
    const content = { segments: [{ id: "s1", text: "Pale", selectable: true }, { id: "s2", text: "and", selectable: false }] };
    expect(rationaleTargets("highlight_text", content)).toEqual([{ id: "s1", label: "Pale" }]);
  });

  it("asks nothing for a calculation", () => {
    expect(rationaleTargets("fill_blank_calc", emptyItem("fill_blank_calc").content)).toEqual([]);
  });
});

describe("alignPerOption", () => {
  it("keeps what was written, adds empty boxes and drops removed ids", () => {
    const aligned = alignPerOption({ a: "Why a", z: "stale" }, [
      { id: "a", label: "A" },
      { id: "b", label: "B" },
    ]);
    expect(aligned).toEqual({ a: "Why a", b: "" });
  });
});
