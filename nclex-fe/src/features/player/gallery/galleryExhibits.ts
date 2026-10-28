import type { MediaResolver } from "../media/MediaContext";
import type { GalleryItem } from "./galleryTypes";
import { sampleItems } from "./sampleItems";

/** Local stand-ins for the CDN, served from public/dev-media. */
export const galleryMedia: MediaResolver = (mediaId) =>
  ({
    "wound-photo": {
      url: "/dev-media/wound.svg",
      altText: "Oval sacral wound about 4 cm across: red granulation tissue with a small yellow centre of slough.",
    },
    "heart-sounds": {
      url: "/dev-media/heart-sounds.wav",
      captionsUrl: "/dev-media/heart-sounds.vtt",
      transcriptUrl: "/dev-media/heart-sounds-transcript.txt",
    },
  })[mediaId] ?? null;

const multipleChoice = sampleItems.find((item) => item.itemType === "multiple_choice")!;
const matrix = sampleItems.find((item) => item.itemType === "matrix_mc")!;

/** A trend item: multiple choice with a vital-signs flowsheet, a wound photo and heart sounds. */
export const exhibitItem: GalleryItem = {
  ...multipleChoice,
  exhibits: [
    {
      id: "vitals",
      kind: "flowsheet",
      title: "Vital signs",
      table: {
        columns: ["08:00", "12:00", "16:00", "20:00"],
        rows: [
          { label: "Temperature (°C)", cells: ["37.0", "37.6", "38.3", "38.9"] },
          { label: "Heart rate (/min)", cells: ["84", "96", "108", "118"] },
          { label: "Blood pressure (mm Hg)", cells: ["128/76", "120/70", "108/64", "96/58"] },
          { label: "Respiratory rate (/min)", cells: ["16", "18", "22", "26"] },
          { label: "SpO₂ (%)", cells: ["97", "96", "94", "92"] },
        ],
      },
    },
    { id: "wound", kind: "image", title: "Sacral wound", mediaId: "wound-photo" },
    { id: "heart", kind: "audio", title: "Heart sounds", mediaId: "heart-sounds" },
  ],
};

/** An unfolding case: a matrix item with the case's nurses' notes, vitals and orders tabs. */
export const caseStudyItem: GalleryItem = {
  ...matrix,
  caseStudy: {
    id: "00000000-0000-0000-0000-000000000001",
    title: "Heart failure exacerbation",
    position: 2,
    scenario: {
      tabs: [
        {
          id: "notes",
          title: "Nurses' notes",
          exhibits: [
            {
              id: "notes-0700",
              kind: "text",
              title: "07:00",
              body: "72-year-old client admitted with shortness of breath and a 3 kg weight gain over 4 days.\n\nReports needing three pillows to sleep. Crackles in both lung bases; 2+ pitting edema of both ankles.",
            },
          ],
        },
        {
          id: "vitals",
          title: "Vital signs",
          exhibits: [
            {
              id: "case-vitals",
              kind: "flowsheet",
              title: "Vital signs",
              table: {
                columns: ["07:00", "11:00"],
                rows: [
                  { label: "Heart rate (/min)", cells: ["104", "98"] },
                  { label: "Blood pressure (mm Hg)", cells: ["152/94", "144/88"] },
                  { label: "SpO₂ (%)", cells: ["90", "93"] },
                ],
              },
            },
          ],
        },
        {
          id: "orders",
          title: "Orders",
          exhibits: [
            {
              id: "orders-list",
              kind: "table",
              title: "Provider orders",
              table: {
                columns: ["Order", "Frequency"],
                rows: [
                  { label: "Furosemide 40 mg IV", cells: ["Now, then every 12 hours"] },
                  { label: "Daily weight", cells: ["Every morning before breakfast"] },
                ],
              },
            },
          ],
        },
      ],
    },
  },
};
