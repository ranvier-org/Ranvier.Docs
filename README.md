# Ranvier documentation

A shared documentation host for the Ranvier organisation. Libraries keep their documentation beside their source; this repository imports it, validates navigation and links, and publishes it with a common Ranvier.View/Ranvier.Css theme and runnable example support.

Existing repository sites remain in place until a separate migration. Building this repository does not deploy or alter them.

## Start

Use Node 26 or newer and the .NET SDK selected by `global.json`.

```sh
npm install
npm run sources:fetch
npm run build
npm run check:links
```

For the full Node/F#/theme/metadata/link/browser acceptance gate, install Chromium with `npx playwright install chromium`, then run `npm run verify`.

`sources:fetch` acquires the registered public repositories into `.sources/` and records exact revisions in `sources.lock.json`. Repeated runs reuse that lock. Run `npm run sources:fetch -- --refresh` to deliberately update the coordinated source revisions.

For a local authoring checkout:

```sh
npm run preview -- --source view=../Ranvier.View
```

An ordinary new repository needs one entry in [sources.json](sources.json). Its `docs/content/` and optional `docs/static/` are picked up automatically. An optional `docs/ranvier-docs.json` controls sections, titles, landing pages, additional examples and assets.

## Complete guide

- [Quick start and prerequisites](docs/content/quick-start.md)
- [Architecture and ownership](docs/content/architecture.md)
- [Adding repositories and source acquisition](docs/content/repositories.md)
- [Every configuration option](docs/content/configuration.md)
- [Sections, navigation, landing pages and source links](docs/content/navigation.md)
- [Links across repositories, anchors and assets](docs/content/links-assets.md)
- [Core, views, motion, CSS and editable examples](docs/content/examples.md)
- [Shared runnable showcase](docs/content/runnable-examples.md)
- [Theme and section-specific styling](docs/content/theme.md)
- [Local preview and verification](docs/content/preview.md)
- [CI, publication and version coordination](docs/content/ci-publishing.md)
- [Migration, redirects and rollback](docs/content/migration.md)
- [Diagnostics and troubleshooting](docs/content/troubleshooting.md)

Run `npm test` for the complete Node test suite. The build, example and theme verification commands are documented in the guide and CI.
