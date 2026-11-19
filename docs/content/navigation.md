---
title: Sections and navigation
---

# Sections and navigation

## Three distinct choices

Repository navigation selects a library. Section navigation groups pages within that library. The page's table of contents follows its headings. Keep these separate: adding a heading does not create another library, and adding a menu section does not move source files or change their routes.

The host registry's source order controls the shared library navigation. Each library manifest controls the sidebar for that source. Markdown headings control its local table of contents.

## Default discovery

Without explicit `sections`, discovered Markdown (`.md`) and literate F# (`.fsx`) pages are organised automatically by directory hierarchy and front-matter `order`:

- Root pages appear first (`index.md` / `README.md` at the top, then numeric `order` ascending, then path).
- Subdirectories containing an `index.md` become clickable overview groups (`MenuGroup`) whose children are ordered by `order` ascending then path.
- Subdirectories without an `index.md` become collapsible subsections (`MenuSection`).
- Internal tooling folders (`.ai`, `.agents`, `.claude`, `.codex`, `.nacara`, `superpowers`) are excluded automatically.
- Page titles resolve from YAML front matter (including `(** --- ... --- *)` in `.fsx` files), then the first level-one heading, then the filename.

## Define hierarchical sections, groups, badges and links

```json
{
  "version": 1,
  "title": "Views and motion",
  "index": "index.md",
  "unlisted": ["index.md"],
  "sections": [
    {
      "title": "Documentation",
      "items": [
        { "dir": "guide" },
        {
          "section": "Views & motion",
          "items": [
            { "path": "view/getting-started.md", "title": "Start here" },
            { "path": "view/motion.md", "badge": "updated" }
          ]
        },
        {
          "group": "analyzers/index.md",
          "badge": "new",
          "dir": "analyzers"
        },
        {
          "link": "https://ranvier-org.github.io/Ranvier.View/proofs/",
          "title": "Formal proofs",
          "badge": "experimental"
        }
      ]
    }
  ]
}
```

Each section accepts `items` (or `pages` for simple 1-level lists). Supported item shapes:

- `"path/to/page.md"` or `{ "path": "...", "title"?: "...", "badge"?: "new"|"updated"|"experimental"|"beta"|"deprecated" }`
- `{ "section": "Subheading", "items": [...] }` — collapsible nested subsection at any depth
- `{ "group": "folder/index.md", "title"?: "...", "badge"?: "...", "items"?: [...], "dir"?: "folder" }` — overview page with collapsible children
- `{ "dir": "folder" }` — expands non-unlisted pages and subdirectories in `folder/` ordered by `index.md`, `order`, and path
- `{ "link": "https://...", "title": "...", "badge"?: "..." }` — external or reference link

An explicit menu must cover every discovered page unless it is listed in `unlisted`.

## Content layout & Ranvier domain directives

Alongside Nacara's built-in callout, tab, step, details, preview and file-tree directives, the shared host registers native Ranvier server-rendered directives available to every repository:

````markdown
:::center
Centred table or figure.
:::

::::states
:::state kind="pending" label="Pending" size="sm"
:::
:::state kind="recovered" label="Recovered"
:::
::::

::::cards
:::card title="Reactive Signals" href='repo:core/guide/signals.fsx' badge="Updated"
Fine-grained reactive state and derived memos.
:::
::::
````

`href` attributes on `:::card` and raw HTML `<a>` elements support `repo:<id>/<path>#anchor` cross-repository links with full build-time target and heading validation.

## Landing pages

`index` is a content-relative source path. It controls the library navigation's destination, not the central site's root. The central root is a shared landing page listing all registered sources.

An explicit `index` avoids depending on a discovery fallback. Keep the landing page in its normal section unless intentionally hiding it. A hidden page still needs to be listed under `unlisted` when sections are explicit.

## Hidden versus excluded

Use `unlisted` for a page that should render but not appear in the sidebar, such as a compatibility appendix reached from a reference page:

```json
{
  "version": 1,
  "unlisted": ["reference/compatibility.md"],
  "sections": [
    { "title": "Guide", "pages": ["index.md", "reference/index.md"] }
  ]
}
```

Use `exclude` for files that should not enter the generated site at all. An excluded page cannot be the target of a valid cross-repository link. `unlisted` is presentation, not access control; hidden pages remain public and searchable.

## Page front matter

Use ordinary Nacara front matter for page metadata, including `title` and `description`:

```yaml
---
title: Reactive animation
description: Connect timeline state to reactive signals.
---
```

The repository manifest organises pages; page front matter describes one page. The central host does not attempt to reinterpret every theme-specific Nacara option as an organisation registry setting. Keep custom route/layout behaviour within the supported Nacara/theme contract and verify it in the rendered output.

## Source edit links

Each collection knows its original repository and configured ref. Its edit URL points back to that repository's original content directory and the page's relative source path. Readers should never be sent to `.generated/` in the central repository to edit library prose.

For example, a View page staged under `.generated/content/view/view/motion.md` originates at `Ranvier.View/docs/content/view/motion.md`. The edit link uses the source repository path, while its public link uses the shared site's route.

## Links from a section landing page

Use normal Markdown links for pages in the same source:

```markdown
- [Components](components.md)
- [Hydration](../server/hydration.md)
```

Use `repo:` links for other sources. This keeps documentation portable across a changed public hostname or deployment base. The [linking guide](links-assets.md) explains the complete syntax and validation.

## Changing the common shell

Repository authors should use sections, labels and content rather than fork the shell. Common typography, spacing, navigation behaviour and dark-mode support belong to the central Ranvier theme. A change there is validated across every imported collection. The result is shared presentation with repository-owned information architecture.
