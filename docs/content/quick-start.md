---
title: Quick start
---

# Quick start

## Prerequisites

Install Git, Node 26 or newer and the .NET SDK selected by this repository's `global.json`. Node is an internal compiler and bundler prerequisite, including for imported standalone View examples. Partas.Build automatically restores cached npm dependencies, NuGet packages and pinned .NET tools. Authors run the .NET commands below; no separate package installation is required. The build uses the public repositories listed in `sources.json`; reading public sources requires no personal token.

Clone this host, then build:

```sh
dotnet fsi build.fsx -- docs
```

The first fetch resolves each configured branch and records its exact commit in `sources.lock.json`. Future fetches use those commits. Review and commit the lock when accepting an update. Source checkouts live in `.sources/<id>` and are disposable build inputs, but the acquisition command refuses to overwrite local changes in them.

## Build the complete site

```sh
dotnet fsi build.fsx -- docs
```

The build acquires locked sources, prepares the generated content tree, resolves library roots, compiles the shared example project, bundles assets, builds the F# host, runs Nacara and checks links. Published files are written to `output/`. The link check inspects those files, including anchors and local referenced assets, under the configured deployment base. An ordinary build does not run the full test suite.

The generated folders are implementation outputs. Edit source documentation, manifests and shared implementation files instead of `.generated/` or `output/`.

## Preview one local library with the coordinated dependencies

```sh
dotnet fsi build.fsx -- docs --watch --source view=../Ranvier.View
```

This selects a local View checkout and keeps other libraries at their acquired revisions. Use the same flag with `docs` or `prepare`. Repeat it for additional overrides:

```sh
dotnet fsi build.fsx -- docs --source view=../Ranvier.View --source css=../Ranvier.Css
```

Use `--local` when all optional `local` locations in the registry exist and are the intended inputs. A missing local checkout is an error; the importer does not silently substitute a different revision.

## Add a library using defaults

Put Markdown in that library's `docs/content/`, with `index.md` as its landing page. Add a source entry in the host:

```json
{
  "id": "query",
  "repository": "ranvier-org/Ranvier.Query",
  "ref": "main",
  "local": "../Ranvier.Query"
}
```

Then fetch sources and build. The default route is `/query/` beneath the site's deployment base. The default menu lists discovered Markdown and literate F# pages. For deliberate grouping and ordering, add `docs/ranvier-docs.json` as described in [Connect a repository](repositories.md).

## First verification

Check the rendered landing page, a nested page, navigation, an internal link, an image and a working component example. For focused development, select a suite with `dotnet fsi build.fsx -- test --suite node` (or `examples`, `theme`, `metadata`, `browser`). Run `dotnet fsi build.fsx -- verify` explicitly for full acceptance; it restores browser tooling and runs all stages once. See [Preview and verification](preview.md).
