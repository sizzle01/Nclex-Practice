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

  it("explains a locked account and how long to wait", async () => {
    loginMock.mockRejectedValue(new ApiError(423, "ACCOUNT_LOCKED", [], "Locked.", 900));
    const user = userEvent.setup();
    renderWithAuth(<LoginForm />);

    await user.type(screen.getByLabelText("Email"), "ada@example.com");
    await user.type(screen.getByLabelText("Password"), "a-password");
    await user.click(screen.getByRole("button", { name: "Log in" }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Too many wrong passwords, so your account is locked for now. Try again in 15 minutes, or reset your password.",
    );
  });

  it("explains a rate limit and how long to wait", async () => {
    loginMock.mockRejectedValue(new ApiError(429, "RATE_LIMITED", [], "Too many requests.", 61));
    const user = userEvent.setup();
    renderWithAuth(<LoginForm />);

    await user.type(screen.getByLabelText("Email"), "ada@example.com");
    await user.type(screen.getByLabelText("Password"), "a-password");
    await user.click(screen.getByRole("button", { name: "Log in" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Too many attempts from your network. Try again in 2 minutes.");
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
