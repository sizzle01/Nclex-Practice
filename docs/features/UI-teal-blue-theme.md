# UI: clinical teal-blue theme

Branch: `feature/ui-teal-blue-theme`, based on `feature/f01-accounts` (F00 and F01 are not merged to `main` yet).

## 1. Summary

The frontend gets a modern look built around a clinical teal-blue (`#0e7490`). Every colour, radius, shadow and the
font now come from design tokens in `globals.css`, with a matching dark palette that follows the operating-system
setting. The top bar is sticky, with a logo mark and avatar menu. Login and sign-up sit in a centred card, the exam choice
is shown as selectable option cards, the landing page has a hero and three highlight cards, and the dashboard opens with
a welcome banner. No behaviour, route, API call or backend code changed.

## 2. How to try it

```bash
cd nclex-fe
npm install
npm run dev            # http://localhost:3000
```

- `/`: hero and highlights. Signed in, the buttons switch to **Go to your dashboard**.
- `/login`, `/signup`: submit the empty form to see the error styles; Tab through the fields to see the focus rings.
- `/health`: status card (with the API down it shows "✗ Cannot reach the API").
- `/dashboard`: needs the backend and an account (see F01 walkthrough, section 2).
- Dark mode: switch the OS appearance to dark, or in Chrome DevTools use Rendering → "Emulate CSS prefers-color-scheme".
- Mobile: DevTools device toolbar at 375px wide. The user's name in the top bar collapses to the avatar.

## 3. Reading order

1. `nclex-fe/src/app/globals.css`: design tokens (light and dark) and base element styles. Start here.
2. `nclex-fe/src/app/layout.tsx`: loads Inter through `next/font` and exposes it as `--font-inter`.
3. `nclex-fe/src/components/ui/ui.module.css`: shared card, button (primary, secondary, ghost) and badge classes.
4. `nclex-fe/src/components/app-shell/TopBar.tsx` + `.module.css`: sticky header with the logo mark.
5. `nclex-fe/src/features/auth/components/UserMenu.tsx` + `.module.css`: auth links as buttons, avatar toggle.
6. `nclex-fe/src/components/app-shell/AuthCard.tsx`: centred card used by `/login` and `/signup`.
7. `nclex-fe/src/components/forms/forms.module.css`: inputs, radio cards, hints, errors, submit button.
8. `nclex-fe/src/app/page.tsx` + `page.module.css` and `src/features/home/components/HomeActions.tsx`: landing page.
9. `nclex-fe/src/features/dashboard/components/DashboardPlaceholder.tsx`: welcome banner and empty-state cards.
10. `nclex-fe/src/features/health/components/HealthStatus.module.css`: status rows as tinted pills.

## 4. Request flow

There is no new request. A page render now goes: `layout.tsx` adds the Inter variable class to `<html>`, so
`--font-sans` resolves to Inter. `globals.css` picks the light or dark token set from `prefers-color-scheme`. Each
component's CSS Module reads only tokens (`var(--accent)`, `var(--surface)`, …) and the shared `ui.module.css` classes.
`HomeActions` is the only new client logic: it reads `useAuth().status` to choose between the sign-up/log-in buttons
and the dashboard link.

## 5. Database changes

None.

## 6. Key decisions and deviations

- **No new dependencies.** CSS Modules and custom properties only; Inter comes from the built-in `next/font/google`,
  which self-hosts the font at build time, so browsers never call Google. The build does need network access once.
- **Contrast checked** for every text/background token pair in both themes (all ≥ 4.5:1, for example white on
  `#0e7490` is 5.36:1), and input borders meet 3:1, per interfaces.md section 4. Errors and statuses still carry a
  ✗/✓/! symbol and text, not colour alone.
- **Dark accent** is `#2cc4d8` with dark text on buttons (7.8:1). This replaces the `#67e8f9` from commit `9f569c6`
  on the F01 branch; the light accent `#0e7490` is unchanged.
- **Mobile user menu:** the name is visually hidden (not `display: none`), so the toggle keeps its accessible name.
- `prefers-reduced-motion` turns off the small hover and press transitions.
- The shared styles live in `ui.module.css` (the plan said `Card.module.css`) because they cover buttons and badges too.
- The landing-page highlight copy describes planned features (item types, timed exams, category results) from the
  design docs; they are not built yet.

## 7. Tests

No test changes: markup kept the same labels, roles and text, so the existing tests cover the forms, protected route
and health status unchanged.

- `cd nclex-fe && npm run lint && npm run typecheck && npm test`: all pass (6 files, 22 tests). `npm run build` passes.
- `cd nclex-be && dotnet build`: passes. `dotnet test`: 31 passed, 57 failed only because Docker was not running
  (Testcontainers `DockerUnavailableException`); no backend code changed.
- Checked by eye with headless Chrome screenshots: `/`, `/login`, `/signup`, `/health` in light and dark at 1280px, and
  `/` and `/signup` at 375px. The signed-in dashboard was not checked visually (it needs the API).

## 8. Follow-ups and known limitations

- No manual light/dark toggle; the theme follows the OS setting.
- The signed-in dashboard and open user menu have not been screenshot-checked yet.
- Visual regression tests could come with Playwright in F16.
- Track badge and notification bell in the top bar (interfaces.md 5.2) still arrive with their own features.

## 9. Try it yourself

1. Change `--accent` and `--accent-hover` in `globals.css` to a navy (`#1e3a8a` / `#172554`) and watch every button,
   link, badge and focus ring follow. Check white text still has 4.5:1 contrast.
2. Add a `.buttonDanger` class to `ui.module.css` using `--error`, and use it for "Log out on all devices".
3. Add a fourth highlight card on the landing page; the grid wraps it automatically.
