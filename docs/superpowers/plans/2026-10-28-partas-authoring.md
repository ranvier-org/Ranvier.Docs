# Partas.Build authoring and homepage repair

The author runs `dotnet fsi build.fsx -- docs --watch`, optionally selecting a local source. Partas.Build owns dependency restore, source acquisition, build stages, preview lifecycle, focused tests, full verification and publication. Existing JavaScript parsers and bundlers remain implementation dependencies; npm scripts are not the authoring API.

1. Add Partas.Build command tree and shared source options. Cache dependency bootstrap. Make every pipeline visible through generated help and `--explain`.
2. Reduce the Node build driver to preparation helpers. Move compilation, host execution and preview lifecycle to the F# build. Route browser bundle processes through explicit Partas stages. Keep importer and cache regression coverage.
3. Replace CI's manually coordinated commands with the same Partas `verify` command. Provide focused test commands; preview never runs test suites.
4. Rewrite author and maintainer instructions around the automatic workflow, keeping full configuration reference.
5. Diagnose and restore the original core homepage layout and presentation within the shared host. Add a targeted regression.
6. Verify changed orchestration and homepage, push, merge and publish. Reuse passing unrelated checks rather than repeating full tests during edits.

Review focus: bootstrap is automatic and repeatable; source paths with spaces and repeated overrides retain argument boundaries; watch rebuilds the selected sources and excludes build outputs; check failures stop publication; original homepage styles and content survive shared rendering.

## Verification record

- The real Partas `prepare` workflow restored dependencies and built all three browser demos through direct .NET/Fickl/Vite stages. The old npm scripts and Node build/verification drivers were removed.
- The real Partas `docs` workflow compiled successfully, rendered all 148 pages and passed the output link/anchor/asset check. Source acquisition reused locked checkouts without network fetches.
- Focused source acquisition checks cover local/unpublished overrides, offline cached checkouts, missing acquired folders, partially initialized checkouts and preservation of dirty inputs. Browser/performance preparation checks cover serializable plans, cache validation and changed-input rejection.
- Three actual CLI smoke checks cover repeated paths with spaces, focused versus full verification stages, and compiler environment propagation when invoked from another directory. Partas 0.8.2 root environment defaults do not reach child commands in this configuration; compiler and test stages explicitly carry their required settings.
- A source watcher smoke check observed an original documentation edit and terminated on cancellation. Preview keeps a static server running while rebuilding output; the browser is reloaded after the completion message.
- Both targeted core homepage browser checks pass: desktop/mobile layout, state icons, collection isolation, automatic demo cycling, pausing and keyboard language selection. A browser CSS probe verifies the final core-only list/code resets against the original presentation.
- Publishing uses the same Partas verification tree in GitHub Actions. Existing repository sites remain untouched.
