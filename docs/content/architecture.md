---
title: Architecture and ownership
---

# Architecture and ownership

## Source repositories

A library repository owns its documentation text, images, diagrams, sample code and optional documentation manifest. It remains the place to review a behavioural change with its documentation. Authors can preview their current checkout through the central host without publishing a library package first.

The host does not copy its own theme or build scripts back into library repositories. A manifest is data: it selects content, menus and existing sample projects. Arbitrary source-provided site code is not a required integration mechanism.

## Central repository

`sources.json` is the reviewed registry of imports. `sources.lock.json` records coordinated Git revisions. `scripts/sources.mjs` acquires those revisions. `scripts/import.mjs` validates each repository's configuration and stages a fresh generated tree. The F# Nacara host consumes `.generated/site.json`, renders one collection per repository and provides the shared shell.

`src/Nacara.Plugin.Ranvier` supplies the component/map compilation pipeline. `src/Ranvier.Docs.Examples` supplies the baseline example dependency environment. The theme renders markup with Ranvier.View and styles with Ranvier.Css. Nacara remains the documentation engine; a small adapter bridges its HTML layout contract.

The common environment is for documentation only. Adding CSS or motion to the documentation build does not add those dependencies to every library's distributed package.

Known pre-import adapters prepare the existing View browser demos and the core's performance-history JSON. Their outputs remain ordinary declared/static import inputs. The history snapshot is a locked dataset registration rather than a documentation collection, so operational measurement files cannot accidentally become sidebar pages.

The maintained `src/SourceDependencies.targets` extension makes the .NET and Fable compilation graphs use the same selected core/View/CSS projects. It maps the baseline Ranvier package identities, including traced variants, and existing baseline project references to the coordinated roots. Other package dependencies remain unchanged. The build propagates this hook and trace setting into child compilers so a page cannot accidentally compile one library from source while resolving a second copy from an older package.

This extension is central documentation infrastructure. It does not patch generated View/CSS bindings or rewrite acquired repository source. When extending the shared baseline, add a deliberate tested mapping rather than a broad rule that replaces every package whose name begins with Ranvier.

## Import stages

1. Read and validate the registry.
2. Resolve acquired checkouts or explicit local overrides.
3. Read each optional repository manifest and discover content.
4. Validate landing pages, menus and explicitly hidden pages.
5. Resolve explicit cross-repository links and their heading fragments.
6. Copy content and assets into a temporary generation.
7. Write source metadata, including exact revisions and dirty-checkout status.
8. Replace the previous generated tree only after every import succeeds.

A deleted source page disappears from the next successful generation. Invalid input leaves the previous generation available for inspection. A valid generated tree is still not a published site: Nacara, example compilation and output checks must succeed afterward.

## Paths and identities

A source id is stable machine identity, such as `view`. A route is a stable first URL segment, normally the same as the id. A title is a reader-facing label and can change freely. Cross-repository links use the id, so changing a menu title does not require changing every incoming link.

The source-relative identity of `docs/content/view/motion.md` is `view/motion.md`. Its generated location is `.generated/content/<route>/view/motion.md`. The host's collection id is the source id; the route controls its public prefix. Normal Nacara route and front-matter rules apply after staging.

## Publication and isolation

Builds do not rewrite checked-in library source or documentation inputs. Normal ignored build products can be written in selected checkouts: .NET `bin/obj`, npm `node_modules`, standalone demo `dist` and generated core `docs/static/perf-data`. Those products are generated from their maintained projects/generator; the acquired checkouts remain Git-clean. The documentation importer stages its changes centrally instead of editing the source Markdown.

Existing repository sites and workflows are outside this implementation's migration scope. The central workflow produces a reviewable artifact; production publication is a distinct explicit operation. A failed import, compiler failure or link-check failure must not publish partial output.

## Extending the implementation

Change discovery, path validation, staging or manifest behaviour in the Node importer with corresponding Node tests. Change page layout and styling in the Ranvier theme. Change example compilation/hydration in the shared plugin, with example and browser tests. Change shared dependencies in the preset and review the source lock at the same time. Keep those responsibilities explicit so adding a library does not become another site fork.
