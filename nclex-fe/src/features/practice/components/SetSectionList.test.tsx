import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { sectionStorageKey } from "../sectionCollapse";
import { groupSets } from "../setCategories";
import { SetSectionList } from "./SetSectionList";
import { challengeSet, practiceSet, sampleSet, topicAbgSet, topicPreeclampsiaSet } from "./testSets";

const catalog = groupSets([sampleSet, practiceSet, challengeSet], "all");
const topics = groupSets([topicAbgSet, topicPreeclampsiaSet], "topics");

function renderList(sections = catalog, openByDefault = true) {
  return render(
    <SetSectionList sections={sections} selectedId={null} panelId="panel" openByDefault={openByDefault} onOpen={vi.fn()} />,
  );
}

const toggle = (name: RegExp) => screen.getByRole("button", { name });

describe("SetSectionList (F28 collapsible sections)", () => {
  beforeEach(() => localStorage.clear());

  it("starts every section open by default, with its cards showing", () => {
    renderList();

    expect(toggle(/^Start here/)).toHaveAttribute("aria-expanded", "true");
    expect(toggle(/^Practice sets/)).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByRole("heading", { name: "RN Sample Set" })).toBeVisible();
  });

  it("starts closed when openByDefault is false, keeping the section headings", () => {
    renderList(topics, false);

    expect(screen.getByRole("heading", { name: /^Fundamental/ })).toBeInTheDocument();
    expect(toggle(/^Fundamental/)).toHaveAttribute("aria-expanded", "false");
    expect(screen.queryByRole("heading", { name: "ABG" })).not.toBeInTheDocument();
  });

  it("toggles a section with a click, and points the button at the list it controls", async () => {
    const user = userEvent.setup();
    renderList();
    const button = toggle(/^Start here/);
    const list = document.getElementById(button.getAttribute("aria-controls")!)!;

    await user.click(button);
    expect(button).toHaveAttribute("aria-expanded", "false");
    expect(list).not.toBeVisible();

    await user.click(button);
    expect(button).toHaveAttribute("aria-expanded", "true");
    expect(list).toBeVisible();
  });

  it("toggles with Enter and Space from the keyboard", async () => {
    const user = userEvent.setup();
    renderList();
    toggle(/^Start here/).focus();

    await user.keyboard("{Enter}");
    expect(toggle(/^Start here/)).toHaveAttribute("aria-expanded", "false");
    await user.keyboard(" ");
    expect(toggle(/^Start here/)).toHaveAttribute("aria-expanded", "true");
  });

  it("collapses and expands every section at once", async () => {
    const user = userEvent.setup();
    renderList();

    await user.click(screen.getByRole("button", { name: "Collapse all" }));
    expect(toggle(/^Start here/)).toHaveAttribute("aria-expanded", "false");
    expect(toggle(/^Mock exams: timed/)).toHaveAttribute("aria-expanded", "false");

    await user.click(screen.getByRole("button", { name: "Expand all" }));
    expect(toggle(/^Start here/)).toHaveAttribute("aria-expanded", "true");
    expect(toggle(/^Mock exams: timed/)).toHaveAttribute("aria-expanded", "true");
  });

  it("remembers what the candidate closed across page loads", async () => {
    const user = userEvent.setup();
    const { unmount } = renderList();
    await user.click(toggle(/^Practice sets/));
    unmount();

    renderList();
    expect(toggle(/^Practice sets/)).toHaveAttribute("aria-expanded", "false");
    expect(toggle(/^Start here/)).toHaveAttribute("aria-expanded", "true");
  });

  it("falls back to the defaults when the saved value is not valid", () => {
    localStorage.setItem(sectionStorageKey, "not json");
    renderList();

    expect(toggle(/^Start here/)).toHaveAttribute("aria-expanded", "true");
  });
});
