/// <summary>Applies the shared Ranvier Brand Design System across all repository collections.</summary>
module internal Ranvier.Docs.BrandTheme

open Nacara.Plugins.Ranvier.Theme

let private brandTokensAndPalette =
    """/* Ranvier brand tokens v1.1 + Nacara theme & syntax bindings */
:root,
:root[data-theme="light"] {
  --rv-canvas: #F7F9FB;
  --rv-surface: #FFFFFF;
  --rv-raised: #EFF3F7;
  --rv-border: #CBD7E0;
  --rv-border-strong: #7C91A0;
  --rv-text: #17232D;
  --rv-muted: #526574;
  --rv-contour: #087F99;
  --rv-accent-text: #08758F;
  --rv-violet: #6953C5;
  --rv-pending: #A85D11;
  --rv-value: #187E54;
  --rv-error: #AD3543;
  --rv-on-accent: #FFFFFF;
  --rv-grad-a: #08758F;
  --rv-grad-b: #376BAF;
  --rv-grad-c: #6953C5;
  --rv-gradient-accent: linear-gradient(115deg, #08758F 0%, #376BAF 53%, #6953C5 100%);
  --rv-code-bg: var(--rv-surface);
  --rv-code-title-bg: var(--rv-raised);
  --nacara-shadow: 0 1px 2px rgb(23 35 45 / 6%), 0 2px 8px rgb(23 35 45 / 5%);
  --nacara-shadow-floating: 0 12px 32px -8px rgb(23 35 45 / 18%), 0 2px 6px rgb(23 35 45 / 6%);
  --tok-string: #1F7A5A;
}

@media (prefers-color-scheme: dark) {
  :root:not([data-theme="light"]) {
    --rv-canvas: #0D1118;
    --rv-surface: #171E28;
    --rv-raised: #202936;
    --rv-border: #33414F;
    --rv-border-strong: #586B78;
    --rv-text: #EBF3F7;
    --rv-muted: #9BAAB5;
    --rv-contour: #64D9EE;
    --rv-accent-text: #64D9EE;
    --rv-violet: #9D8CFF;
    --rv-pending: #F4B971;
    --rv-value: #7CE3B0;
    --rv-error: #F4868B;
    --rv-on-accent: #0D1118;
    --rv-grad-a: #64D9EE;
    --rv-grad-b: #6D9FFB;
    --rv-grad-c: #9D8CFF;
    --rv-gradient-accent: linear-gradient(115deg, #64D9EE 0%, #6D9FFB 53%, #9D8CFF 100%);
    --rv-code-bg: color-mix(in oklab, var(--rv-surface) 60%, var(--rv-canvas));
    --rv-code-title-bg: var(--rv-surface);
    --nacara-shadow: 0 1px 2px rgb(0 0 0 / 45%), 0 4px 16px rgb(0 0 0 / 30%);
    --nacara-shadow-floating: 0 16px 40px -8px rgb(0 0 0 / 55%), 0 0 0 1px var(--rv-border);
    --tok-string: #9FDDBF;
  }
}

:root[data-theme="dark"] {
  --rv-canvas: #0D1118;
  --rv-surface: #171E28;
  --rv-raised: #202936;
  --rv-border: #33414F;
  --rv-border-strong: #586B78;
  --rv-text: #EBF3F7;
  --rv-muted: #9BAAB5;
  --rv-contour: #64D9EE;
  --rv-accent-text: #64D9EE;
  --rv-violet: #9D8CFF;
  --rv-pending: #F4B971;
  --rv-value: #7CE3B0;
  --rv-error: #F4868B;
  --rv-on-accent: #0D1118;
  --rv-grad-a: #64D9EE;
  --rv-grad-b: #6D9FFB;
  --rv-grad-c: #9D8CFF;
  --rv-gradient-accent: linear-gradient(115deg, #64D9EE 0%, #6D9FFB 53%, #9D8CFF 100%);
  --rv-code-bg: color-mix(in oklab, var(--rv-surface) 60%, var(--rv-canvas));
  --rv-code-title-bg: var(--rv-surface);
  --nacara-shadow: 0 1px 2px rgb(0 0 0 / 45%), 0 4px 16px rgb(0 0 0 / 30%);
  --nacara-shadow-floating: 0 16px 40px -8px rgb(0 0 0 / 55%), 0 0 0 1px var(--rv-border);
  --tok-string: #9FDDBF;
}

:root,
:root[data-theme="light"],
:root[data-theme="dark"] {
  --rv-ease: cubic-bezier(.2, .8, .2, 1);
  --rv-font-display: "Geist", ui-sans-serif, system-ui, sans-serif;
  --nacara-font-sans: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", "Noto Sans", "Liberation Sans", Arial, sans-serif;
  --nacara-font-mono: "Geist Mono", ui-monospace, "Cascadia Code", "JetBrains Mono", SFMono-Regular, Menlo, Consolas, "Liberation Mono", monospace;
  --nacara-radius: 0.625rem;
  --nacara-radius-sm: 0.375rem;
  --nacara-content-width: 46rem;
  --nacara-sidebar-width: 16.5rem;
  --nacara-toc-width: 14rem;
  --nacara-navbar-opacity: 76%;
  --nacara-navbar-blur: 12px;
  --nacara-bg: var(--rv-canvas);
  --nacara-bg-subtle: var(--rv-surface);
  --nacara-bg-raised: var(--rv-raised);
  --nacara-border: var(--rv-border);
  --nacara-text: var(--rv-text);
  --nacara-text-muted: var(--rv-muted);
  --nacara-heading: var(--rv-text);
  --nacara-primary: var(--rv-accent-text);
  --nacara-primary-contrast: var(--rv-on-accent);
  --nacara-primary-subtle: color-mix(in oklab, var(--rv-contour) 14%, var(--rv-canvas));
  --nacara-note: var(--rv-accent-text);
  --nacara-tip: var(--rv-value);
  --nacara-warning: var(--rv-pending);
  --nacara-danger: var(--rv-error);
  --nacara-code-inline-bg: var(--rv-raised);
  --nacara-code-inline-border: var(--rv-border);
  --nacara-code-inline-text: var(--rv-text);
  --tok-comment: var(--rv-muted);
  --tok-number: var(--rv-violet);
  --tok-constant: var(--rv-violet);
  --tok-constructor: var(--rv-grad-b);
  --tok-property: var(--rv-text);
  --tok-escape: var(--rv-accent-text);
  --tok-keyword: var(--rv-violet);
  --tok-operator: var(--rv-muted);
  --tok-function: var(--rv-grad-b);
  --tok-type: var(--rv-accent-text);
  --tok-namespace: var(--rv-accent-text);
  --tok-variable: var(--rv-text);
  --tok-parameter: var(--rv-text);
  --tok-punctuation: var(--rv-muted);
  --tok-tag: var(--rv-accent-text);
  --tok-attribute: var(--rv-violet);
  --tok-preprocessor: var(--rv-violet);
  --tok-invalid: var(--rv-error);
  --tok-inserted: var(--rv-value);
  --tok-deleted: var(--rv-error);
}
.rv-accent-fill { background: var(--rv-gradient-accent); color: var(--rv-on-accent); }
"""

let private brandChromeAndComponents =
    """@media (min-width: 1600px) {
  :root { --nacara-content-width: 48rem; }
}
html {
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  scroll-padding-top: calc(var(--nacara-navbar-height) + 1.5rem);
}
body { line-height: 1.7; }
::selection { background: color-mix(in oklab, var(--nacara-primary) 24%, transparent); }
:focus-visible { outline: 2px solid var(--nacara-primary); outline-offset: 2px; }

.nacara-navbar {
  background: color-mix(in oklab, var(--nacara-bg) var(--nacara-navbar-opacity), transparent);
  backdrop-filter: saturate(160%) blur(var(--nacara-navbar-blur));
  -webkit-backdrop-filter: saturate(160%) blur(var(--nacara-navbar-blur));
  border-bottom: 1px solid var(--nacara-border);
  box-shadow: none;
}
.nacara-navbar__brand {
  display: inline-flex;
  align-items: baseline;
  gap: 0;
  flex: none;
  margin-inline-end: 1.5rem;
  font-family: var(--rv-font-display);
  font-size: 1.3125rem;
  font-weight: 600;
  letter-spacing: -.035em;
  text-transform: lowercase;
  color: var(--nacara-heading);
  text-decoration: none;
}
.nacara-navbar__brand::after {
  content: "";
  width: .3rem;
  height: .3rem;
  margin-inline-start: .15rem;
  border-radius: 50%;
  background: var(--rv-contour);
}
.nacara-navbar__link {
  font-size: .875rem;
  font-weight: 500;
  color: var(--nacara-text-muted);
  padding: .375rem .75rem;
  border-radius: 999px;
  transition: color .15s, background-color .15s;
}
.nacara-navbar__link:hover {
  color: var(--nacara-text);
  background: var(--nacara-bg-raised);
  text-decoration: none;
}
.nacara-navbar__link[data-active]:not([data-active="false"]),
.nacara-navbar__link[aria-current="page"] {
  color: var(--nacara-text);
  background: var(--nacara-bg-raised);
}
.nacara-sidebar__sections a[aria-current]:not([aria-current="false"]) {
  color: var(--nacara-primary);
  font-weight: 550;
}
.nacara-sidebar__title {
  font-size: .75rem;
  font-weight: 600;
  text-transform: none;
  letter-spacing: 0;
  color: var(--nacara-text-muted);
  margin: 1.75rem 0 .375rem;
  padding-inline: .625rem;
}
.nacara-sidebar__list > li:first-child > .nacara-sidebar__section > .nacara-sidebar__title { margin-top: 0; }
.nacara-sidebar__section { margin-bottom: 0; }
.nacara-sidebar__sections { margin-bottom: 0; padding-bottom: 0; }
.nacara-sidebar__link,
.nacara-sidebar__group-link {
  font-size: .875rem;
  line-height: 1.45;
  color: var(--nacara-text-muted);
  padding: .3125rem .625rem;
  border-radius: var(--nacara-radius-sm);
  transition: color .15s, background-color .15s;
}
.nacara-sidebar__link:hover,
.nacara-sidebar__group-link:hover {
  color: var(--nacara-text);
  background: var(--nacara-bg-subtle);
  text-decoration: none;
}
.nacara-sidebar__link[aria-current="page"],
.nacara-sidebar__group-link[aria-current="page"] {
  color: var(--nacara-primary);
  background: var(--nacara-primary-subtle);
  box-shadow: inset 2px 0 var(--rv-contour);
  font-weight: 550;
}
.nacara-sidebar__filter {
  border: 1px solid var(--nacara-border);
  border-radius: var(--nacara-radius-sm);
  background: var(--nacara-bg-subtle);
  font-size: .8125rem;
}
.nacara-badge {
  border-radius: 999px;
  font-size: .6875rem;
  font-weight: 600;
  padding: .125rem .5rem;
}

.nacara-toc__title {
  font-size: .75rem;
  font-weight: 600;
  text-transform: none;
  letter-spacing: 0;
  color: var(--nacara-text-muted);
}
.nacara-toc__list { border-left: 1px solid var(--nacara-border); }
.nacara-toc__link {
  font-size: .8125rem;
  color: var(--nacara-text-muted);
  line-height: 1.45;
  padding: .3125rem 0 .3125rem .875rem;
  margin-left: -1px;
  border-left: 1px solid transparent;
  transition: color .15s, border-color .15s;
}
.nacara-toc__link[data-active="true"] {
  color: var(--nacara-primary);
  border-left-color: var(--nacara-primary);
}

.nacara-content h1 {
  font-size: clamp(2rem, 1.6rem + 1.6vw, 2.5rem);
  font-weight: 650;
  line-height: 1.15;
  letter-spacing: -.025em;
  color: var(--nacara-heading);
}
.nacara-content :is(h1, h2, h3, h4),
.nacara-navbar__link,
.nacara-sidebar__title,
.nacara-toc__title,
.nacara-page-nav__label,
.rv-btn {
  font-family: var(--rv-font-display);
}
.nacara-content h2 {
  font-size: 1.5rem;
  font-weight: 600;
  line-height: 1.25;
  letter-spacing: -.02em;
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px solid var(--nacara-border);
  background: linear-gradient(var(--rv-contour), var(--rv-contour)) left -1px / 2.5rem 2px no-repeat;
}
.nacara-content > div > h2:first-child {
  border-top: 0;
  padding-top: 0;
  background: none;
  margin-top: 1.5rem;
}
.nacara-content h3 {
  font-size: 1.1875rem;
  font-weight: 600;
  line-height: 1.35;
  margin-top: 2.25rem;
}
.nacara-content h4 { font-size: 1rem; font-weight: 600; }
.nacara-content .nacara-heading-anchor { color: var(--nacara-text-muted); }
.nacara-content .nacara-heading-anchor:hover { color: var(--nacara-primary); }
.nacara-content > h1 + div > p:first-child {
  font-size: 1.125rem;
  line-height: 1.65;
  color: var(--nacara-text-muted);
}
.nacara-content a:not([class]) {
  color: var(--nacara-primary);
  text-decoration: underline;
  text-decoration-color: color-mix(in oklab, var(--nacara-primary) 40%, transparent);
  text-underline-offset: .2em;
  text-decoration-thickness: 1px;
  transition: text-decoration-color .15s;
}
.nacara-content a:not([class]):hover { text-decoration-color: currentColor; }
.nacara-content :not(pre) > code {
  font-size: .875em;
  padding: .1em .35em;
  border-radius: .3125rem;
  background: var(--nacara-code-inline-bg);
  border: 1px solid var(--nacara-code-inline-border);
  color: var(--nacara-code-inline-text);
  -webkit-box-decoration-break: clone;
  box-decoration-break: clone;
}
.nacara-content :is(p, li, blockquote) > code { overflow-wrap: anywhere; }
.nacara-content :is(h1, h2, h3, h4) > code { font-size: .9em; font-weight: inherit; }
.nacara-content a:not([class]) > code { color: inherit; }

.nacara-callout {
  margin: 1.75rem 0;
  padding: .875rem 1rem .875rem 1.125rem;
  font-size: .9375rem;
  border: 1px solid color-mix(in oklab, var(--callout-color) 30%, var(--nacara-border));
  border-left: 3px solid var(--callout-color);
  border-radius: var(--nacara-radius-sm) var(--nacara-radius) var(--nacara-radius) var(--nacara-radius-sm);
  background: color-mix(in oklab, var(--callout-color) 7%, var(--nacara-bg));
}
.nacara-callout[data-title]::before {
  color: var(--callout-color);
  font-weight: 600;
  font-size: .8125rem;
}

.nacara-details {
  margin: 1.75rem 0;
  background: var(--nacara-bg);
  border-radius: var(--nacara-radius);
}
.nacara-details > summary {
  font-weight: 550;
  font-size: .9375rem;
  padding: .625rem 1rem;
  transition: background-color .15s;
}
.nacara-details > summary:hover { background: var(--nacara-bg-subtle); }

.nacara-content table {
  width: fit-content;
  max-width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  font-size: .875rem;
  border: 1px solid var(--nacara-border);
  border-radius: var(--nacara-radius);
  background: var(--nacara-bg-subtle);
}
.nacara-content th,
.nacara-content td {
  border-width: 0 0 1px;
  border-style: solid;
  border-color: var(--nacara-border);
  padding: .625rem .875rem;
}
.nacara-content th {
  background: var(--nacara-bg-raised);
  font-weight: 600;
  font-size: .8125rem;
  text-align: left;
}
.nacara-content tr:last-child td { border-bottom: 0; }
.nacara-content tbody tr:hover td { background: color-mix(in oklab, var(--rv-contour) 5%, transparent); }
.nacara-content thead tr:first-child th:first-child { border-top-left-radius: calc(var(--nacara-radius) - 1px); }
.nacara-content thead tr:first-child th:last-child { border-top-right-radius: calc(var(--nacara-radius) - 1px); }

.nacara-code {
  border: 1px solid var(--nacara-border);
  border-radius: var(--nacara-radius);
  background: var(--rv-code-bg);
  box-shadow: inset 2px 0 color-mix(in oklab, var(--rv-contour) 55%, transparent);
  margin: 1.75rem 0;
}
.nacara-code__title {
  background: var(--rv-code-title-bg);
  font-size: .75rem;
  color: var(--nacara-text-muted);
}
.nacara-code pre,
.nacara-code code {
  font-size: .84375rem;
  line-height: 1.7;
}
.nacara-code__line[data-marker="mark"] {
  background: color-mix(in oklab, var(--nacara-primary) 10%, transparent);
  box-shadow: inset 2px 0 var(--nacara-primary);
}
.nacara-code__copy {
  border: 1px solid var(--nacara-border);
  background: var(--nacara-bg-raised);
  border-radius: var(--nacara-radius-sm);
}

nacara-tabs {
  background: var(--rv-code-bg);
  margin: 1.75rem 0;
}
.nacara-tabs__list {
  gap: 0;
  padding: 0 .5rem;
  background: var(--rv-code-title-bg);
}
.nacara-tabs__tab {
  font-size: .8125rem;
  font-weight: 500;
  padding: .625rem .75rem;
  border-radius: 0;
  box-shadow: inset 0 -2px transparent;
  transition: color .15s, box-shadow .15s;
}
.nacara-tabs__tab:hover { color: var(--nacara-text); }
.nacara-tabs__tab[aria-selected="true"] {
  background: none;
  color: var(--nacara-text);
  box-shadow: inset 0 -2px var(--nacara-primary);
}
nacara-tab:has(> .nacara-code:only-child) { padding: 0; }
nacara-tab > .nacara-code:only-child { margin: 0; border: 0; border-radius: 0; }

.nacara-page-nav__link {
  border: 1px solid var(--nacara-border);
  border-radius: var(--nacara-radius);
  padding: 1rem 1.25rem;
  transition: border-color .15s;
}
.nacara-page-nav__link:hover { border-color: var(--rv-border-strong); text-decoration: none; }
.nacara-page-nav__label { font-size: .75rem; color: var(--nacara-text-muted); }
.nacara-edit-link { font-size: .8125rem; color: var(--nacara-text-muted); }
.nacara-footer {
  border-top: 1px solid var(--nacara-border);
  color: var(--nacara-text-muted);
  font-size: .8125rem;
  padding: 2.5rem 1.5rem;
}
.nacara-footer a { color: var(--nacara-text-muted); }
.nacara-footer a:hover { color: var(--nacara-text); }

.rv-btn {
  display: inline-flex;
  align-items: center;
  gap: .5rem;
  height: 2.625rem;
  padding: 0 1.25rem;
  border-radius: var(--nacara-radius-sm);
  font: inherit;
  font-size: .9375rem;
  font-weight: 600;
  text-decoration: none;
  cursor: pointer;
  border: 1px solid transparent;
  transition: background-color .15s, border-color .15s, filter .15s, transform .15s var(--rv-ease);
}
.rv-btn:hover { text-decoration: none; }
.rv-btn--primary,
.nacara-content a.rv-btn--primary {
  background: var(--rv-accent-text);
  color: var(--rv-on-accent);
}
.rv-btn--primary:hover { filter: brightness(1.06); }
.rv-btn--secondary,
.nacara-content a.rv-btn--secondary {
  background: var(--nacara-bg-subtle);
  color: var(--nacara-text);
  border-color: var(--rv-border);
}
.rv-btn--secondary:hover { border-color: var(--rv-border-strong); }
.rv-btn:active { transform: translateY(1px); }

.rv-badge {
  display: inline-flex;
  align-items: center;
  padding: .0625rem .5rem;
  border-radius: 999px;
  font-size: .75rem;
  font-weight: 600;
  line-height: 1.5;
  color: var(--rv-muted);
  background: var(--rv-raised);
  border: 1px solid var(--rv-border);
}

.rv-state {
  --rv-state-size: 1.5rem;
  display: inline-flex;
  align-items: center;
  gap: .4375rem;
  padding: .125rem .625rem .125rem .3125rem;
  border: 1px solid var(--nacara-border);
  border-radius: 999px;
  background: var(--nacara-bg-subtle);
  color: var(--nacara-text);
  font-size: .8125rem;
  font-weight: 500;
  line-height: 1.5;
  white-space: nowrap;
  vertical-align: middle;
}
.rv-state::before {
  content: "";
  flex: none;
  width: var(--rv-state-size);
  height: var(--rv-state-size);
  background: var(--rv-state-icon) center / contain no-repeat;
}
.rv-state--sm { --rv-state-size: 1rem; }
.rv-state--plain { padding: 0; border: 0; background: none; }
.rv-states { display: flex; flex-wrap: wrap; gap: .5rem; margin: 1rem 0; }
.rv-state--ready { --rv-state-icon: url("__BASE__brand/states/ready-dark.svg"); }
.rv-state--pending { --rv-state-icon: url("__BASE__brand/states/pending-dark.svg"); }
.rv-state--retained { --rv-state-icon: url("__BASE__brand/states/retained-dark.svg"); }
.rv-state--fallback { --rv-state-icon: url("__BASE__brand/states/fallback-dark.svg"); }
.rv-state--failed { --rv-state-icon: url("__BASE__brand/states/failed-dark.svg"); }
.rv-state--recovered { --rv-state-icon: url("__BASE__brand/states/recovered-dark.svg"); }
:root[data-theme="light"] .rv-state--ready { --rv-state-icon: url("__BASE__brand/states/ready-light.svg"); }
:root[data-theme="light"] .rv-state--pending { --rv-state-icon: url("__BASE__brand/states/pending-light.svg"); }
:root[data-theme="light"] .rv-state--retained { --rv-state-icon: url("__BASE__brand/states/retained-light.svg"); }
:root[data-theme="light"] .rv-state--fallback { --rv-state-icon: url("__BASE__brand/states/fallback-light.svg"); }
:root[data-theme="light"] .rv-state--failed { --rv-state-icon: url("__BASE__brand/states/failed-light.svg"); }
:root[data-theme="light"] .rv-state--recovered { --rv-state-icon: url("__BASE__brand/states/recovered-light.svg"); }
@media (forced-colors: active) {
  .rv-state--ready { --rv-state-icon: url("__BASE__brand/states/ready-mono.svg"); }
  .rv-state--pending { --rv-state-icon: url("__BASE__brand/states/pending-mono.svg"); }
  .rv-state--retained { --rv-state-icon: url("__BASE__brand/states/retained-mono.svg"); }
  .rv-state--fallback { --rv-state-icon: url("__BASE__brand/states/fallback-mono.svg"); }
  .rv-state--failed { --rv-state-icon: url("__BASE__brand/states/failed-mono.svg"); }
  .rv-state--recovered { --rv-state-icon: url("__BASE__brand/states/recovered-mono.svg"); }
}

.rv-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(15rem, 1fr)); gap: 1rem; margin: 1.5rem 0 3rem; }
.nacara-content .rv-card {
  display: flex; flex-direction: column; padding: 1.25rem 1.25rem 1.375rem;
  border: 1px solid var(--nacara-border); border-radius: var(--nacara-radius);
  background: var(--nacara-bg-subtle); color: var(--nacara-text); text-decoration: none;
  transition: border-color .2s, box-shadow .2s;
}
.nacara-content a.rv-card:hover { border-color: var(--rv-border-strong); box-shadow: inset 0 2px var(--rv-contour); text-decoration: none; }
.rv-card__title {
  display: flex; align-items: center; justify-content: space-between; gap: .5rem; margin: 0 0 .5rem;
  font-family: var(--rv-font-display); font-size: 1rem; font-weight: 600; color: var(--nacara-heading);
}
.nacara-content .rv-card p { margin: 0; font-size: .875rem; line-height: 1.6; color: var(--nacara-text-muted); }

@media (max-width: 40rem) {
  .nacara-navbar__brand { margin-inline-end: .5rem; }
}
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    transition-duration: 0s !important;
    animation-duration: 0s !important;
  }
}
.nacara-content :is(td, th)[style*="text-align: right"] { white-space: nowrap; }
@supports not selector(::-webkit-scrollbar) {
  .nacara-code pre, .nacara-content table, .rv-demo__code pre {
    scrollbar-width: thin;
    scrollbar-color: var(--rv-border-strong) transparent;
  }
  .nacara-code pre:hover, .nacara-content table:hover, .rv-demo__code pre:hover {
    scrollbar-color: color-mix(in oklab, var(--rv-contour) 55%, var(--rv-border)) transparent;
  }
}
:is(.nacara-code pre, .nacara-content table, .rv-demo__code pre)::-webkit-scrollbar { height: 8px; width: 8px; }
:is(.nacara-code pre, .nacara-content table, .rv-demo__code pre)::-webkit-scrollbar-track { background: transparent; }
:is(.nacara-code pre, .nacara-content table, .rv-demo__code pre)::-webkit-scrollbar-thumb {
  background: var(--rv-border-strong);
  border-radius: 999px;
  border: 2px solid transparent;
  background-clip: content-box;
}
:is(.nacara-code pre, .nacara-content table, .rv-demo__code pre)::-webkit-scrollbar-thumb:hover {
  background-color: color-mix(in oklab, var(--rv-contour) 55%, var(--rv-border));
}
:is(.nacara-code pre, .nacara-content table, .rv-demo__code pre)::-webkit-scrollbar-corner { background: transparent; }
:is(.nacara-code pre, .nacara-content table, .rv-demo__code pre)::-webkit-scrollbar-button { display: none; }
"""

let mapsCss =
    """.rv-map-card {
  margin: 1.75rem 0; display: grid; grid-template-columns: minmax(0, 1fr);
  border: 1px solid var(--nacara-border); border-radius: var(--nacara-radius);
  background: var(--rv-code-bg); box-shadow: var(--nacara-shadow-floating); overflow: hidden; }
.rv-map-card > .nacara-code,
.rv-map-card > nacara-tabs { margin: 0; border: 0; border-radius: 0; box-shadow: none; }
.rv-map-card > .rv-map-host { border-top: 1px solid var(--nacara-border); }
.rv-map-card > .rv-map-code {
  margin: 0; padding: 0; border: 0; border-radius: 0; background: transparent; }
.rv-map-code > summary {
  padding: .625rem 1rem; color: var(--rv-text); font-weight: 500; cursor: pointer; }
.rv-map-code > summary:focus-visible { outline: 2px solid var(--rv-contour); outline-offset: -3px; }
.rv-map-code[open] > summary { border-bottom: 1px solid var(--nacara-border); }
.rv-map-code__body { overflow-y: auto; min-width: 0; }
.rv-map-code__body:focus-visible { outline: 2px solid var(--rv-contour); outline-offset: -3px; }
.rv-map-code__body > .nacara-code,
.rv-map-code__body > nacara-tabs { margin: 0; border: 0; border-radius: 0; box-shadow: none; }
.nacara-code__line.rv-map-line--run {
  background: color-mix(in oklab, var(--rv-contour) 14%, transparent);
  box-shadow: inset 2px 0 var(--rv-contour);
  transition: background .18s ease-out; }
.rv-map { display: grid; grid-template-columns: minmax(0, 1fr); font-size: .8125rem; }
.rv-map__stage {
  position: relative;
  padding: .75rem 1rem 2.25rem;
  isolation: isolate;
  background:
    radial-gradient(60% 80% at 20% 30%, color-mix(in oklab, var(--rv-contour) 10%, transparent), transparent 70%),
    radial-gradient(50% 70% at 85% 70%, color-mix(in oklab, var(--rv-violet) 9%, transparent), transparent 70%),
    var(--rv-canvas); }
.rv-map__svg { display: block; width: 100%; height: auto; max-height: 22rem; overflow: visible; }
.rv-map__inspect {
  position: absolute; left: 1rem; right: 1rem; bottom: .625rem;
  color: var(--nacara-text-muted); font-family: var(--nacara-font-mono, monospace); font-size: .75rem;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.rv-map-edge { fill: none; stroke: var(--rv-border-strong); stroke-width: 1.5; }
.rv-map-dot { fill: var(--rv-contour); }
.rv-map-dot--bright { fill: var(--rv-violet); filter: drop-shadow(0 0 4px var(--rv-violet)); }
.rv-map-node { cursor: pointer; outline: none; }
.rv-map-node__shape {
  fill: var(--rv-raised); stroke: var(--rv-contour); stroke-width: 1.75;
  transform-box: fill-box; transform-origin: center;
  transition: fill .2s ease-out, stroke .2s ease-out, opacity .2s ease-out; }
.rv-map-node--memo .rv-map-node__shape { stroke: var(--rv-violet); }
.rv-map-node--timed .rv-map-node__shape { stroke: var(--rv-violet); stroke-width: 2.5; }
.rv-map-node__timing { fill: var(--nacara-text-muted); font-size: 8px; }
.rv-map-node.is-timing .rv-map-node__shape { stroke-dasharray: 6 2; }
.rv-map-node.is-timing .rv-map-node__timing { fill: var(--rv-violet); }
.rv-map-node--async .rv-map-node__shape { stroke: var(--rv-pending); stroke-dasharray: 4 3; }
.rv-map-node--stream .rv-map-node__shape { stroke: var(--rv-pending); stroke-dasharray: 4 3; }
.rv-map-node__open { fill: none; stroke: var(--rv-pending); stroke-width: 1.75; opacity: 0; transition: opacity .2s ease-out; }
.rv-map-node.is-open .rv-map-node__open { opacity: 1; }
.rv-map-node--effect .rv-map-node__shape { stroke: var(--rv-accent-text); }
.rv-map-node__flight {
  fill: none; stroke: var(--rv-pending); stroke-width: 1.5; stroke-dasharray: 3 5; opacity: 0;
  transform-box: fill-box; transform-origin: center; }
.rv-map-node__name { fill: var(--rv-text); font-size: 11px; font-weight: 500; }
.rv-map-node__value {
  fill: var(--rv-value); font-family: var(--nacara-font-mono, monospace); font-size: 10.5px;
  transition: opacity .2s ease-out; }
.rv-map-node:hover .rv-map-node__shape,
.rv-map-node:focus-visible .rv-map-node__shape { stroke-width: 2.5; }
.rv-map-node:focus-visible .rv-map-node__name { text-decoration: underline; }
.rv-map-node.is-fresh .rv-map-node__shape { opacity: .55; }
.rv-map-node.is-running .rv-map-node__shape {
  fill: color-mix(in oklab, var(--rv-contour) 30%, var(--rv-raised));
  filter: drop-shadow(0 0 6px color-mix(in oklab, var(--rv-contour) 70%, transparent)); }
.rv-map-node.is-pending .rv-map-node__value,
.rv-map-node.is-waiting .rv-map-node__value { opacity: .45; }
.rv-map-node.is-waiting .rv-map-node__shape { stroke-dasharray: 2 3; }
.rv-map-node.is-flight .rv-map-node__flight { opacity: 1; animation: rv-map-spin 2.4s linear infinite; }
.rv-map-node.is-failed .rv-map-node__shape { stroke: var(--rv-error); fill: color-mix(in oklab, var(--rv-error) 14%, var(--rv-raised)); }
.rv-map-node.is-failed .rv-map-node__value { fill: var(--rv-error); }
.rv-map-node__cue.is-flash .rv-map-node__shape { fill: color-mix(in oklab, var(--rv-contour) 45%, var(--rv-raised)); }
.rv-map-node__cue.is-dropped .rv-map-node__flight { animation: rv-map-drop .7s ease-out; }
.rv-map-node__cue.is-settled .rv-map-node__shape { animation: rv-map-glow .8s ease-out; }
.rv-map-node__cue.is-failing .rv-map-node__shape { animation: rv-map-shake .4s ease-in-out 2; }
.rv-map-node__value.is-swap { animation: rv-map-swap .6s ease-out; }
.rv-map-node.is-flight .rv-map-node__flight { animation-duration: calc(2.4s / var(--rv-map-speed, 1)); }
.rv-map-node__cue.is-dropped .rv-map-node__flight { animation-duration: calc(.7s / var(--rv-map-speed, 1)); }
.rv-map-node__cue.is-settled .rv-map-node__shape { animation-duration: calc(.8s / var(--rv-map-speed, 1)); }
.rv-map-node__cue.is-failing .rv-map-node__shape { animation-duration: calc(.4s / var(--rv-map-speed, 1)); }
.rv-map-node__value.is-swap { animation-duration: calc(.6s / var(--rv-map-speed, 1)); }
@keyframes rv-map-spin { to { transform: rotate(360deg); } }
@keyframes rv-map-drop { from { opacity: 1; stroke: var(--rv-error); } to { opacity: 0; transform: scale(1.5); } }
@keyframes rv-map-glow { 40% { filter: drop-shadow(0 0 8px var(--rv-contour)); } }
@keyframes rv-map-shake { 25% { transform: translateX(-2px); } 75% { transform: translateX(2px); } }
@keyframes rv-map-swap { from { fill: var(--rv-violet); } }
.rv-map__bar {
  display: flex; flex-wrap: wrap; align-items: center; gap: .5rem 1rem; padding: .625rem 1rem;
  border-top: 1px solid var(--nacara-border); background: var(--rv-surface); }
.rv-map__controls { display: flex; flex-wrap: wrap; gap: .5rem; }
.rv-map__controls:empty { display: none; }
.rv-map__button {
  padding: .3rem .75rem; border: 1px solid var(--rv-border); border-radius: 999px;
  background: var(--rv-raised); color: var(--rv-text); font: inherit; font-weight: 500; cursor: pointer;
  transition: border-color .15s ease-out, background .15s ease-out; }
.rv-map__button:hover { border-color: var(--rv-contour); }
.rv-map__button:focus-visible { outline: 2px solid var(--rv-contour); outline-offset: 2px; }
.rv-map__button:active { background: color-mix(in oklab, var(--rv-contour) 16%, var(--rv-raised)); }
.rv-map__button--reset { color: var(--nacara-text-muted); }
.rv-map__input {
  display: inline-flex; align-items: center; gap: .4rem; padding: .2rem .6rem;
  border: 1px solid var(--rv-border); border-radius: 999px; background: var(--rv-raised); color: var(--rv-text);
  font-weight: 500; }
.rv-map__input:focus-within { border-color: var(--rv-contour); }
.rv-map__input-field { font: inherit; color: inherit; accent-color: var(--rv-contour); }
.rv-map__input-field:focus-visible { outline: 2px solid var(--rv-contour); outline-offset: 2px; }
.rv-map__input--number .rv-map__input-field,
.rv-map__input--text .rv-map__input-field {
  width: 6rem; padding: .1rem .35rem; border: 1px solid var(--rv-border); border-radius: .35rem;
  background: var(--rv-surface); }
.rv-map__input--slider .rv-map__input-field { width: 7rem; }
.rv-map__input-value { min-width: 1.5rem; font-family: var(--nacara-font-mono, monospace); color: var(--rv-muted); }
.rv-map__timeline { display: flex; flex-wrap: wrap; align-items: center; gap: .5rem; flex: 1 1 16rem; }
.rv-map__speed { display: inline-flex; align-items: center; gap: .35rem; white-space: nowrap; }
.rv-map__speed-select {
  padding: .25rem .4rem; border: 1px solid var(--rv-border); border-radius: .4rem;
  background: var(--rv-raised); color: var(--rv-text); font: inherit; cursor: pointer; }
.rv-map__speed-select:focus-visible { outline: 2px solid var(--rv-contour); outline-offset: 2px; }
.rv-map__track { position: relative; flex: 1 1 8rem; min-width: 6rem; display: grid; align-items: center; }
.rv-map__ticks { position: absolute; inset: 0; pointer-events: none; }
.rv-map__tick {
  position: absolute; top: 50%; width: 2px; height: .625rem; margin: -.3125rem 0 0 -1px;
  border-radius: 1px; background: color-mix(in oklab, var(--rv-contour) 55%, transparent); }
.rv-map__scrub { width: 100%; margin: 0; accent-color: var(--rv-contour); background: transparent; }
.rv-map__log {
  margin: 0; padding: .5rem 1rem; list-style: none; max-height: 9.5rem; overflow-y: auto;
  border-top: 1px solid var(--nacara-border);
  font-family: var(--nacara-font-mono, monospace); font-size: .75rem; line-height: 1.6; color: var(--nacara-text-muted); }
.rv-map__caption {
  margin: 0; padding: .625rem 1rem; border-top: 1px solid var(--nacara-border);
  background: var(--rv-surface); color: var(--rv-text); line-height: 1.6; }
.rv-map__caption:empty { display: none; }
.rv-map__log:empty { display: none; }
.rv-map__log li:last-child { color: var(--rv-text); }
.rv-map__log li.is-error { color: var(--rv-error); }
.rv-map__why {
  margin: 0; padding: .625rem 1rem; border-top: 1px solid var(--nacara-border);
  background: var(--rv-surface); font-size: .75rem; white-space: pre-wrap; }
.rv-map__why:empty { display: none; }
.rv-map__error {
  padding: .5rem 1rem; border-top: 1px solid var(--rv-error);
  background: color-mix(in oklab, var(--rv-error) 12%, transparent); color: var(--rv-error); font-weight: 500; }
.rv-map__error:empty { display: none; }
@media (forced-colors: active) {
  .rv-map-node__shape, .rv-map-edge { stroke: CanvasText; }
  .rv-map-dot { fill: Highlight; }
  .nacara-code__line.rv-map-line--run { outline: 1px solid Highlight; }
}
@media (prefers-reduced-motion: reduce) {
  .rv-map-node.is-flight .rv-map-node__flight,
  .rv-map-node__cue.is-dropped .rv-map-node__flight,
  .rv-map-node__cue.is-settled .rv-map-node__shape,
  .rv-map-node__cue.is-failing .rv-map-node__shape,
  .rv-map-node__value.is-swap { animation: none; }
  .rv-map-node__shape, .nacara-code__line.rv-map-line--run { transition: none; }
}
.rv-map-group {
  fill: color-mix(in oklab, var(--rv-contour) 6%, transparent);
  stroke: var(--rv-border-strong); stroke-width: 1; stroke-dasharray: 2 4; }
.rv-map-group__column { fill: var(--rv-muted); font-size: 10px; }
.rv-map-node--row .rv-map-node__name { font-size: 10.5px; }
.rv-map-node--member .rv-map-node__name,
.rv-map-node--member .rv-map-node__value { display: none; }
.rv-map-failed { padding: 1rem 1.25rem; border-top: 1px solid var(--nacara-border); color: var(--rv-error); }
.rv-map-failed__title { margin: 0 0 .5rem; font-weight: 600; }
.rv-map-failed__text { margin: 0; white-space: pre-wrap; font-size: .8125rem; color: var(--nacara-text); }
.nacara-content .rv-demo__frames { margin: 0; padding: 0; }
.nacara-content .rv-demo__frames > li { margin: 0; }
.nacara-content .rv-demo__frames code { background: none; border: 0; padding: 0; }
"""

/// <summary>Splash hero & showcase styles applied only to the root landing collection or repositories declaring landing.css.</summary>
let splashLandingCss =
    """body:has(.nacara-layout--splash) { overflow-x: clip; }
.nacara-layout--splash { --nacara-splash-width: 72rem; }
.nacara-layout--splash .nacara-content h2 {
  margin: 4.5rem 0 .5rem; padding-top: 1.25rem; border-top: 0;
  font-size: clamp(1.625rem, 1.35rem + 1vw, 2.125rem); font-weight: 600; letter-spacing: -.03em;
  background: linear-gradient(var(--rv-contour), var(--rv-contour)) left top / 2.5rem 2px no-repeat; }
.nacara-layout--splash .nacara-content h2 .nacara-heading-anchor { display: none; }
.nacara-layout--splash .nacara-content h2 + p {
  color: var(--nacara-text-muted); font-size: 1.0625rem; max-width: 42rem; margin: 0 0 2rem; }
.rv-hero {
  display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1.15fr); gap: 3.5rem; align-items: center;
  padding: 4.5rem 0 1rem; position: relative; isolation: isolate; }
.rv-hero::before {
  content: ""; position: absolute; z-index: -1; pointer-events: none;
  inset: -6rem -12rem -4rem;
  background:
    radial-gradient(38% 55% at 22% 45%, color-mix(in oklab, var(--rv-contour) 24%, transparent), transparent 70%),
    radial-gradient(34% 50% at 78% 40%, color-mix(in oklab, var(--rv-violet) 20%, transparent), transparent 70%);
  filter: blur(24px); }
:root[data-theme="light"] .rv-hero::before { opacity: .7; }
.rv-hero__status {
  display: inline-flex; align-items: center; gap: .625rem; margin-bottom: 1.5rem;
  font-size: .8125rem; font-weight: 500; color: var(--nacara-text-muted); }
.nacara-content h1.rv-hero__title {
  margin: 0 0 1rem; padding: 0; border: 0;
  font-size: clamp(3.25rem, 2.2rem + 4vw, 5rem); font-weight: 600; line-height: .95; letter-spacing: -.055em;
  color: var(--nacara-heading); }
.nacara-content h1.rv-hero__title::after {
  content: ""; display: inline-block; width: .16em; height: .16em; margin-left: .06em;
  border-radius: 50%; background: var(--rv-contour); }
.nacara-content .rv-hero p.rv-hero__line {
  margin: 0 0 1rem; font-family: var(--rv-font-display);
  font-size: clamp(1.1875rem, 1.05rem + .5vw, 1.375rem); font-weight: 500; line-height: 1.4; letter-spacing: -.015em;
  color: var(--nacara-heading); }
.nacara-content .rv-hero p.rv-hero__sub {
  margin: 0 0 2rem; font-size: 1rem; line-height: 1.65; color: var(--nacara-text-muted); max-width: 34rem; }
.rv-hero__actions { display: flex; gap: .75rem; flex-wrap: wrap; }
.nacara-content .rv-hero__actions a.rv-btn { text-decoration: none; }
.nacara-content .rv-hero a.rv-btn--primary { background: var(--rv-gradient-accent); }
.rv-demo {
  margin: 0; display: grid; grid-template-columns: minmax(0, 1fr);
  border: 1px solid var(--nacara-border); border-radius: var(--nacara-radius);
  background: var(--rv-code-bg); box-shadow: var(--nacara-shadow-floating); overflow: hidden; }
.rv-demo__code pre {
  margin: 0; padding: 1.25rem 1.25rem 1rem; overflow-x: auto; background: none; border: 0;
  font-family: var(--nacara-font-mono); font-size: .8125rem; line-height: 1.7; color: var(--nacara-text); }
.rv-demo__code code { font: inherit; background: none; border: 0; padding: 0; }
.rv-demo .k { color: var(--rv-violet); }
.rv-demo .t { color: var(--rv-accent-text); }
.rv-demo .s { color: var(--rv-value); }
.rv-demo .n { color: var(--rv-violet); }
.rv-demo .c { color: var(--nacara-text-muted); }
.rv-demo__out { border-top: 1px solid var(--nacara-border); background: var(--nacara-bg-subtle); padding: .875rem 1.25rem 1rem; }
.rv-demo__label { font-family: var(--nacara-font-mono); font-size: .75rem; color: var(--nacara-text-muted); margin-bottom: .5rem; }
@media (max-width: 60rem) {
  .rv-hero { grid-template-columns: 1fr; gap: 2.5rem; padding-top: 2.5rem; }
  .rv-hero::before { inset: -4rem 0 -2rem; }
}
"""

let private fontLinks : StaticView list =
    [ Markup.view "link" [ "rel", "preconnect"; "href", "https://fonts.googleapis.com" ] []
      Markup.view "link" [ "rel", "preconnect"; "href", "https://fonts.gstatic.com"; "crossorigin", "" ] []
      Markup.view "link" [ "rel", "stylesheet"; "href", "https://fonts.googleapis.com/css2?family=Geist:wght@400..700&family=Geist+Mono:wght@400..600&display=swap" ] [] ]

let private sharedFooter : StaticView =
    Markup.view
        "p"
        []
        [ Markup.text "Ranvier · Pre-1.0 · "
          Markup.view "a" [ "href", "https://github.com/ranvier-org" ] [ Markup.text "GitHub" ]
          Markup.text " · Built with Nacara & Ranvier.View" ]

let private normalizeSlash (url: string) =
    if url.EndsWith("/") then url else url + "/"

/// <summary>CSS snippet for the LiveExample browser iframe so live previews match the Ranvier brand.</summary>
let livePreviewCss =
    brandTokensAndPalette + "\n" + mapsCss

/// <summary>Layers Ranvier brand tokens, Geist typography, components, state glyphs, and map styles onto ThemeOptions.</summary>
let apply (baseUrl: string) (sourceAssetBase: string option) (options: ThemeOptions) : ThemeOptions =
    let assetBase = sourceAssetBase |> Option.defaultValue baseUrl |> normalizeSlash
    let combinedCss =
        String.concat
            "\n"
            [ brandTokensAndPalette
              brandChromeAndComponents.Replace("__BASE__", assetBase)
              mapsCss ]

    { options with
        FavIcon = options.FavIcon |> Option.orElse (Some "favicon.svg")
        Footer = options.Footer |> Option.orElse (Some sharedFooter)
        HeadExtra = options.HeadExtra @ fontLinks }
    |> Theme.css combinedCss
