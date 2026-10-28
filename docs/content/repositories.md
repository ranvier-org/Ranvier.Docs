---
title: Connect a repository
---

# Connect a repository

## The minimal integration

A repository containing `docs/content/index.md` can be imported with a single host registry entry. A per-repository manifest is optional. The shared infrastructure already provides core Ranvier, View, motion and CSS examples, common styling and publication.

Add this object to the `sources` array in the host's `sources.json`:

```json
{
  "id": "forms",
  "repository": "ranvier-org/Ranvier.Forms",
  "ref": "main",
  "local": "../Ranvier.Forms"
}
```

Choose an id that remains meaningful if the repository's display name changes. Ids use a lowercase letter followed by lowercase letters, digits or hyphens. A route defaults to that id and must be unique. Registry order is the public repository-navigation order.

Fetch and build the new import:

```sh
dotnet fsi build.fsx -- docs
```

Review the registry and lock changes together. Adding a new entry acquires that repository while retaining the accepted revisions of existing entries. Adding a repository is an explicit opt-in; the host does not execute arbitrary newly discovered organisation repositories automatically. Once registered, newly added pages are discovered automatically.

## Repository layout

The default contract is:

```text
docs/
  ranvier-docs.json       optional configuration
  content/
    index.md             normal landing page
    guide/
      introduction.md
      concepts.md
    images/
      graph.svg
  static/
    downloads/
      sample.json
```

Markdown and `.fsx` pages inside the content directory enter the site's collection. Other content files are copied so relative image/download links can retain their paths. `docs/static/` is optional and copied under the repository's route prefix.

Plain Markdown without YAML front matter is supported. The importer adds a derived title to its staged copy while preserving the source file and document body. Existing front matter remains authoritative. This lets a repository with ordinary Markdown guides participate without first converting every source page to a site-specific format.

The importer ignores `.git`, `bin`, `obj`, `node_modules`, `output` and `.SageFs` directories. Imported content must not contain symlinks. This prevents a documentation tree from unexpectedly reading or copying files outside its checkout.

## Add a repository manifest

Use `docs/ranvier-docs.json` for customisation owned by that library:

```json
{
  "version": 1,
  "title": "Forms",
  "description": "Reactive form state and validation",
  "index": "index.md",
  "sections": [
    { "title": "Start", "pages": ["index.md", "guide/introduction.md"] },
    { "title": "Concepts", "pages": ["guide/concepts.md"] }
  ]
}
```

All paths in that manifest are repository-relative unless an option explicitly names a page. Page paths, including `index`, section `pages` and `unlisted`, are relative to the configured content directory. These two path bases are deliberate: projects and assets belong to the repository; navigation belongs to its documentation collection.

## Existing nonstandard layouts

Use `content` to select a different folder:

```json
{ "version": 1, "content": "manual", "index": "README.md" }
```

For a repository whose `docs/` contains plain Markdown rather than `docs/content/`, the host registry can set `"content": "docs"` before the library has a manifest. WebCore uses that initial integration. Once a library owns its manifest, its `content` option takes precedence over the registration fallback.

Use `manifest` in the registry if the configuration itself lives elsewhere, for example `"manifest": "documentation/site.json"`. It must remain inside the repository.

## Local overrides

The default build reads acquired `.sources/<id>` checkouts. `local` is a convenience location, not an unconditional preference. `--local` selects configured local paths. `--source id=path` overrides one checkout explicitly and takes precedence over both.

```sh
dotnet fsi build.fsx -- prepare --source forms=../Ranvier.Forms
dotnet fsi build.fsx -- docs --watch --source forms=../Ranvier.Forms
```

An author can test a repository that has not yet been published remotely by registering it and overriding its source root. Real local repositories record HEAD and whether the checkout is dirty. The public authoring commands use real checkouts so previews retain meaningful provenance.

## Removing or renaming an import

Before removing a source, remove or replace incoming `repo:<id>/...` links. Remove its registry entry, deliberately refresh the lock, then rebuild and check links. Old generated pages disappear on the next successful import. The acquisition command does not delete unused checkout directories; archive them manually only after preserving local work.

Changing `title` is a presentation change. Changing `id` breaks explicit incoming repository links. Changing `route` changes public URLs even if source links still resolve. Treat id and route changes as a migration with redirects, as described in [Migration](migration.md).
