# Central Ranvier Docs Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans or superpowers:subagent-driven-development. Preserve sibling work and execute without additional permission questions, as explicitly requested by the user.

**Goal:** Deliver the complete shared docs host, import contract, Ranvier examples support, own theme and exhaustive author/maintainer guides before repository migration.

**Architecture:** Stage registered repository content into a generated central content tree with validated explicit cross-repository links. A Nacara host uses the shared Ranvier theme and example preset; repository manifests customise navigation and content without owning separate site infrastructure.

**Tech Stack:** Node built-ins for acquisition/staging/validation; F#/.NET, Nacara, Ranvier.View, Ranvier.Css and the existing example compiler/hydrator for publishing.

**Spec:** ../specs/2026-10-28-central-docs.md

## Global constraints

- Existing repository sites are not migrated, disabled or deployed.
- Theme uses server-rendered Ranvier.View and structured Ranvier.Css.
- Content defaults require no per-repository boilerplate; explicit manifests support customisation.
- Every build records exact repository revisions.
- Preserve staged/unstaged work in sibling repositories.

## Review focus

- Cross-repository links, routes and anchors must resolve under a non-root deployment base.
- Import must neither escape configured source roots nor leave stale generated pages.
- Markdown code fences and examples must survive link staging unchanged.
- Docs must be readable without JavaScript, with hydrated examples remaining interactive.
- CI must build from acquired sources without machine-specific sibling paths or secrets.

## Tasks

- [x] Importer: tests first in tests/import.test.mjs, implementation scripts/import.mjs, sources.json and schema. Verify `npm test`.
- [x] Theme: complete existing ../Nacara.Plugins.Ranvier theme plan; Release build, tests, generated docs and CSS checks.
- [x] Examples: reusable local plugin, shared example preset with core/View/motion/CSS, smoke content and regression checks.
- [x] Host: Nacara site consumes generated source/menu metadata, uses own theme and example plugins, build/preview entry point and CI.
- [x] Guides: exhaustive documentation of every manifest option and supported integration, authoring, section, linking, preview and publication workflow.
- [x] Integration: build real representative sources, verify browser output and links, review changes and resolve material findings.

## Execution record

- User approved central docs approach and explicitly requested immediate implementation without further permission questions.
- User explicitly deferred repository migration until the implementation is complete.
- Existing theme task was interrupted; its workspace contains useful partial style work and is being continued in place.
- Standalone workspace is C:/Users/shaya/RiderProjects/Ranvier.Docs, own unborn main repository; source, tests and docs staged, no remote/commits yet. Existing library deployments untouched.
- Locked inputs: five library repositories plus core-performance dataset from perf-data. Central guide is self source. `.generated/site.json` records source/dataset provenance.
- Importer uses CommonMark and parse5 positions, preserves code and executable literate F#, supplies plain-Markdown title metadata, and rebases legacy repository prefixes. Explicit section coverage and cross-repository links are validated.
- All three real View browser demos build/cache. Core performance generator produces real 101-commit history and seven groups from the locked dataset.
- Verified successful full site generation: 148 pages, 534 files. Generated output link/anchor/asset checker passes, with #/ hash-router links checked as client routes.
- Latest independent passes: 109 full example tests; six full theme tests; 42 metadata checks; 34 Node tests (more helper regressions being added). Browser SSR/search passed. Bootstrap import error fixed with self-contained inline source; data-ranvier-ready marks completed activation.
- Metadata preparation now expands compressed Fable.Precompiled checker resources. Measured initial browser parse fell from 42.36s to 1.32s. Editable execution then exposed missing animejs importmap; maintained local runtime/template helper implemented, host integration pending final regeneration.
- First complete `npm run verify` passed tool restore, all Node tests and full site generation, then failed solution gate because referenced external projects became Debug despite -c Release, causing Windows compiler stack overflow in generated CSS bindings. Source bridge must retain parent configuration; this is being fixed with a minimal solution regression.
- Final verification (2026-10-28): complete site generation, Release solution build, 37 Node tests, 109 example tests, seven theme tests, 42 metadata checks, and published links/assets passed. Eight browser checks passed in the combined run; the remaining performance check encountered Chromium inspector-cache eviction while reading the large response. It now reads the dataset through Playwright's request client and passed a targeted rerun. No broad suites were repeated for that test-only correction.
- Live examples now compile, mount, react and recompile edited code using the local animejs runtime. Center directives have an actual-theme rendering regression. Source dependencies retain Release configuration. Independent final review found no material remaining issues.
- View/CSS source fixes were not required; source bridging and runtime preparation are maintained central extensions. Existing repository sites remain unchanged. Publication delivers this central implementation for subsequent migration.
