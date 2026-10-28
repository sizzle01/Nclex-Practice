import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it } from "vitest";
import { expectNoAxeViolations } from "@/test/axe";
import { stubMatchMedia } from "@/test/matchMedia";
import { caseStudyItem, exhibitItem, galleryMedia } from "../gallery/galleryExhibits";
import { MediaProvider } from "../media/MediaContext";
import { ExhibitPanel } from "./ExhibitPanel";

function renderPanel(item: { exhibits?: unknown; caseStudy?: unknown }, withMedia = true) {
  const panel = <ExhibitPanel exhibits={item.exhibits ?? []} caseStudy={(item.caseStudy as never) ?? null} />;
  return render(withMedia ? <MediaProvider resolve={galleryMedia}>{panel}</MediaProvider> : panel);
}

afterEach(() => stubMatchMedia(false));

describe("ExhibitPanel", () => {
  it("renders nothing when the item has no exhibits or case study", () => {
    const { container } = renderPanel({ exhibits: [] });

    expect(container).toBeEmptyDOMElement();
  });

  it("shows a flowsheet with parameters as row headers and times as column headers", () => {
    renderPanel(exhibitItem);

    const table = screen.getByRole("table", { name: "Vital signs" });
    expect(within(table).getByRole("columnheader", { name: "16:00" })).toBeInTheDocument();
    expect(within(table).getByRole("rowheader", { name: "Heart rate (/min)" })).toBeInTheDocument();
    expect(within(table).getByRole("cell", { name: "118" })).toBeInTheDocument();
  });

  it("switches exhibit tabs with the arrow keys, Home and End", async () => {
    renderPanel(exhibitItem);
    const vitals = screen.getByRole("tab", { name: "Vital signs" });
    expect(vitals).toHaveAttribute("aria-selected", "true");
    expect(screen.getByRole("tab", { name: "Sacral wound" })).toHaveAttribute("tabindex", "-1");

    vitals.focus();
    await userEvent.keyboard("{ArrowRight}");
    expect(screen.getByRole("tab", { name: "Sacral wound" })).toHaveFocus();
    expect(screen.getByRole("tabpanel")).toContainElement(screen.getByRole("img"));

    await userEvent.keyboard("{End}");
    expect(screen.getByRole("tab", { name: "Heart sounds" })).toHaveAttribute("aria-selected", "true");
    await userEvent.keyboard("{ArrowRight}");
    expect(screen.getByRole("tab", { name: "Vital signs" })).toHaveFocus();
  });

  it("gives images their alt text", async () => {
    renderPanel(exhibitItem);

    await userEvent.click(screen.getByRole("tab", { name: "Sacral wound" }));

    expect(screen.getByRole("img")).toHaveAccessibleName(/Oval sacral wound/);
  });

  it("plays audio with captions and a transcript link", async () => {
    const { container } = renderPanel(exhibitItem);

    await userEvent.click(screen.getByRole("tab", { name: "Heart sounds" }));

    const track = container.querySelector("audio track");
    expect(track).toHaveAttribute("kind", "captions");
    expect(track).toHaveAttribute("src", "/dev-media/heart-sounds.vtt");
    expect(screen.getByRole("link", { name: "Read the transcript" })).toHaveAttribute(
      "href",
      "/dev-media/heart-sounds-transcript.txt",
    );
  });

  it("plays video with captions", () => {
    const { container } = render(
      <MediaProvider resolve={() => ({ url: "/clip.mp4", captionsUrl: "/clip.vtt" })}>
        <ExhibitPanel
          exhibits={[{ id: "v", kind: "video", title: "Inhaler technique", mediaId: "clip" }]}
          caseStudy={null}
        />
      </MediaProvider>,
    );

    expect(container.querySelector("video track")).toHaveAttribute("src", "/clip.vtt");
  });

  it("explains an image it can't show", async () => {
    renderPanel(exhibitItem, false);

    await userEvent.click(screen.getByRole("tab", { name: "Sacral wound" }));

    expect(screen.getByRole("note")).toHaveTextContent("Sacral wound: this image can't be shown right now.");
  });

  it("shows a case study's tabs under its title", async () => {
    renderPanel(caseStudyItem);

    expect(screen.getByRole("complementary", { name: /Case study: Heart failure exacerbation/ })).toBeInTheDocument();
    expect(screen.getAllByRole("tab").map((tab) => tab.textContent)).toEqual([
      "Nurses' notes",
      "Vital signs",
      "Orders",
    ]);
    expect(screen.getByText(/3 kg weight gain/)).toBeInTheDocument();

    await userEvent.click(screen.getByRole("tab", { name: "Orders" }));
    expect(screen.getByRole("rowheader", { name: "Furosemide 40 mg IV" })).toBeInTheDocument();
  });

  it("is a closed drawer on narrow screens, opened with a button", async () => {
    stubMatchMedia(true);
    renderPanel(caseStudyItem);

    const toggle = screen.getByRole("button", { name: "Show case details" });
    expect(toggle).toHaveAttribute("aria-expanded", "false");
    expect(screen.queryByRole("tablist")).not.toBeInTheDocument();

    await userEvent.click(toggle);

    expect(screen.getByRole("button", { name: "Hide case details" })).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByRole("tablist")).toBeInTheDocument();
  });

  it.each([
    ["exhibits", exhibitItem],
    ["a case study", caseStudyItem],
  ])("has no axe violations with %s", async (_, item) => {
    const { container } = renderPanel(item);

    await expectNoAxeViolations(container);
  });
});
