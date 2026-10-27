/**
 * Loads a page from scratch, as typing the URL would. Unlike router navigation, this throws away
 * everything held in memory (the access token, cached queries), which is what we want after the
 * account itself is gone. A module of its own so tests can replace it.
 */
export function hardNavigate(path: string) {
  window.location.assign(path);
}
