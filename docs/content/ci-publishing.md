---
title: CI and publishing
---

# CI and publishing

## Build contract

The central workflow checks out this host, installs the selected Node/.NET tooling and invokes the Partas.Build workflow. Partas.Build restores internal dependencies and pinned tools, acquires locked sources and runs the requested build or verification stages. It produces `output/` and provenance metadata as reviewable build artifacts. Authors use the same .NET entry point locally.

Library repositories are inputs. Their existing docs workflows remain untouched until migration. A central build does not change library packages or publish a new library version.

## Reproducible source selection

`sources.json` expresses intended refs. `sources.lock.json` expresses the exact accepted revisions. A normal `sources` command or `docs` build uses the lock. To update and validate coordinated inputs:

```sh
dotnet fsi build.fsx -- sources --refresh all
dotnet fsi build.fsx -- verify
```

Review and commit the lock changes with any needed documentation/build changes. If a registration's repository or ref changes while its lock entry remains unchanged, acquisition fails and tells you to refresh. The lock is not silently discarded.

To update only selected libraries and keep all other accepted revisions unchanged, name source ids after the refresh flag:

```sh
dotnet fsi build.fsx -- sources --refresh view
dotnet fsi build.fsx -- sources --refresh core,view,css
```

Use `--refresh all` when intentionally updating every acquired source. The flag requires a value. Unknown ids are errors. Adding a new registration needs a normal fetch; existing locked inputs are retained.

The `core-performance` dataset pins the core repository's `perf-data` branch. A maintained build adapter runs the core's own performance-index generator against that snapshot, producing the ignored `docs/static/perf-data/` files used by its charts. This uses real recorded measurements and records the dataset revision. Refresh just that dataset when accepting newer history without updating library code:

```sh
dotnet fsi build.fsx -- sources --refresh core-performance
```

Generated history is copied as static output by the ordinary importer. Do not edit the generated chart JSON as a data correction; correct the measurement source/generator or accept a different reviewed dataset snapshot.

An update to one library can require a matching View/CSS/core update. Accept the coordinated set only after compiling shared samples and generating the complete site. A passing empty preset project is insufficient when real documentation calls a newer API.

## Dependency versions

NuGet references and local source roots are separate from npm runtime packages. Nacara, Fable/checker metadata, Xantham bindings and Ranvier source revisions must agree sufficiently to compile and run the shared preset. Review package and lock changes together.

Keep exact versions for the central host's own packages and tools. Avoid a developer-only package feed as an undocumented prerequisite. Any locally built prerelease required for development must be replaced by an acquired source project or a published pinned package before claiming clean-machine CI readiness.

## Triggering rebuilds & downstream PR validation

Pushes and pull requests in the central host run its workflow. Merging to `main` automatically verifies and deploys the shared site to GitHub Pages, while manual `workflow_dispatch` runs (`dotnet fsi build.fsx -- publish`) remain available on demand.

The scheduled and dispatchable `sync-sources.yml` workflow (`repository_dispatch` type `docs-updated`, 6-hourly cron, or manual dispatch) refreshes `sources.lock.json`, runs the full `verify` gate, and opens an auto-merging pull request whenever upstream documentation revisions advance.

Downstream library repositories can validate their `docs/ranvier-docs.json` manifest, menu coverage, `.fsx` front matter, directives and intra-repository links directly in their own pull requests using `scripts/validate-repo.mjs` or the reusable workflow:

```yaml
jobs:
  docs-check:
    uses: ranvier-org/Ranvier.Docs/.github/workflows/validate-source.yml@main
```

Before enabling publication, configure the repository's GitHub Pages source as GitHub Actions and its `github-pages` environment. Set `baseUrl` for the actual destination. A project site normally needs `/<repository-name>/`; a custom-domain root may use `/`. `origin` is the scheme/hostname without the path prefix.

This implementation is prepared before the existing sites migrate. Do not assume the eventual central repository name, domain or redirects are already live because a local artifact exists.

## Permissions and credentials

Reading public Git repositories requires no write token. The build job uses read-only repository permissions. The deployment job needs `pages: write` and `id-token: write` for Pages. Optional future dispatch/update automation should use the narrowest separate credential it requires.

Do not put credentials in a repository manifest or static content. Manifests are published source configuration; the importer is not a secret store.

## PR validation

For documentation-only changes, review source selection, menus, rendered output, links and any example compilation. For infrastructure changes, run the complete Node and affected F# suites plus a full site build and browser checks. Treat a successful build artifact as something to inspect, not as automatic proof that every interactive behaviour works.

For a required View/CSS fix, patch a maintained generator or extension, run its repository's required checks, merge the approved-by-checks PR under the user's standing authorization, and publish the release before pinning a package-based consumer to it. Source-based docs development can use the reviewed commit while publication completes, but record that distinction.

## Rollback

Revert the host commit that changed dependency revisions or infrastructure, acquire its lock and rebuild. Preserve the previous successful Pages deployment until the replacement build has passed. Do not repair a broken publication by manually editing output files: that loses the source-to-artifact relationship and is overwritten by the next build.

## Provenance

The uploaded metadata records generation time, source ids, repository refs, exact revisions and dirty flags. CI should build clean acquired checkouts. This record helps establish which library revision produced a rendered example and whether a local preview differs from a published artifact. Absolute local paths in metadata are diagnostic context and should not be interpreted as portable consumer configuration.
