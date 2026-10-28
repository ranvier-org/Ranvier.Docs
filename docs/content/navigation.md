---
title: Sections and navigation
---

# Sections and navigation

## Three distinct choices

Repository navigation selects a library. Section navigation groups pages within that library. The page's table of contents follows its headings. Keep these separate: adding a heading does not create another library, and adding a menu section does not move source files or change their routes.

The host registry's source order controls the shared library navigation. Each library manifest controls the sidebar for that source. Markdown headings control its local table of contents.

## Default discovery

Without explicit `sections`, all discovered Markdown and `.fsx` pages are listed in one section. The default ordering is deterministic by path. A page's menu title comes from front-matter `title`, then its first level-one heading, then its filename with hyphens replaced by spaces.

This is enough to hook up an existing repository. Use an explicit menu when the documentation has a teaching order or multiple kinds of material; filename prefixes should not be required just to place a tutorial before reference material.

## Define custom sections

```json
{
  "version": 1,
  "title": "Views and motion",
  "index": "index.md",
  "sections": [
    {
      "title": "Learn the renderer",
      "pages": ["index.md", "view/getting-started.md", "view/components.md"]
    },
    {
      "title": "Motion",
      "pages": [
        { "path": "view/motion.md", "title": "Animation overview" },
        "view/motion-reactive.md"
      ]
    },
    {
      "title": "Server rendering",
      "pages": ["server/index.md", "server/rendering.md", "server/hydration.md"]
    }
  ]
}
```

This example illustrates the format; a real explicit menu must also list all the other discovered pages in that source, or declare them `unlisted`. Section and page order are the array order. Sections are flat groups in this contract. To express another conceptual level, use another named section or a landing page with links; arbitrary recursively nested section objects are not supported.

Changing a section title or menu label does not rename files. Changing a page path changes its source identity and can affect incoming links. Prefer stable descriptive paths, even when reorganising the sidebar.

## Content layout directives

The shared Markdown pipeline retains Nacara's built-in callout, tab, step, details, preview and file-tree directives. It also registers the core documentation's existing `center` container, rendered with a native Ranvier server view. This centres the container's content, including a table as a whole, without requiring Partas.Nacara.Theme:

````markdown
:::center
Centred content.
:::
````

Use headings and section configuration for information structure; use a directive only for a local presentation need. Unknown directives produce a build warning so authors can distinguish an unsupported layout from ordinary Markdown.

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
