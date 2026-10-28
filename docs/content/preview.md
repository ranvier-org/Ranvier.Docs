---
title: Preview and verification
---

# Preview and verification

## Prepare without rendering

```sh
npm run prepare:docs
```

This validates imports and creates `.generated/content/`, `.generated/static/` and `.generated/site.json`. It is useful for inspecting menus and transformed source links before a full build. It does not compile F# examples or prove that generated HTML works.

Use `--source id=path` to select one local checkout, `--local` to select configured local paths, and `--registry path/to/registry.json` for a different registry. Registry file paths are resolved from the host root. A custom registry still needs the core, view and css ids when using the shared build preset.

## Build once

```sh
npm run build
npm run check:links
```

The build generates coordinated dependency properties and a combined example project, compiles the Release host and generates static output. Inspect `.generated/site.json` to confirm which checkouts and revisions were actually selected. A successful import does not prove successful compilation; a successful compilation does not prove links resolve. Run all relevant gates.

## Local preview

```sh
npm run preview -- --source view=../Ranvier.View
```

The command starts Nacara watch mode after preparing and compiling the host. Use the preview URL printed by Nacara, including the configured project-site prefix if present. Do not assume the preview is served at the origin root.

Source changes are debounced. The coordinator watches imported content and relevant configuration/assets, prepares a new generation, rebuilds and restarts the preview host so navigation and collection changes are applied. A broken import prints an error; fix the source and let the next change retry. This is coordinated restart-based preview, not state-preserving browser hot-module replacement.

Use Ctrl+C to stop the coordinator and its child host. It does not stop a shared SageFs daemon or unrelated processes. If a tool starts an additional browser or server for verification, stop only that task's process afterward.

## Test commands

For the complete acceptance gate, after acquiring sources and installing Chromium:

```sh
npm run verify
```

This restores tools, runs the complete Node suite, builds the site and Release solution, runs both full F# suites, validates checker metadata and generated links/assets, and runs browser interactions. It propagates the same coordinated source/tracing environment to child compilers as the site build. A failed stage stops the command. `.artifacts/verification.json` records the completed stages and whether the full gate finished.

The individual commands below are useful for focused development; `verify` is the reproducible full gate.

For direct .NET commands, propagate the same trace/source-bridge environment to child compilers. In a POSIX shell:

```sh
export RanvierTrace=true
export CustomBeforeMicrosoftCommonTargets="$PWD/src/SourceDependencies.targets"
```

In PowerShell:

```powershell
$env:RanvierTrace = 'true'
$env:CustomBeforeMicrosoftCommonTargets = Join-Path (Get-Location) 'src/SourceDependencies.targets'
```

Run these from the central repository root. `npm run build`, `preview` and `verify` configure their own child environments automatically.

The complete Node suite exercises importer behaviour, registry/source acquisition, output links and host build-input generation:

```sh
npm test
```

The shared F# example suite is an executable Expecto project. Run its complete suite, without filtering, when changing the scanner, workspace generator, map runtime or preset:

```sh
dotnet build Ranvier.Docs.slnx -c Release -p:RanvierTrace=true
dotnet run --project tests/examples/Ranvier.Docs.Maps.Tests.fsproj -c Release --no-build
dotnet fsi tests/examples/MetadataTests.fsx
```

A filter matching no tests is not acceptance evidence. The metadata checks use the restored host assets and verify browser-checker reference handling.

The complete central theme regression suite covers SSR escaping, generated assets, stylesheet equivalence and a real Nacara build:

```sh
dotnet run --project tests/Nacara.Plugin.Ranvier.Tests/Nacara.Plugin.Ranvier.Tests.fsproj -c Release --no-build
```

When modifying the vendored central theme, sync the provenance record rather than silently allowing the central and originating implementations to drift.

## Browser verification

Install the pinned browser test tooling and run the shared browser checks after building:

```sh
npx playwright install chromium
npm run test:browser
```

The tests start the built-output server automatically. To inspect an already built site manually, run `npm run serve` and open its printed URL. This static server does not rebuild source files; use `preview` for editing.

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
