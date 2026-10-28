---
title: Quick start
---

# Quick start

## Prerequisites

Install Git, Node 26 or newer, npm and the .NET SDK selected by this repository's `global.json`. Node 26 is required by the imported standalone View examples. Allow the build to restore its NuGet packages and tools. Browser examples also need the npm dependencies declared by their presets. The build uses the public repositories listed in `sources.json`; reading public sources requires no personal token.

Clone this host, then install and acquire sources:

```sh
npm install
npm run sources:fetch
```

The first fetch resolves each configured branch and records its exact commit in `sources.lock.json`. Future fetches use those commits. Review and commit the lock when accepting an update. Source checkouts live in `.sources/<id>` and are disposable build inputs, but the acquisition command refuses to overwrite local changes in them.

## Build the complete site

```sh
npm run build
npm run check:links
```

The build prepares the generated content tree, resolves library roots, compiles the shared example project, restores/builds the F# host, and runs Nacara. Published files are written to `output/`. The link check inspects those files, including anchors and local referenced assets, under the configured deployment base.

The generated folders are implementation outputs. Edit source documentation, manifests and shared implementation files instead of `.generated/` or `output/`.

## Preview one local library with the coordinated dependencies

```sh
npm run preview -- --source view=../Ranvier.View
```

This selects a local View checkout and keeps other libraries at their acquired revisions. Use the same flag with `build` or `prepare:docs`. Repeat it for additional overrides:

```sh
npm run build -- --source view=../Ranvier.View --source css=../Ranvier.Css
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

Check the rendered landing page, a nested page, navigation, an internal link, an image and a working component example. Run `npm test`, the full site build and `npm run check:links`. If the theme, importer or examples infrastructure changed, also run the component's complete test command described in [Preview and verification](preview.md).
