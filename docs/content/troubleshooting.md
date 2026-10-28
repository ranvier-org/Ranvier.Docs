---
title: Troubleshooting
---

# Troubleshooting

## Checkout missing

Run `dotnet fsi build.fsx -- sources` for acquired inputs. If using `--local`, confirm every selected local path exists. Use a single `--source id=path` override when only one library should come from a developer checkout. The importer intentionally does not fall back silently to a different source revision.

## Registry differs from lock

A repository/ref changed while its accepted lock entry did not. Review the intended change, then run `dotnet fsi build.fsx -- sources --refresh all` (or name the affected ids) and validate the new coordinated set. Do not delete the lock just to suppress the diagnostic.

## Dirty acquired checkout

Acquisition refuses to overwrite `.sources/<id>` with uncommitted changes. Preserve those changes in an authoring checkout or commit them before fetching. A deliberate local override is the normal development path. The command does not force-reset a library checkout.

## No pages found

Check the manifest's `content` directory and exclusions. The default is `docs/content/`. Page discovery includes Markdown and `.fsx`; static files and `.fs` implementation source are not automatically documentation pages. A repository with plain Markdown directly under `docs/` needs that explicit content root.

## Missing or duplicate navigation page

An explicit menu must cover every discovered page or put it in `unlisted`. Remove obsolete menu entries after deleting a page, list a new page in its intended section, and remove duplicate page entries. Default discovery avoids this maintenance until a custom menu is supplied.

## Unknown manifest option

Check spelling against the [configuration reference](configuration.md). Repository manifests accept a fixed versioned contract. An unsupported option is not silently ignored; extend the importer, schema and tests together if the capability belongs in the shared contract.

## Invalid path or symlink

Use forward-slash relative paths without `.`/`..` segments. Content, sample projects and static assets must stay inside the source repository. Choose the checkout itself with `--source` rather than making an asset path escape the repository. Materialise needed documentation assets instead of symlinking private/external directories.

## Missing repository page or heading

Read the source/page and line in the diagnostic. Confirm the destination source id, content-relative filename and heading fragment. A repository title is not its id. `repo:view/...` targets the View registration even if the public label is “Views and motion”. Check the final HTML id for complex headings.

## Ordinary link works locally but breaks after publishing

Look for a leading slash that escapes the configured `baseUrl`, a hard-coded old repository-site URL, a title-derived route that changed, or a relative asset base that depends on the current page's output directory. Run `dotnet fsi build.fsx -- check` against the actual generated output. Opening one page by filename is not a complete hosted-route test.

## Example module is missing

Check `examplesProject` in the source manifest and `.generated/Examples.fsproj`. Confirm its compile items and dependencies, and use a distinctive fully qualified module name. Editing generated project references is temporary; fix the manifest or the checked-in sample project.

## Fable or npm resolution fails

Separate .NET references from browser runtime packages. NuGet dependencies belong to sample projects; npm runtime dependencies belong to `packages`. Check that package versions agree across sources. Run `dotnet fsi build.fsx -- docs` to restore the internal dependencies and pinned tools automatically. Verify the selected View/CSS/core revisions and the Xantham checker metadata versions.

## An example renders but does not interact

Inspect the browser console and network requests for the generated client bundle. Verify hydration markers survived HTML processing; the Ranvier examples plugin must run after HTML minification. Confirm the page's bundle URL includes the deployment base and that event handlers do not assume a different host id.

Each successfully activated example host gains `data-ranvier-ready="true"`; `data-ranvier-map-cell` identifies its authored cell id. The readiness attribute is set after its asynchronous entry import has completed strict hydration. An absent readiness attribute with a logged import/hydration error points to client activation, while the SSR content can still be present. Browser tests wait for this state before interacting instead of relying on a fixed delay or clicking repeatedly.

## The live editor fails while a rendered component succeeds

These use different compilation paths. The live editor needs checker metadata and a standalone-supported API surface. Compiler-plugin-dependent forms are not automatically supported. Confirm the example explicitly mounts into its output host and uses the shared preset. Check the first reported compiler error rather than repeatedly pressing Run.

## Motion fails during the server build

Keep browser-only effects out of module initialisation and server rendering. Initial markup must be constructible without a DOM animation engine. Use established SSR-safe motion components and start browser work in the appropriate lifecycle. Verify an actual browser action as well as the static initial state.

## Script or style template nodes are rejected

Ranvier.View versions used by the theme require an explicit policy for raw-text HTML. The theme supplies a narrow trusted script/style serialization boundary while ordinary attributes and text are escaped through Ranvier. Do not bypass escaping for an entire document to work around that guard. Upstream fixes belong in a maintained View implementation/extension, never a generated DOM file.

## Preview does not reflect a change

Confirm the source path being watched from `.generated/site.json`. The coordinator rebuilds/restarts after imports; terminal errors can leave the last valid output visible. Fix import/compile errors first. A change to the host/theme implementation may require restarting the coordinator; library content and manifest changes are its ordinary watched inputs.

## CI differs from a developer machine

Compare source revisions, dirty flags, .NET SDK, pinned tools, npm lock and NuGet feeds. A package available only in a local cache is not a portable dependency. Reproduce with acquired locked inputs and no local overrides. Keep exact failure output when reporting a tool bug.

## Generated library source needs a fix

Find its generator or add a maintained extension at the supported seam. Editing generated source is not a durable fix. For a required View/CSS change, include a regression, run its full acceptance gates, and coordinate the merged/published version with the docs dependency lock.
