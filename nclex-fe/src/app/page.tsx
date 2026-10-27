import Link from "next/link";
import ui from "@/components/ui/ui.module.css";
import { HomeActions } from "@/features/home/components/HomeActions";
import styles from "./page.module.css";

const highlights = [
  {
    title: "NGN-style questions",
    body: "Practise every NCLEX item type, from multiple choice to case studies, with rationales.",
  },
  {
    title: "Timed mock exams",
    body: "Sit full-length exams under real conditions and resume any time you need to stop.",
  },
  {
    title: "Track your progress",
    body: "See your results by client-needs category and focus on where you need it most.",
  },
];

export default function HomePage() {
  return (
    <>
      <section className={styles.hero} aria-labelledby="home-heading">
        <span className={ui.badge}>NCLEX-RN &amp; NCLEX-PN</span>
        <h1 id="home-heading" className={styles.title}>
          Prepare for the NCLEX with <span className={styles.highlight}>confidence</span>
        </h1>
        <p className={styles.lead}>
          Practice questions and mock exams for <span className={styles.nowrap}>NCLEX-RN</span> and{" "}
          <span className={styles.nowrap}>NCLEX-PN</span>.
        </p>
        <HomeActions className={styles.actions} />
      </section>

      <section aria-label="Highlights" className={styles.grid}>
        {highlights.map((item) => (
          <article key={item.title} className={ui.card}>
            <span className={styles.icon} aria-hidden="true">
              ✓
            </span>
            <h2 className={ui.cardTitle}>{item.title}</h2>
            <p className={`${ui.muted} ${styles.cardBody}`}>{item.body}</p>
          </article>
        ))}
      </section>

      <p className={styles.status}>
        <Link href="/health">System status</Link>
      </p>
    </>
  );
}
