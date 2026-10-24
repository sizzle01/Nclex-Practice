import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "@/lib/api/errors";
import { login } from "../api/authApi";
import { renderWithAuth } from "../test/renderWithAuth";
import { LoginForm } from "./LoginForm";

vi.mock("../api/authApi", () => ({ login: vi.fn() }));
const replace = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ replace }) }));
const loginMock = vi.mocked(login);

describe("LoginForm", () => {
  beforeEach(() => {
    loginMock.mockReset();
    replace.mockReset();
    window.history.replaceState(null, "", "/login");
  });

  it("shows a generic message when the credentials are wrong", async () => {
    loginMock.mockRejectedValue(new ApiError(401, "INVALID_CREDENTIALS", [], "The email or password is incorrect."));
    const user = userEvent.setup();
    renderWithAuth(<LoginForm />);

    await user.type(screen.getByLabelText("Email"), "ada@example.com");
    await user.type(screen.getByLabelText("Password"), "wrong-password");
    await user.click(screen.getByRole("button", { name: "Log in" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("The email or password is incorrect.");
    expect(replace).not.toHaveBeenCalled();
  });

  it("returns to the page in ?next= after logging in", async () => {
    window.history.replaceState(null, "", "/login?next=%2Fpractice");
    loginMock.mockResolvedValue({} as never);
    const user = userEvent.setup();
    renderWithAuth(<LoginForm />);

    await user.type(screen.getByLabelText("Email"), "ada@example.com");
    await user.type(screen.getByLabelText("Password"), "correct-horse-battery");
    await user.click(screen.getByRole("button", { name: "Log in" }));

    await waitFor(() => expect(replace).toHaveBeenCalledWith("/practice"));
  });
});
