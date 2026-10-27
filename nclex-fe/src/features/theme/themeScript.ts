import { themeStorageKey } from "./themePreference";

/**
 * Inline script for <head>. It runs while the HTML is parsed, before the first paint, so a saved
 * "dark" choice never flashes light first. It repeats readThemePreference/applyThemePreference in
 * plain ES5 because it runs before any bundle loads.
 */
export const themeScript = `(function(){try{var t=localStorage.getItem(${JSON.stringify(themeStorageKey)});if(t==="light"||t==="dark")document.documentElement.setAttribute("data-theme",t)}catch(e){}})()`;
