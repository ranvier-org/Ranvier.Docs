---
title: Configuration reference
---

# Configuration reference

Configuration files use JSON, version `1`. The schemas in `schemas/` provide editor completion. The importer performs runtime validation of identities, paths, required pages, menu coverage, npm dependency pairs and link targets. Unknown repository-manifest options are errors so misspelled options do not silently change the published result.

## Host registry: sources.json

- **`version`**: required, `1`.
- **`title`**: shared site title; defaults to `Ranvier`.
- **`baseUrl`**: public deployment prefix; defaults to `/`. Use `/Ranvier.Docs/` for a GitHub project site. Include both leading and trailing slashes. This is a path, not a full URL.
- **`origin`**: public scheme and hostname, default `https://ranvier-org.github.io`. Nacara uses it for absolute site metadata.
- **`sources`**: required nonempty array of registrations, in navigation order.
- **`$schema`**: optional editor schema reference.

Each registration supports:

- **`id`**: required stable lowercase source id. Used by `repo:` links, local overrides, generated metadata and collection identity.
- **`kind`**: `docs` by default. A `dataset` input is acquired and revision-locked like a repository but does not create pages or navigation. Maintained build adapters can consume it as data; it appears under generated `datasets` provenance.
- **`repository`**: required GitHub `owner/name`, without a URL prefix or `.git` suffix. Used for acquisition, provenance and edit links.
- **`ref`**: Git ref to resolve on an intentional lock refresh; defaults to `HEAD`. Prefer an explicit branch, tag or reviewed commit.
- **`route`**: unique first public URL segment, defaulting to `id`. Routes are lowercase letters, digits and hyphens, beginning with a letter. Nested or empty route prefixes are not supported by this registry contract.
- **`title`**: fallback display title. A repository manifest's title takes precedence.
- **`description`**: fallback description used by the central source metadata.
- **`local`**: optional checkout path, resolved relative to the central host. Used by `--local`; does not affect normal acquired builds.
- **`manifest`**: optional repository-relative path to its manifest; defaults to `docs/ranvier-docs.json`. A missing optional manifest activates defaults.
- **`content`**: fallback content root for importing a library before it owns a manifest; defaults to `docs/content`.
- **`defaults`**: optional repository-manifest object owned by the central host. Use it to bootstrap an existing repository before migrating its configuration, for example prebuilt browser-bundle routes. Its fields are applied before the source's own manifest; source fields override matching defaults. Array-valued options are replaced as a whole, not concatenated.
- **`self`**: when true, read this host's own checkout rather than acquire it. Used for the authoring guide. Do not use it for external library imports.

## Repository manifest: docs/ranvier-docs.json

All options are optional. An absent file is equivalent to using defaults.

- **`version`**: when supplied, must be `1`.
- **`title`**: reader-facing repository title.
- **`description`**: short repository description.
- **`content`**: repository-relative content directory, default `docs/content` or the registration's fallback.
- **`static`**: repository-relative static asset directory, default `docs/static`. Use `false` to disable it. A missing default directory is allowed.
- **`index`**: landing page relative to `content`. The default is `index.md`, then `README.md`, then the first discovered page. Explicitly choose it for stable navigation.
- **`sections`**: ordered array of navigation groups; each has `title` and `pages`. Without it, one section lists all discovered pages in deterministic path order.
- **`unlisted`**: array of existing page paths intentionally omitted from the menu. Pages remain rendered, linkable and searchable. Defaults to empty.
- **`exclude`**: array of patterns matching content-relative file paths. `*` matches characters within a path segment; `**` matches across segments. Matching files are not staged or rendered. Defaults to empty. For example, `research/**` excludes that subtree.
- **`examplesProject`**: repository-relative `.fsproj` containing additional sample modules. The generated shared example project references it alongside the baseline preset. See [Runnable examples](examples.md).
- **`packages`**: runtime npm dependencies as `[name, version]` pairs, for example `[["animejs", "4.5.0"]]`. These are browser bundling dependencies, not NuGet references. Different versions of the same package across sources are a build error.
- **`browserBundles`**: built example output to copy. Each object has a repository-relative `path` and a content-route-relative destination `route`, for example `{ "path": "examples/demo/dist", "route": "examples/demo" }`. Build that output before importing; missing paths fail rather than producing broken demo links.
- **`$schema`**: optional editor schema reference.

Each section page is either a relative path string or an object with `path` and optional `title`:

```json
{
  "title": "Getting started",
  "pages": [
    "index.md",
    { "path": "guide/installation.md", "title": "Install" }
  ]
}
```

An explicit menu must cover every discovered page unless it appears in `unlisted`. Referencing a nonexistent page, listing a page twice, or forgetting a page is an error. A custom menu label does not rewrite the page's own title.

## Full example

```json
{
  "version": 1,
  "title": "Forms",
  "description": "Reactive forms and validation",
  "content": "docs/content",
  "static": "docs/static",
  "index": "index.md",
  "exclude": ["drafts/**"],
  "unlisted": ["reference/compatibility.md"],
  "sections": [
    { "title": "Start", "pages": ["index.md", "guide/install.md"] },
    { "title": "Recipes", "pages": [{ "path": "recipes/login.md", "title": "Login form" }] }
  ],
  "examplesProject": "docs/samples/Forms.Examples.fsproj",
  "packages": [["animejs", "4.5.0"]],
  "browserBundles": [
    { "path": "examples/login/dist", "route": "examples/login" }
  ]
}
```

## Path rules

Use forward slashes on every platform. Repository-relative paths must be nonempty, cannot be absolute, and cannot contain `.` or `..` segments. `local` and explicit `--source` checkout overrides are the intentional exception: they locate a checkout outside the central host. Referenced source directories are checked against their resolved repository root. Symlinks inside imported trees are rejected.

## Lock and generated metadata

`sources.lock.json` has `version` and a `sources` object keyed by id. Each entry records `repository`, configured `ref` and exact `revision`. Maintain it with `dotnet fsi build.fsx -- sources`; avoid hand-editing commit hashes. Use `--refresh core,view,css` for selected source ids or `--refresh all` for every source.

`.generated/site.json` is generated integration metadata, not source configuration. It contains `title`, `baseUrl`, `origin`, generation time and source entries with resolved root, revision, dirty status, staged content root, original content path, source edit URL, menu sections, landing page, example project and npm packages. It intentionally contains local absolute paths for the local build; publish the site output, not this metadata as executable configuration on another machine.

The generated `datasets` array records dataset ids, repositories, configured refs, actual revisions, roots and dirty status independently of documentation collections. Dataset registrations do not become `repo:` link targets.
