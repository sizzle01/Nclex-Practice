import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ReactNode } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/api/client";
import { expectNoAxeViolations } from "@/test/axe";
import { stubMatchMedia } from "@/test/matchMedia";
import {
  caseStudyItem,
  exhibitItem,
  galleryMedia,
} from "../gallery/galleryExhibits";
import { MediaProvider } from "../media/MediaContext";
import { ExhibitPanel } from "./ExhibitPanel";

// Only the network is replaced: media from the API go through the real useMedia and its query.
vi.mock("@/lib/api/client", () => ({ apiClient: { GET: vi.fn() } }));
const getMock = vi.mocked(apiClient.GET);

function reply(status: number, body: unknown) {
  const ok = status < 400;
  return {
    data: ok ? body : undefined,
    error: ok ? undefined : body,
    response: new Response(null, { status }),
  } as never;
}

function withQueryClient(children: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

/** Renders the panel with the gallery's local media, or, with withMedia false, media from the (mocked) API. */
function renderPanel(
  item: { exhibits?: unknown; caseStudy?: unknown },
  withMedia = true,
) {
  const panel = (
    <ExhibitPanel
      exhibits={item.exhibits ?? []}
      caseStudy={(item.caseStudy as never) ?? null}
    />
  );
  return render(
    withQueryClient(
      withMedia ? (
        <MediaProvider resolve={galleryMedia}>{panel}</MediaProvider>
      ) : (
        panel
      ),
    ),
  );
}

const imageExhibit = [
  { id: "photo", kind: "image", title: "Sacral skin", mediaId: "m-1" },
];

beforeEach(() => getMock.mockReset());
afterEach(() => stubMatchMedia(false));

describe("ExhibitPanel", () => {
  it("renders nothing when the item has no exhibits or case study", () => {
    const { container } = renderPanel({ exhibits: [] });

    expect(container).toBeEmptyDOMElement();
  });

  it("shows a flowsheet with parameters as row headers and times as column headers", () => {
    renderPanel(exhibitItem);

    const table = screen.getByRole("table", { name: "Vital signs" });
    expect(
      within(table).getByRole("columnheader", { name: "16:00" }),
    ).toBeInTheDocument();
    expect(
      within(table).getByRole("rowheader", { name: "Heart rate (/min)" }),
    ).toBeInTheDocument();
    expect(
      within(table).getByRole("cell", { name: "118" }),
    ).toBeInTheDocument();
  });

  it("switches exhibit tabs with the arrow keys, Home and End", async () => {
    renderPanel(exhibitItem);
    const vitals = screen.getByRole("tab", { name: "Vital signs" });
    expect(vitals).toHaveAttribute("aria-selected", "true");
    expect(screen.getByRole("tab", { name: "Sacral wound" })).toHaveAttribute(
      "tabindex",
      "-1",
    );

    vitals.focus();
    await userEvent.keyboard("{ArrowRight}");
    expect(screen.getByRole("tab", { name: "Sacral wound" })).toHaveFocus();
    expect(screen.getByRole("tabpanel")).toContainElement(
      screen.getByRole("img"),
    );

    await userEvent.keyboard("{End}");
    expect(screen.getByRole("tab", { name: "Heart sounds" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
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
    expect(
      screen.getByRole("link", { name: "Read the transcript" }),
    ).toHaveAttribute("href", "/dev-media/heart-sounds-transcript.txt");
  });

  it("plays video with captions", () => {
    const { container } = render(
      withQueryClient(
        <MediaProvider
          resolve={() => ({ url: "/clip.mp4", captionsUrl: "/clip.vtt" })}
        >
          <ExhibitPanel
            exhibits={[
              {
                id: "v",
                kind: "video",
                title: "Inhaler technique",
                mediaId: "clip",
              },
            ]}
            caseStudy={null}
          />
        </MediaProvider>,
      ),
    );

    expect(container.querySelector("video track")).toHaveAttribute(
      "src",
      "/clip.vtt",
    );
  });

  it("loads media from the API and uses its signed link and alt text", async () => {
    getMock.mockResolvedValue(
      reply(200, {
        id: "m-1",
        kind: "image",
        url: "/api/v1/media/m-1/file?exp=1&sig=abc",
        altText: "Red area over the sacrum",
        captionsUrl: null,
      }),
    );
    renderPanel({ exhibits: imageExhibit }, false);

    expect(screen.getByText("Loading Sacral skin…")).toBeInTheDocument();
    const image = await screen.findByRole("img", {
      name: "Red area over the sacrum",
    });
    expect(image).toHaveAttribute(
      "src",
      "/api/v1/media/m-1/file?exp=1&sig=abc",
    );
    expect(getMock).toHaveBeenCalledWith("/api/v1/media/{id}", {
      params: { path: { id: "m-1" } },
    });
  });

  it("links an audio clip's captions and transcript from the API", async () => {
    getMock.mockResolvedValue(
      reply(200, {
        id: "m-2",
        kind: "audio",
        url: "/api/v1/media/m-2/file?exp=1&sig=a",
        altText: null,
        captionsUrl: "/api/v1/media/m-2/captions?exp=1&sig=b",
        transcriptUrl: "/api/v1/media/m-2/transcript?exp=1&sig=c",
      }),
    );
    const { container } = renderPanel(
      {
        exhibits: [
          { id: "lungs", kind: "audio", title: "Lung sounds", mediaId: "m-2" },
        ],
      },
      false,
    );

    const link = await screen.findByRole("link", {
      name: "Read the transcript",
    });
    expect(link).toHaveAttribute(
      "href",
      "/api/v1/media/m-2/transcript?exp=1&sig=c",
    );
    expect(container.querySelector("audio track")).toHaveAttribute(
      "src",
      "/api/v1/media/m-2/captions?exp=1&sig=b",
    );
  });

  it("explains media the API can't find", async () => {
    getMock.mockResolvedValue(
      reply(404, { code: "NOT_FOUND", title: "Not found." }),
    );
    renderPanel({ exhibits: imageExhibit }, false);

    expect(await screen.findByRole("note")).toHaveTextContent(
      "Sacral skin: this image can't be shown right now.",
    );
  });

  it("asks once for fresh links when the browser is refused one, then gives up", async () => {
    getMock
      .mockResolvedValueOnce(
        reply(200, {
          id: "m-1",
          kind: "image",
          url: "/expired",
          altText: "Red area",
          captionsUrl: null,
        }),
      )
      .mockResolvedValue(
        reply(200, {
          id: "m-1",
          kind: "image",
          url: "/fresh",
          altText: "Red area",
          captionsUrl: null,
        }),
      );
    renderPanel({ exhibits: imageExhibit }, false);

    fireEvent.error(await screen.findByRole("img"));
    await waitFor(() =>
      expect(screen.getByRole("img")).toHaveAttribute("src", "/fresh"),
    );
    expect(getMock).toHaveBeenCalledTimes(2);

    fireEvent.error(screen.getByRole("img"));
    expect(screen.getByRole("note")).toHaveTextContent(
      "can't be shown right now",
    );
    expect(getMock).toHaveBeenCalledTimes(2);
  });

  it("shows a case study's tabs under its title", async () => {
    renderPanel(caseStudyItem);

    expect(
      screen.getByRole("complementary", {
        name: /Case study: Heart failure exacerbation/,
      }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole("tab").map((tab) => tab.textContent)).toEqual([
      "Nurses' notes",
      "Vital signs",
      "Orders",
    ]);
    expect(screen.getByText(/3 kg weight gain/)).toBeInTheDocument();

    await userEvent.click(screen.getByRole("tab", { name: "Orders" }));
    expect(
      screen.getByRole("rowheader", { name: "Furosemide 40 mg IV" }),
    ).toBeInTheDocument();
  });

  it("is a closed drawer on narrow screens, opened with a button", async () => {
    stubMatchMedia(true);
    renderPanel(caseStudyItem);

    const toggle = screen.getByRole("button", { name: "Show case details" });
    expect(toggle).toHaveAttribute("aria-expanded", "false");
    expect(screen.queryByRole("tablist")).not.toBeInTheDocument();

    await userEvent.click(toggle);

    expect(
      screen.getByRole("button", { name: "Hide case details" }),
    ).toHaveAttribute("aria-expanded", "true");
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
