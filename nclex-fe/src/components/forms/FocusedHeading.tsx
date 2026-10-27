"use client";

import { type ReactNode, useEffect, useRef } from "react";

/**
 * A page heading that takes keyboard focus when it appears. Used when a form is replaced by its
 * result: the button that had focus is gone, so focus moves here and screen readers read the
 * result instead of falling back to the top of the page (interfaces.md section 4).
 */
export function FocusedHeading({ id, children }: { id?: string; children: ReactNode }) {
  const ref = useRef<HTMLHeadingElement>(null);
  useEffect(() => ref.current?.focus(), []);

  return (
    <h1 id={id} ref={ref} tabIndex={-1}>
      {children}
    </h1>
  );
}
