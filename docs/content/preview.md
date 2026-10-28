---
title: Preview and verification
---

# Preview and verification

## Prepare without rendering

```sh
dotnet fsi build.fsx -- prepare
```

This validates imports and creates `.generated/content/`, `.generated/static/` and `.generated/site.json`. It is useful for inspecting menus and transformed source links before a full build. It does not compile F# examples or prove that generated HTML works.

Use `--source id=path` to select one local checkout, `--local` to select configured local paths, and `--registry path/to/registry.json` for a different registry. Registry file paths are resolved from the host root. A custom registry still needs the core, view and css ids when using the shared build preset.

## Build once

```sh
dotnet fsi build.fsx -- docs
```

Partas.Build restores the cached internal dependencies and pinned tools, acquires locked sources, generates coordinated dependency properties and a combined example project, compiles the Release host, bundles assets, generates static output and checks links. Inspect `.generated/site.json` to confirm the selected checkouts and revisions. Run `dotnet fsi build.fsx -- check` to check an existing output again.

## Local preview

```sh
dotnet fsi build.fsx -- docs --watch --source view=../Ranvier.View
```

The command builds the site and serves its output through the static preview server. Ordinary builds and preview do not run the full test suite. Use the printed preview URL, including the configured project-site prefix if present. Do not assume the preview is served at the origin root.

Partas.Build watches imported content and relevant configuration/assets and debounces changes. It prepares imports, recompiles the host, renders output and checks links while the static server keeps running. Reload the browser after the refresh message. A broken import prints an error; fix the source and save to retry.

Use Ctrl+C to stop the watcher and preview server. It does not stop a shared SageFs daemon or unrelated processes. If a tool starts an additional browser or server for verification, stop only that task's process afterward.

## Test commands

Run the complete acceptance gate explicitly:

```sh
dotnet fsi build.fsx -- verify
```

This restores tools and internal dependencies, installs Chromium automatically, runs the complete Node suite, builds the site and Release solution, runs both full F# suites, validates checker metadata and generated links/assets, and runs browser interactions. Each acceptance stage runs once. It configures the source/tracing environment for child compilers automatically. A failed stage stops the command. Add `--report .artifacts/verification.json` to save a Partas.Build execution report.

For focused development, select a complete suite. The default is `node`, which exercises imports, source acquisition, output links and build-input generation:

```sh
dotnet fsi build.fsx -- test --suite node
dotnet fsi build.fsx -- test --suite examples
dotnet fsi build.fsx -- test --suite theme
dotnet fsi build.fsx -- test --suite metadata
dotnet fsi build.fsx -- test --suite browser
```

The example suite covers the scanner, workspace generator, map runtime and preset. The metadata suite verifies browser-checker reference handling. The theme suite covers SSR escaping, generated assets, stylesheet equivalence and a real Nacara build. Partas.Build configures their prerequisites and child environments; no manual environment setup is needed. Focused suites are development checks; `verify` remains the full acceptance gate.

When modifying the vendored central theme, sync the provenance record rather than silently allowing the central and originating implementations to drift.

## Browser verification

Build the site, then run the shared browser suite:

```sh
dotnet fsi build.fsx -- docs
dotnet fsi build.fsx -- test --suite browser
```

Partas.Build installs Chromium automatically and builds missing output. Run `docs` first after editing source so the browser checks exercise the latest rendering. The tests start the built-output server. To inspect an already built site manually, run `dotnet fsi build.fsx -- serve` and open its printed URL. This static server does not rebuild source files; use `docs --watch` for editing.

Inspect at least these behaviours after a shared rendering change:

1. Open the central landing page and one page from each imported source.
2. Confirm the selected source and section navigation are correct.
3. Follow a cross-source link and a heading fragment.
4. Check a source edit link points to the library, not staging.
5. Confirm content and navigation are present before JavaScript runs.
6. Interact with an SSR counter and observe the reactive update.
7. Start or change a motion example and observe the intended result.
8. Compile a supported live editor example and verify its output.
9. Open search and find a page from another collection.
10. Check narrow layout, keyboard navigation and dark/light preference.

Browser testing complements compiler and output checks. A generated bundle can exist while hydration fails; a working counter does not prove the live compiler's metadata is complete.

## Diagnose the selected inputs

Look at source `root`, `revision` and `dirty` in `.generated/site.json`. Local overrides intentionally permit dirty working trees and record that fact. Acquired sources should match the lock used by CI. If local and CI results differ, compare those identities before changing code.

## Generated files

`.generated/` is safe to regenerate as a whole after preserving anything you need for debugging. Do not patch its compiled projects or staged Markdown as a fix. `output/` is the publishing artifact. `.sources/` holds acquired Git checkouts; fetching refuses dirty ones. `node_modules/` and .NET `bin/obj` are restore/build products.

For View/CSS generated APIs, fix the generator or add a maintained extension. Regenerating a library would erase an edit made directly to generated source.
