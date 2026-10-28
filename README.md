# Ranvier documentation

A shared documentation host for the Ranvier organisation. Libraries keep their documentation beside their source; this repository imports it, validates navigation and links, and publishes it with a common Ranvier.View/Ranvier.Css theme and runnable example support.

Existing repository sites remain in place until a separate migration. Building this repository does not deploy or alter them.

## Start

Install Git, Node 26 or newer and the .NET SDK selected by `global.json`. Partas.Build drives the authoring workflow and automatically restores internal npm dependencies and pinned .NET tools.

```sh
dotnet fsi build.fsx -- docs
```

This acquires locked sources, prepares imports, compiles examples, bundles assets, builds the site and checks links. Preview and ordinary builds do not run the full test suite. For the full Node/F#/theme/metadata/link/browser acceptance gate, run `dotnet fsi build.fsx -- verify`; it installs Chromium automatically.

`dotnet fsi build.fsx -- sources` acquires registered public repositories into `.sources/` and records exact revisions in `sources.lock.json`. Repeated runs reuse that lock. Use `--refresh core,view,css` to update selected sources, or `--refresh all` to deliberately update every source.

For a local authoring checkout:

```sh
dotnet fsi build.fsx -- docs --watch --source view=../Ranvier.View
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

Run `dotnet fsi build.fsx -- test` for the complete Node test suite. Focused suites and the full acceptance gate are documented in the guide.
