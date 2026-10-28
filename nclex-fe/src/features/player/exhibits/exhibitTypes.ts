/**
 * An exhibit beside a question (interfaces.md section 1). Trend, chart, audio and video items are
 * ordinary item types plus exhibits (architecture.md 5.4); a flowsheet is trend data, with
 * timestamps as columns and vitals or labs as rows.
 */
export interface Exhibit {
  id: string;
  kind: "table" | "flowsheet" | "image" | "audio" | "video" | "text";
  title: string;
  table?: { columns: string[]; rows: Array<{ label: string; cells: string[] }> } | null;
  mediaId?: string | null;
  body?: string | null;
}

/** One tab of the exhibit panel: a case-study tab (nurses' notes, vitals, labs…) or one item exhibit. */
export interface ExhibitTab {
  id: string;
  title: string;
  exhibits: Exhibit[];
}

/**
 * The item's exhibits. The API types the column as free JSON, so anything that is not a list of
 * exhibits is treated as none.
 */
export function parseExhibits(json: unknown): Exhibit[] {
  return Array.isArray(json) ? (json as Exhibit[]).filter((e) => e && typeof e.id === "string") : [];
}

/**
 * A case study's tabs. case_studies.scenario is "tabs: nurses' notes, vitals, labs, orders, history"
 * (database.md 2.4); F11 settles its shape as { tabs: [{ id, title, exhibits }] }, reusing Exhibit
 * (interfaces.md section 1).
 */
export function parseScenario(json: unknown): ExhibitTab[] {
  const tabs = (json as { tabs?: unknown } | null)?.tabs;
  if (!Array.isArray(tabs)) return [];
  return (tabs as ExhibitTab[])
    .filter((tab) => tab && typeof tab.id === "string")
    .map((tab) => ({ ...tab, exhibits: parseExhibits(tab.exhibits) }));
}

/** The panel's tabs: the case study's tabs first, then one tab per exhibit of the item itself. */
export function exhibitTabs(caseTabs: ExhibitTab[], exhibits: Exhibit[]): ExhibitTab[] {
  return [...caseTabs, ...exhibits.map((exhibit) => ({ id: exhibit.id, title: exhibit.title, exhibits: [exhibit] }))];
}
