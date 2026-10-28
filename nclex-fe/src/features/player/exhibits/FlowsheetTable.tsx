import styles from "./exhibits.module.css";
import type { Exhibit } from "./exhibitTypes";

/**
 * A table or flowsheet exhibit. Row labels are row headers and the columns (timestamps, for a
 * flowsheet) are column headers, so a screen reader reads each value with its parameter and time.
 * Wide flowsheets scroll inside their own box, which is focusable so keyboard users can scroll it.
 */
export function FlowsheetTable({ exhibit }: { exhibit: Exhibit }) {
  const table = exhibit.table;
  if (!table) return null;
  return (
    <div className={styles.tableScroll} role="region" aria-label={exhibit.title} tabIndex={0}>
      <table className={styles.table}>
        <caption className={styles.caption}>{exhibit.title}</caption>
        <thead>
          <tr>
            <th scope="col">{exhibit.kind === "flowsheet" ? "Parameter" : ""}</th>
            {table.columns.map((column, index) => (
              <th key={index} scope="col">
                {column}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {table.rows.map((row, r) => (
            <tr key={r}>
              <th scope="row">{row.label}</th>
              {row.cells.map((cell, c) => (
                <td key={c}>{cell}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
