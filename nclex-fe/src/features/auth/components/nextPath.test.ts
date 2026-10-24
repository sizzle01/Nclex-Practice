import { describe, expect, it } from "vitest";
import { nextPathFrom } from "./nextPath";

describe("nextPathFrom", () => {
  it.each([
    ["?next=%2Fpractice%3Fmode%3Dtutor", "/practice?mode=tutor"],
    ["", "/dashboard"],
    ["?next=https%3A%2F%2Fevil.example", "/dashboard"],
    ["?next=%2F%2Fevil.example", "/dashboard"],
    ["?next=%2F%5Cevil.example", "/dashboard"],
  ])("%s goes to %s", (search, expected) => {
    expect(nextPathFrom(search)).toBe(expected);
  });
});
