import { ExhibitMedia } from "./ExhibitMedia";
import styles from "./exhibits.module.css";
import type { Exhibit } from "./exhibitTypes";
import { FlowsheetTable } from "./FlowsheetTable";

/** Shows one exhibit in the way its kind needs. */
export function ExhibitView({ exhibit }: { exhibit: Exhibit }) {
  switch (exhibit.kind) {
    case "table":
    case "flowsheet":
      return <FlowsheetTable exhibit={exhibit} />;
    case "image":
    case "audio":
    case "video":
      return <ExhibitMedia exhibit={exhibit} />;
    default:
      return (
        <section className={styles.text} aria-label={exhibit.title}>
          <h4 className={styles.textTitle}>{exhibit.title}</h4>
          {(exhibit.body ?? "").split(/\n{2,}/).map((paragraph, index) => (
            <p key={index}>{paragraph}</p>
          ))}
        </section>
      );
  }
}
