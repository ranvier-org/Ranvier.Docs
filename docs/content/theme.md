---
title: Theme and section styling
---

# Theme and section styling

## Implementation

The shared theme is `Nacara.Plugin.Ranvier.Theme`, under the `Nacara.Plugins.Ranvier.Theme` namespace. Its document shell, navigation, content containers, code blocks and other markup are constructed with Ranvier.View and rendered at build time. Its styles are structured Ranvier.Css declarations/rules.

Nacara's final layout result still uses its HTML adapter. That adapter wraps completed Ranvier-rendered HTML; the theme does not use Partas.Nacara.Theme as its implementation. Progressive JavaScript handles features such as theme preference and narrow navigation. Ordinary page content does not require a browser Ranvier runtime to be readable.

## Shared settings versus per-repository content

Library manifests control section titles, menu order, page labels and landing pages. Common theme settings live in the central F# host. Change the shell centrally so all libraries share tested accessibility, spacing and navigation behaviour. A library-specific information architecture should not require copying the theme.

## Configure a theme

This is the theme's public configuration shape:

```fsharp
open Nacara.Plugins.Ranvier.Theme

let options =
    Theme.defaults
    |> Theme.navbar [ NavbarSection ("View", "view", "/view/") ]
    |> Theme.menu "view" [ Menu.page "view:introduction.md" ]
    |> Theme.footer (
        Markup.view "p" [] [ Markup.text "Built with Ranvier" ]
    )
```

In the central host, collection-qualified menu ids have the form `<source-id>:<content-relative-page>`. The host derives them from manifests; a library author normally changes JSON rather than this code. The section key is the first public route segment.

Register the theme with the site and use its layout for each collection. The central host already does this. Source-specific layout options supply that repository's edit URL while the navbar, theme assets and typography remain shared.

## Module and classic plugin scripts

The current Nacara script-asset boolean controls `defer`; it does not declare an ECMAScript module. For a plugin that emits ESM, explicitly mark its logical script asset path:

```fsharp
let withLiveEditor =
    options
    |> Theme.moduleScripts [ "assets/custom-widget.mjs" ]
```

The option is additive and applies only to named page assets. Ordinary scripts retain classic-script semantics, including their requested defer flag. Use logical asset paths before public-base expansion rather than a full browser URL. The current LiveExample client remains classic because it uses `document.currentScript` to locate its assets. The theme's first-paint preference bootstrap is also self-contained classic JavaScript; it needs no module import or extra browser request.

## Style one repository section

The rendered body carries `data-section` for its route segment. Scope a CSS rule to that value to customise a section without affecting all collections:

```fsharp
let scopedRules =
    [
        Ranvier.Css.Rule.style
            (Ranvier.Css.Selector.unsafeFromText "[data-section=\"view\"]")
            [
                Ranvier.Css.Declaration.create "--nacara-primary" "#006d77"
                Ranvier.Css.Declaration.create "--nacara-sidebar-width" "19rem"
            ]
    ]

let optionsWithSectionStyles =
    options
    |> Theme.css (Ranvier.Css.Stylesheet.render scopedRules)
```

This snippet uses the actual structured CSS API. The explicit selector escape hatch is for a known static selector. It is not a reason to generate selectors from untrusted text. Use the source's configured `route`, which may differ from its id.

`Theme.css` adds overrides after the base stylesheet. Prefer custom-property overrides for colour or layout adjustments; avoid depending on incidental generated CSS text or duplicating the complete base cascade. Test contrast and both colour schemes when changing an accent.

## Structured stylesheet layers

The public style lists include `Styles.tokens`, `Styles.baseRules`, `Styles.navbar`, `Styles.layout`, `Styles.components`, `Styles.code`, `Styles.responsive` and the combined `Styles.rules`. `Styles.render ()` serialises the base cascade. These lists are inspectable structured Rule values, not an embedded raw copy of the upstream stylesheet.

The typography variables include `Css.Tokens.fontSans`, `fontMono`, `fontSize`, `fontSize2xs`, `fontSizeXs`, `fontSizeSm` and `fontSizeLg`. Keep declaration order and media-rule ordering intact when composing overrides; the CSS cascade is part of behaviour.

## Footer and head content

Use `Markup.view` for elements and `Markup.text` for ordinary escaped text. Theme footer/head extension points accept static Ranvier views. For scripts and styles from trusted source, use the theme's dedicated `Markup.script` and `Markup.style` policy rather than placing raw-text HTML into an ordinary template element.

That policy exists because the current View serializer intentionally rejects raw script/style template nodes without an explicit serialization policy. The narrow theme boundary escapes closing delimiters and attribute values while retaining normal Ranvier escaping for other markup. Tests cover mixed-case closing delimiters and external script attributes.

## Assets and licensing

Theme scripts are embedded and emitted with content-addressed filenames by the plugin. The central copy includes source provenance, the Nacara license and applicable Apache notices. Preserve those files when importing upstream-derived fixes or publishing a package.

The source implementation also exists in the `Nacara.Plugins.Ranvier` theme workspace. This central implementation vendors the required source/assets to make clean-machine builds independent of an unpublished sibling package. Record and review synchronisations explicitly; do not let the two copies diverge invisibly.

## Theme verification

The regression suite checks rendered escaping and trusted content boundaries, raw-text script/style handling, generated assets, the normalised full stylesheet cascade and a real Nacara site build. Run that complete suite for a shared theme change, then build all imported docs and inspect the browser layout. A CSS-only change can still break narrow navigation, code overflow or focus visibility even if SSR compiles.
