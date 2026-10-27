# UI: clinical teal-blue theme

Branch: `feature/ui-teal-blue-theme`, based on `feature/f01-accounts` (F00 and F01 are not merged to `main` yet).

## 1. Summary

The frontend gets a modern look built around a clinical teal-blue (`#0e7490`). Every colour, radius, shadow and the
font now come from design tokens in `globals.css`, with a matching dark palette that follows the operating-system
setting. The top bar is sticky, with a logo mark and avatar menu. Login and sign-up sit in a centred card, the exam choice
is shown as selectable option cards, the landing page has a hero and three highlight cards, and the dashboard opens with
a welcome banner. A three-way switch in the top bar (System / Light / Dark) lets anyone pick a theme; the choice is
remembered in the browser and applied before the first paint, so there is no flash. No route, API call or backend code
changed.

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
- Theme: use the **System / Light / Dark** switch in the top bar. Reload: the choice sticks. Choose **System**, then
  flip the OS appearance (or Chrome DevTools → Rendering → "Emulate CSS prefers-color-scheme"): the page follows it.
  Keyboard: Tab into the switch, then use the arrow keys.
- To reset: DevTools → Application → Local storage → delete `nclex-theme`.
- Mobile: DevTools device toolbar at 375px wide. The brand text, the theme labels and the user's name collapse to icons.

## 3. Reading order

1. `nclex-fe/src/app/globals.css`: design tokens as `light-dark()` pairs, the `data-theme` overrides and base styles. Start here.
2. `nclex-fe/src/app/layout.tsx`: loads Inter through `next/font` and runs the theme script in `<head>`.
3. `nclex-fe/src/features/theme/themePreference.ts`: read, save and apply the choice (pure functions).
4. `nclex-fe/src/features/theme/themeScript.ts`: the pre-paint inline script.
5. `nclex-fe/src/features/theme/useTheme.ts`: hook over localStorage (`useSyncExternalStore`), syncs across tabs.
6. `nclex-fe/src/features/theme/components/ThemeSwitcher.tsx` + `.module.css`: the segmented radio group.
7. `nclex-fe/src/components/ui/ui.module.css`: shared card, button (primary, secondary, ghost) and badge classes.
8. `nclex-fe/src/components/app-shell/TopBar.tsx` + `.module.css`: sticky header with the logo mark.
9. `nclex-fe/src/features/auth/components/UserMenu.tsx` + `.module.css`: auth links as buttons, avatar toggle.
10. `nclex-fe/src/components/app-shell/AuthCard.tsx`: centred card used by `/login` and `/signup`.
11. `nclex-fe/src/components/forms/forms.module.css`: inputs, radio cards, hints, errors, submit button.
12. `nclex-fe/src/app/page.tsx` + `page.module.css` and `src/features/home/components/HomeActions.tsx`: landing page.
13. `nclex-fe/src/features/dashboard/components/DashboardPlaceholder.tsx`: welcome banner and empty-state cards.
14. `nclex-fe/src/features/health/components/HealthStatus.module.css`: status rows as tinted pills.

## 4. Request flow

There is no new API request. A full page load now goes: the inline script in `<head>` reads `nclex-theme` from
localStorage and, for "light" or "dark", sets `data-theme` on `<html>` while the HTML is still being parsed. Then `layout.tsx` adds the Inter variable class to `<html>`, so
`--font-sans` resolves to Inter. `globals.css` sets `color-scheme` (`light dark` by default, or forced by `data-theme`), which decides which side of each
`light-dark()` token the browser uses. Each
component's CSS Module reads only tokens (`var(--accent)`, `var(--surface)`, …) and the shared `ui.module.css` classes.
`HomeActions` reads `useAuth().status` to choose between the sign-up/log-in buttons and the dashboard link.

Switching theme: clicking **Dark** fires the radio's `onChange` → `setThemePreference("dark")` saves it to
localStorage, sets `data-theme="dark"` and notifies every `useTheme` subscriber, so the radio re-renders as checked.
Other open tabs get a `storage` event and apply the same choice.

## 5. Database changes

None.

## 6. Key decisions and deviations

- **No new dependencies.** CSS Modules and custom properties only; Inter comes from the built-in `next/font/google`,
  which self-hosts the font at build time, so browsers never call Google. The build does need network access once.
- **Contrast checked** for every text/background token pair in both themes (all ≥ 4.5:1, for example white on
  `#0e7490` is 5.36:1), and input borders meet 3:1, per interfaces.md section 4. Errors and statuses still carry a
  ✗/✓/! symbol and text, not colour alone.
- **Theme choice is browser-only** (localStorage key `nclex-theme`), as agreed; the design has no profile field for it,
  so it does not follow the user to another device. "System" is stored as no value.
- **`light-dark()` instead of a duplicated dark block.** Each token lists its light and dark value side by side, and
  one `color-scheme` switch serves both System mode and the forced themes. Supported by all current browsers.
- **No flash:** the `<head>` script follows the Next.js guide "preventing flash before hydration"; `<html>` has
  `suppressHydrationWarning` because that script changes it before React hydrates. `useTheme` re-applies the saved
  choice in `useLayoutEffect` because dev Strict Mode resets `<html>` attributes on remount.
- **Hydration-safe switcher:** the server cannot see localStorage, so `useSyncExternalStore` renders "System" on the
  server and swaps to the saved value right after hydration (the page colours are already right from the script).
- **Switcher is a native radio group** in a `<fieldset>` with a visually hidden "Theme" legend, so arrow keys and
  screen-reader announcements work without custom key handling; each option has an icon and a text label.
- **Dark accent** is `#2cc4d8` with dark text on buttons (7.8:1). This replaces the `#67e8f9` from commit `9f569c6`
  on the F01 branch; the light accent `#0e7490` is unchanged.
- **Mobile user menu:** the name is visually hidden (not `display: none`), so the toggle keeps its accessible name.
- `prefers-reduced-motion` turns off the small hover and press transitions.
- The shared styles live in `ui.module.css` (the plan said `Card.module.css`) because they cover buttons and badges too.
- The landing-page highlight copy describes planned features (item types, timed exams, category results) from the
  design docs; they are not built yet.

## 7. Tests

The restyle kept the same labels, roles and text, so the existing tests cover the forms, protected route and health
status unchanged. New:

- `src/features/theme/themePreference.test.ts`: default, saved, invalid and unavailable storage; applying and
  clearing `data-theme`.
- `src/features/theme/components/ThemeSwitcher.test.tsx`: radio group named "Theme", default and saved selection,
  choosing Dark then System (attribute and storage), arrow-key navigation.

Results:
- `cd nclex-fe && npm run lint && npm run typecheck && npm test`: all pass (8 files, 32 tests). `npm run build` passes.
- `cd nclex-be && dotnet build && dotnet test`: build passes, 88 of 88 tests pass (Docker running); no backend code changed.
- Theme behaviour checked in real Chrome over the DevTools protocol: default System, click Dark (attribute, storage and
  background change), reload keeps Dark, Light forced while the OS is dark, back to System follows the OS.
- Checked by eye with headless Chrome screenshots: `/`, `/login`, `/signup`, `/health` in light and dark at 1280px, and
  `/` and `/signup` at 375px. The signed-in dashboard was not checked visually (it needs the API).

## 8. Follow-ups and known limitations

- The theme choice is per browser, not per account. Saving it to the profile would need a `users` column and an
  `/me` field: a separate feature if wanted.
- If a strict Content Security Policy is added later, the inline theme script will need a nonce.
- The signed-in dashboard and open user menu have not been screenshot-checked yet.
- Visual regression tests could come with Playwright in F16.
- Track badge and notification bell in the top bar (interfaces.md 5.2) still arrive with their own features.

## 9. Try it yourself

1. Change `--accent` and `--accent-hover` in `globals.css` to a navy (`#1e3a8a` / `#172554`) and watch every button,
   link, badge and focus ring follow. Check white text still has 4.5:1 contrast.
2. Add a `.buttonDanger` class to `ui.module.css` using `--error`, and use it for "Log out on all devices".
3. Add a fourth highlight card on the landing page; the grid wraps it automatically.
4. Add a fourth theme, "High contrast": extend `ThemePreference`, the script's allowed values and the switcher
   options, then add `:root[data-theme="high-contrast"]` overrides in `globals.css`. Update the tests first.
