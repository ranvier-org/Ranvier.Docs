# Central Ranvier documentation

## Approved intent

Build the complete shared documentation implementation now. Migration of existing repository sites happens later. Documentation remains next to each library. Every imported repository can use core Ranvier, Ranvier.View, motion and Ranvier.Css in runnable documentation without duplicating infrastructure. Replace Partas.Nacara.Theme with a server-rendered Ranvier.View/Ranvier.Css theme.

## Implementation decisions

- A standalone `Ranvier.Docs` workspace owns the Nacara site, importer, shared example plugin, authoring preset, tests and publication workflow.
- `sources.json` registers repository ids, checkout locations, routes and optional per-repository manifest locations. Local checkouts are first-class; CI acquires sources at recorded revisions and records the exact SHA of every input.
- An optional `docs/ranvier-docs.json` in each source configures its title, introduction, navigation sections, additional example project, static assets and browser bundles. Defaults pick up `docs/content` and `docs/static`. A repository with plain Markdown under `docs` can explicitly select that folder.
- Source files are staged under a generated workspace, never rewritten in their repositories. Preserve relative Markdown links and assets. Explicit `repo:<id>/<page.md>#anchor` links target another repository and are validated before publication. No ad hoc rewriting of code samples.
- The site has one common navigation shell and a documentation section per source, source-specific edit links, generated section navigation where no explicit menu is supplied, global search and local preview.
- A shared examples project supplies core, DOM, animation and typed CSS dependencies. Additional source projects extend this preset. Preserve component, map and editable live fences and their existing diagnostics; compile examples and hydrate SSR HTML.
- Theme markup is rendered at build time with Ranvier.View; structured styles are authored with Ranvier.Css. Keep Nacara as the publishing engine and use its final HTML adapter where required. Do not require browser Ranvier to read ordinary docs.
- New repositories opt in with one registry entry, normally requiring no custom manifest. Custom sections, ordering, labels, landing pages, assets and example dependencies are explicit manifest options.
- Pin coordinated dependency revisions; no rewriting checked-in library source/docs, no migration/disablement of old sites, no production deployment during implementation. Normal ignored .NET/npm/demo/performance build products are allowed in selected checkouts.

## Acceptance evidence

1. Importer tests cover defaults, custom sections, duplicate routes, missing sources, invalid paths, cross-repository page and anchor links, assets, stale-output cleanup and local revision provenance.
2. One command prepares sources and builds the site. Core, View, CSS, Persistence and WebCore configuration examples are provided without modifying those repositories.
3. A representative site renders pages with the shared theme, functional internal links, source links and menus. Browser checks cover hydration, an interactive motion example and editable compilation where supported.
4. Release build, unfiltered tests, output link checks and package/theme checks pass; any externally unavailable dependency or remaining limitation is recorded honestly.
5. Full guides describe quick start, configuration reference, authoring, custom sections/navigation, cross-repository links/anchors/assets, runnable examples, local preview, CI/publishing, version coordination, adding/removing repositories, diagnostics and eventual migration.

## Execution boundaries

Theme implementation is in the existing `../Nacara.Plugins.Ranvier` workspace, continuing its interrupted theme plan and preserving existing changes. Central importer/tests/guides are owned by the primary agent. Shared example infrastructure and the F# Nacara host are separate implementation tasks with distinct files. No changes to existing repository docs deployments are part of this implementation.
