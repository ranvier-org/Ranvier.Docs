---
title: Links, anchors and assets
---

# Links, anchors and assets

## Links within one repository

Keep ordinary relative Markdown links:

```markdown
[Installation](installation.md)
[Components](../view/components.md)
[Cleanup section](lifetimes.md#cleanup)
![Lifetime diagram](images/lifetimes.svg)
```

Paths are relative to the source page. The importer preserves the content directory's relative layout. Nacara resolves page routes and deployment prefixes when it renders the site. Link to source `.md` or `.fsx` files when possible instead of guessing their eventual HTML filename.

## Links to another repository

Use the stable source id followed by the destination's content-relative path:

```markdown
[Rendering](repo:view/server/rendering.md)
[Typed CSS](repo:css/view/css.md)
[Signals](repo:core/guide/signals.fsx)
```

The form is `repo:<source-id>/<page-path>`, optionally followed by `#heading`. The id comes from `sources.json`, not from the repository's display title or its route. Paths name source pages relative to the destination's configured content directory. They do not include `docs/content/` or the public deployment prefix.

During import, these become relative links between staged source files. The publishing engine then resolves public routes. This avoids hard-coding `github.io` addresses and continues to work under a different `baseUrl`.

## Heading fragments and link titles

```markdown
[Reactive timeline](repo:view/view/motion-reactive.md#timeline "Read the timeline guide")
```

The importer checks that the destination page exists and validates the heading fragment. It parses CommonMark headings, including ATX (`#`, `##`, and so on), Setext headings, inline formatting/code, explicit `{#id}` suffixes and explicit HTML `id`/`name` attributes. Automatic ids are lowercased, remove punctuation and join whitespace with hyphens; repeated heading names get `-1`, `-2`, and so on.

The final HTML checker is authoritative for rendered anchor ids. Prefer a simple unique heading name or explicit supported id. Engine-specific heading extensions can differ from CommonMark interpretation, so verify the generated anchor when using them.

Page paths may use URL-encoded characters; malformed encoding is an error. Avoid spaces in new source filenames. Fragments are decoded before validation.

## Reference links

Reference definitions work too:

```markdown
Read the [rendering guide][rendering].

[rendering]: repo:view/server/rendering.md
```

Only parsed Markdown link destinations are interpreted as repository links. The importer preserves fenced and indented code, quoted/list code blocks and multiline inline-code spans. For literate `.fsx` pages it transforms documentation comment regions while preserving executable F# source, strings and ordinary comments. A guide can show a `repo:` example without accidentally importing it as a live link.

## External and published links

Use full HTTPS links for genuinely external destinations. When linking from outside the shared site, use the published URL and public route. Internal cross-repository links should normally use source identities so they survive hosting changes.

For existing GitHub repository sites, parsed Markdown links/images and raw HTML `href`, `src` and `poster` attributes beginning with `/<repository-name>/` are rebased to the shared deployment prefix and that source's route. This preserves existing iframe and diagram references while repositories await migration. Code samples, HTML comments, script strings and already central URLs remain unchanged. The compatibility mapping does not blindly rewrite all text or external URLs; new documentation should use source links and base-aware assets.

The output checker intentionally does not request external websites. External link availability is a separate check and can vary independently of a reproducible documentation build.

## Content assets

Keep a diagram next to the page that uses it, or under a content-relative images folder. Non-page files in the content tree are staged without changing their bytes. Relative Markdown image and download links retain that organisation.

## Static assets

`docs/static/` is optional. Its contents are copied under the source's public route. A file `docs/static/downloads/sample.json` belongs beneath `<baseUrl>/<route>/downloads/sample.json` in the generated site.

Use page-relative links appropriate to the page's published location, or the site's supported base-aware asset helpers for generated HTML. A leading `/` means the hosting origin's root, which may escape a GitHub project site's deployment base. The output checker rejects that mistake when it can inspect the generated reference.

An explicit static directory can be configured with `static`; use `false` to disable it. Static copying is separate from page discovery. Putting a Markdown file in `docs/static/` makes it a downloadable file, not a page with navigation and a theme.

## Standalone browser examples

An existing example app can be built separately and copied with `browserBundles`:

```json
{
  "version": 1,
  "browserBundles": [
    { "path": "examples/demo/dist", "route": "examples/demo" }
  ]
}
```

The example's assets must use a compatible relative/public base. Build the directory before importing. The host copies it; it does not infer how to run every library's independent application build. Prefer an ordinary rendered component fence for a small documentation example, since that shares compilation, CSS, SSR and hydration infrastructure.

The existing View Playground, Motion and Hydration demos have a maintained central build adapter. Their default bundle declarations live in the View registration's `defaults`. Before import, the adapter restores/builds those three source applications with coordinated core/View/CSS roots and relative Vite assets. It caches successful builds using the selected paths, Node version, tool/configuration files and relevant source inputs. Changes invalidate that cache. The Motion output includes its additional scenario pages.

This is explicit compatibility for known applications, not automatic execution of arbitrary build commands from a manifest. For another standalone app, build its output before `dotnet fsi build.fsx -- docs`, or add and test a deliberate central adapter. Normal small component/map/live fences use the shared pipeline and need no standalone app build.

## Broken links

An unknown repository id, nonexistent page or missing explicitly linked heading fails import with the originating source/page and line. Incorrect ordinary page links are caught by Nacara and the final HTML checker. Missing images, scripts, stylesheets and downloadable files are checked in generated output.

Hash-router links such as `#/gallery` in the standalone motion routing demo are treated as client routes rather than DOM heading ids. The containing HTML file must still exist. Ordinary `#heading` links continue to require a real rendered anchor. The checker parses real HTML attributes, so displayed code, comments and script string literals are not mistaken for navigable links.

Fix the source link rather than adding a hand-edited generated file. When a destination moves, update incoming `repo:` links in the same coordinated source update. For public URLs that readers may have bookmarked, also plan redirects during migration.
