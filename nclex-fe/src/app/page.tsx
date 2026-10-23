import Link from "next/link";

export default function HomePage() {
  return (
    <>
      <h1>NCLEX Practice</h1>
      <p>Practice questions and mock exams for NCLEX-RN and NCLEX-PN.</p>
      <p>
        <Link href="/health">System status</Link>
      </p>
    </>
  );
}
