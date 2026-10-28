namespace Nacara.Plugins.Ranvier.Theme

open Ranvier.Css

/// <summary>Default theme stylesheet rules, in cascade order.</summary>
module Styles =

    /// <summary>Default tokens rules in source order.</summary>
    let tokens: Rule list = [
        Rule.descriptors "property" "--nacara-content-width" [
            Declaration.create "syntax" "\"<length>\""
            Declaration.create "inherits" "true"
            Declaration.create "initial-value" "600px"
        ]
        Rule.style (Selector.unsafeFromText ":root") [
            Declaration.create "color-scheme" "light dark"
            Css.Tokens.tokenDeclarations[0]
            Css.Tokens.tokenDeclarations[1]
            Declaration.create "--nacara-space-1" "0.25rem"
            Declaration.create "--nacara-space-2" "0.5rem"
            Declaration.create "--nacara-space-3" "0.75rem"
            Declaration.create "--nacara-space-4" "1rem"
            Declaration.create "--nacara-space-6" "1.5rem"
            Declaration.create "--nacara-space-8" "2rem"
            Declaration.create "--nacara-space-12" "3rem"
            Declaration.create "--nacara-radius" "0.5rem"
            Declaration.create "--nacara-radius-sm" "0.25rem"
            Declaration.create "--nacara-content-width" "75ch"
            Declaration.create "--nacara-splash-width" "68rem"
            Declaration.create "--nacara-layout-gap" "var(--nacara-space-8)"
            Declaration.create "--nacara-sidebar-width" "17rem"
            Declaration.create "--nacara-toc-width" "15rem"
            Declaration.create "--nacara-navbar-height" "3.5rem"
            Declaration.create "--nacara-navbar-opacity" "85%"
            Declaration.create "--nacara-navbar-blur" "8px"
            Declaration.create "--nacara-control-height" "2.25rem"
            Declaration.create "--nacara-control-radius" "var(--nacara-radius-sm)"
            Declaration.create "--nacara-bg" "#ffffff"
            Declaration.create "--nacara-bg-subtle" "#f6f7f9"
            Declaration.create "--nacara-bg-raised" "#ffffff"
            Declaration.create "--nacara-border" "#d0d7de"
            Declaration.create "--nacara-text" "#1b1f24"
            Declaration.create "--nacara-text-muted" "#5a6470"
            Declaration.create "--nacara-primary" "#6669d7"
            Declaration.create "--nacara-primary-contrast" "#ffffff"
            Declaration.create "--nacara-primary-subtle" "#ececfb"
            Declaration.create "--nacara-shadow" "0 1px 2px rgb(16 24 40 / 6%), 0 4px 12px rgb(16 24 40 / 6%)"
            Declaration.create "--nacara-note" "#6669d7"
            Declaration.create "--nacara-tip" "#17935f"
            Declaration.create "--nacara-warning" "#b5730c"
            Declaration.create "--nacara-danger" "#cf3b3b"
            Declaration.create "--nacara-heading" "var(--nacara-text)"
            Declaration.create "--nacara-code-inline-bg" "var(--nacara-bg-subtle)"
            Declaration.create "--nacara-code-inline-border" "var(--nacara-border)"
            Declaration.create "--nacara-code-inline-text" "currentColor"
            Declaration.create "--tok-comment" "#a0a1a7"
            Declaration.create "--tok-string" "#50a14f"
            Declaration.create "--tok-number" "#986801"
            Declaration.create "--tok-constant" "#986801"
            Declaration.create "--tok-constructor" "#4078f2"
            Declaration.create "--tok-property" "#e45649"
            Declaration.create "--tok-escape" "#0184bc"
            Declaration.create "--tok-keyword" "#a626a4"
            Declaration.create "--tok-operator" "#0184bc"
            Declaration.create "--tok-function" "#4078f2"
            Declaration.create "--tok-type" "#c18401"
            Declaration.create "--tok-namespace" "#4078f2"
            Declaration.create "--tok-variable" "#383a42"
            Declaration.create "--tok-parameter" "#383a42"
            Declaration.create "--tok-punctuation" "#383a42"
            Declaration.create "--tok-tag" "#e45649"
            Declaration.create "--tok-attribute" "#986801"
            Declaration.create "--tok-preprocessor" "#a626a4"
            Declaration.create "--tok-invalid" "#e45649"
            Declaration.create "--tok-inserted" "#50a14f"
            Declaration.create "--tok-deleted" "#e45649"
        ]
        Rule.style (Selector.unsafeFromText ":root[data-theme=\"light\"]") [
            Css.colorScheme CssValue.Color.Scheme.light
        ]
        Rule.style (Selector.unsafeFromText ":root[data-theme=\"dark\"]") [
            Css.colorScheme CssValue.Color.Scheme.dark
            Declaration.create "--nacara-bg" "#0f1419"
            Declaration.create "--nacara-bg-subtle" "#161c23"
            Declaration.create "--nacara-bg-raised" "#1a2129"
            Declaration.create "--nacara-border" "#2a333d"
            Declaration.create "--nacara-text" "#e6edf3"
            Declaration.create "--nacara-text-muted" "#9aa7b4"
            Declaration.create "--nacara-primary" "#9b9ef0"
            Declaration.create "--nacara-primary-contrast" "#0f1419"
            Declaration.create "--nacara-primary-subtle" "#21224a"
            Declaration.create "--nacara-shadow" "0 1px 2px rgb(0 0 0 / 40%), 0 4px 14px rgb(0 0 0 / 35%)"
            Declaration.create "--nacara-note" "#9b9ef0"
            Declaration.create "--nacara-tip" "#56d4a0"
            Declaration.create "--nacara-warning" "#e2b23c"
            Declaration.create "--nacara-danger" "#ff7b72"
            Declaration.create "--tok-comment" "#7f848e"
            Declaration.create "--tok-string" "#98c379"
            Declaration.create "--tok-number" "#d19a66"
            Declaration.create "--tok-constant" "#d19a66"
            Declaration.create "--tok-constructor" "#61afef"
            Declaration.create "--tok-property" "#e06c75"
            Declaration.create "--tok-escape" "#56b6c2"
            Declaration.create "--tok-keyword" "#c678dd"
            Declaration.create "--tok-operator" "#56b6c2"
            Declaration.create "--tok-function" "#61afef"
            Declaration.create "--tok-type" "#e5c07b"
            Declaration.create "--tok-namespace" "#61afef"
            Declaration.create "--tok-variable" "#abb2bf"
            Declaration.create "--tok-parameter" "#abb2bf"
            Declaration.create "--tok-punctuation" "#abb2bf"
            Declaration.create "--tok-tag" "#e06c75"
            Declaration.create "--tok-attribute" "#d19a66"
            Declaration.create "--tok-preprocessor" "#c678dd"
            Declaration.create "--tok-invalid" "#e06c75"
            Declaration.create "--tok-inserted" "#98c379"
            Declaration.create "--tok-deleted" "#e06c75"
        ]
    ]

    /// <summary>Default base rules in source order.</summary>
    let baseRules: Rule list = [
        Rule.style (Selector.unsafeFromText "*,\u000a*::before,\u000a*::after") [
            Css.boxSizing CssValue.BoxSizing.borderBox
        ]
        Rule.style (Selector.element "html") [
            Declaration.create "font-family" "var(--nacara-font-sans)"
        ]
        Rule.style (Selector.element "body") [
            Declaration.create "margin" "0"
            Css.fontSize (Css.rem 1.0)
            Css.lineHeight (Css.number 1.65)
            Declaration.create "color" "var(--nacara-text)"
            Declaration.create "background" "var(--nacara-bg)"
            Declaration.create "-webkit-font-smoothing" "antialiased"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content :where(h1, h2, h3, h4, h5, h6, [id]),\u000a.nacara-content,\u000a:target") [
            Declaration.create "scroll-margin-top" "calc(var(--nacara-navbar-height) + var(--nacara-space-4))"
        ]
        Rule.media "(prefers-reduced-motion: no-preference)" [
            Rule.style (Selector.element "html") [
                Css.scrollBehavior CssValue.ScrollBehavior.smooth
            ]
        ]
        Rule.style (Selector.element "a") [
            Declaration.create "color" "var(--nacara-primary)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.unsafeFromText "a:hover") [
            Css.textDecoration CssValue.TextDecoration.underline
        ]
        Rule.style (Selector.unsafeFromText ":where(a, button, select, summary, input, [tabindex]):focus-visible") [
            Declaration.create "outline" "2px solid var(--nacara-primary)"
            Css.outlineOffset (Css.px 2.0)
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
        ]
        Rule.style (Selector.element "kbd") [
            Declaration.create "padding" "0.1em 0.4em"
            Declaration.create "font-family" "var(--nacara-font-mono)"
            Css.fontSize (Css.em 0.8125)
            Css.lineHeight (Css.number 1.0)
            Declaration.create "color" "var(--nacara-text)"
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Css.borderBottomWidth (Css.px 2.0)
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
        ]
        Rule.style (Selector.className "nacara-skip-link") [
            Css.position CssValue.Position.absolute
            Declaration.create "left" "var(--nacara-space-2)"
            Declaration.create "top" "var(--nacara-space-2)"
            Css.zIndex (Css.integer 100)
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-4)"
            Declaration.create "color" "var(--nacara-primary-contrast)"
            Declaration.create "background" "var(--nacara-primary)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "transform" "translateY(-200%)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-skip-link:focus") [
            Declaration.create "transform" "translateY(0)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.className "nacara-visually-hidden") [
            Css.position CssValue.Position.absolute
            Css.width (Css.px 1.0)
            Css.height (Css.px 1.0)
            Css.margin (Css.px (-1.0))
            Declaration.create "padding" "0"
            Declaration.create "border" "0"
            Css.overflow CssValue.Overflow.hidden
            Declaration.create "clip-path" "inset(50%)"
            Css.whiteSpace CssValue.WhiteSpace.nowrap
        ]
    ]

    /// <summary>Default navbar rules in source order.</summary>
    let navbar: Rule list = [
        Rule.style (Selector.className "nacara-navbar") [
            Css.position CssValue.Position.sticky
            Declaration.create "top" "0"
            Css.zIndex (Css.integer 30)
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-4)"
            Declaration.create "height" "var(--nacara-navbar-height)"
            Declaration.create "padding" "0 var(--nacara-space-4)"
            Declaration.create "background" "color-mix(in srgb, var(--nacara-bg) var(--nacara-navbar-opacity), transparent)"
            Declaration.create "backdrop-filter" "blur(var(--nacara-navbar-blur))"
            Declaration.create "border-bottom" "1px solid var(--nacara-border)"
            Declaration.create "color" "var(--nacara-text)"
        ]
        Rule.style (Selector.className "nacara-navbar__brand") [
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-2)"
            Css.fontWeight (Css.number 650.0)
            Declaration.create "color" "var(--nacara-text)"
        ]
        Rule.style (Selector.className "nacara-navbar__items") [
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-1)"
            Css.listStyle CssValue.ListStyle.none
            Declaration.create "margin" "0"
            Declaration.create "padding" "0"
        ]
        Rule.style (Selector.className "nacara-navbar__items--end") [
            Css.marginLeft CssValue.Margin.Left.auto
        ]
        Rule.style (Selector.className "nacara-navbar__link") [
            Css.display CssValue.Display.inlineFlex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-2)"
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-3)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.fontSize (Css.rem 0.9375)
            Css.fontWeight (Css.number 500.0)
        ]
        Rule.style (Selector.unsafeFromText "button.nacara-navbar__link") [
            Css.border CssValue.Border.none
            Declaration.create "background" "none"
            Css.fontFamily CssValue.All.``inherit``
            Css.lineHeight CssValue.All.``inherit``
            Css.cursor CssValue.Cursor.pointer
        ]
        Rule.style (Selector.unsafeFromText "button.nacara-navbar__link[data-nacara-dropdown]::after") [
            Css.content (Css.string "")
            Css.flex CssValue.Flex.none
            Css.width (Css.rem 1.1)
            Css.height (Css.rem 1.1)
            Declaration.create "background" "currentColor"
            Declaration.create "mask" "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23000' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E\")\u000a        center / 1rem no-repeat"
            Declaration.create "transition" "transform 0.15s ease"
            Css.opacity (Css.number 0.55)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-dropdown[data-open=\"true\"] > button.nacara-navbar__link") [
            Declaration.create "background" "var(--nacara-primary-subtle)"
            Declaration.create "color" "var(--nacara-text)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-dropdown[data-open=\"true\"] > button.nacara-navbar__link::after") [
            Declaration.create "transform" "rotate(180deg)"
            Css.opacity (Css.number 1.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-navbar__link:hover") [
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "color" "var(--nacara-text)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-navbar__link[aria-current=\"page\"],\u000a.nacara-navbar__link[data-active=\"true\"]") [
            Declaration.create "color" "var(--nacara-primary)"
            Declaration.create "background" "var(--nacara-primary-subtle)"
        ]
        Rule.style (Selector.className "nacara-dropdown") [
            Css.position CssValue.Position.relative
        ]
        Rule.style (Selector.className "nacara-dropdown__panel") [
            Css.position CssValue.Position.absolute
            Declaration.create "top" "calc(100% + var(--nacara-space-1))"
            Declaration.create "left" "0"
            Css.width CssValue.Width.maxContent
            Css.minWidth (Css.rem 15.0)
            Declaration.create "max-width" "min(28rem, 90vw)"
            Declaration.create "margin" "0"
            Declaration.create "padding" "var(--nacara-space-2)"
            Declaration.create "background" "var(--nacara-bg-raised)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Declaration.create "box-shadow" "var(--nacara-shadow-floating, var(--nacara-shadow))"
            Css.listStyle CssValue.ListStyle.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-dropdown__panel::before") [
            Css.content (Css.string "")
            Css.position CssValue.Position.absolute
            Declaration.create "right" "0"
            Css.bottom (Css.percent 100.0)
            Declaration.create "left" "0"
            Declaration.create "height" "calc(var(--nacara-space-1) + 2px)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-dropdown:not([data-open=\"true\"]) .nacara-dropdown__panel") [
            Css.display CssValue.Display.none
        ]
        Rule.style (Selector.className "nacara-dropdown__item") [
            Css.display CssValue.Display.block
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-3)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "color" "var(--nacara-text)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-dropdown__item:hover") [
            Declaration.create "background" "var(--nacara-primary-subtle)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.className "nacara-dropdown__description") [
            Css.display CssValue.Display.block
            Css.fontSize (Css.rem 0.8125)
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.className "nacara-dropdown__divider") [
            Css.height (Css.px 1.0)
            Declaration.create "margin" "var(--nacara-space-2) 0"
            Declaration.create "background" "var(--nacara-border)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-navbar__items > .nacara-dropdown__divider") [
            Css.height (Css.rem 2.0)
            Declaration.create "margin" "0 var(--nacara-space-2)"
            Declaration.create "background" "none"
            Declaration.create "border-inline-end" "1px solid var(--nacara-border)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-navbar__items > .nacara-dropdown__divider + li:has(> .nacara-icon-button),\u000a.nacara-navbar__items > li:has(> .nacara-icon-button) + .nacara-dropdown__divider") [
            Declaration.create "margin-inline-start" "calc(var(--nacara-space-2) * -1)"
        ]
        Rule.nested (Selector.className "nacara-theme-select") [
            Declaration.create "height" "var(--nacara-control-height)"
            Declaration.create "padding" "0 var(--nacara-space-2)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-control-radius)"
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.font CssValue.All.``inherit``
            Css.fontSize (Css.rem 0.875)
            Css.cursor CssValue.Cursor.pointer
        ] [
            Rule.style (Selector.unsafeFromText "&:hover") [
                Declaration.create "color" "var(--nacara-text)"
            ]
        ]
        Rule.style (Selector.className "nacara-icon-button") [
            Css.display CssValue.Display.inlineFlex
            Css.alignItems CssValue.AlignItems.center
            Css.justifyContent CssValue.JustifyContent.center
            Declaration.create "width" "var(--nacara-control-height)"
            Declaration.create "height" "var(--nacara-control-height)"
            Declaration.create "padding" "0"
            Declaration.create "border" "1px solid transparent"
            Declaration.create "border-radius" "var(--nacara-control-radius)"
            Declaration.create "background" "none"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.cursor CssValue.Cursor.pointer
        ]
        Rule.style (Selector.unsafeFromText ".nacara-icon-button:hover") [
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "color" "var(--nacara-text)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-icon-button > *") [
            Css.display CssValue.Display.flex
        ]
        Rule.style (Selector.unsafeFromText ".nacara-icon-button svg") [
            Css.display CssValue.Display.block
            Css.width (Css.rem 1.125)
            Css.height (Css.rem 1.125)
        ]
        Rule.style (Selector.unsafeFromText "[data-nacara-menu-toggle]") [
            Css.display CssValue.Display.none
        ]
    ]

    /// <summary>Default layout rules in source order.</summary>
    let layout: Rule list = [
        Rule.style (Selector.className "nacara-layout") [
            Css.display CssValue.Display.grid
            Declaration.create "grid-template-columns" "var(--nacara-sidebar-width)\u000a        minmax(0, var(--nacara-content-width))\u000a        var(--nacara-toc-width)"
            Css.justifyContent CssValue.JustifyContent.center
            Css.alignItems CssValue.AlignItems.start
            Declaration.create "gap" "var(--nacara-layout-gap)"
            Css.maxWidth (Css.rem 108.0)
            Declaration.create "margin" "0 auto"
            Declaration.create "padding" "var(--nacara-space-8) var(--nacara-space-4)"
        ]
        Rule.style (Selector.className "nacara-layout--no-toc") [
            Declaration.create "grid-template-columns" "var(--nacara-sidebar-width)\u000a        minmax(0, var(--nacara-content-width))"
        ]
        Rule.style (Selector.className "nacara-layout--splash") [
            Declaration.create "grid-template-columns" "minmax(0, 1fr)"
            Declaration.create "max-width" "var(--nacara-splash-width)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-layout--splash > .nacara-content") [
            Css.maxWidth CssValue.MaxWidth.none
        ]
        Rule.style (Selector.className "nacara-layout--bare") [
            Declaration.create "grid-template-columns" "minmax(0, 1fr)"
            Css.maxWidth CssValue.MaxWidth.none
            Declaration.create "padding" "0"
        ]
        Rule.style (Selector.className "nacara-sidebar") [
            Css.position CssValue.Position.sticky
            Declaration.create "top" "calc(var(--nacara-navbar-height) + var(--nacara-space-4))"
            Declaration.create "max-height" "calc(100vh - var(--nacara-navbar-height) - var(--nacara-space-8))"
            Css.overflowX CssValue.Overflow.X.hidden
            Css.overflowY CssValue.Overflow.Y.auto
        ]
        Rule.style (Selector.className "nacara-sidebar__filter") [
            Declaration.create "width" "calc(100% - 2 * var(--nacara-space-3))"
            Declaration.create "margin" "3px var(--nacara-space-3) var(--nacara-space-3)"
            Declaration.create "padding" "var(--nacara-space-1) var(--nacara-space-2)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "background" "var(--nacara-bg)"
            Declaration.create "color" "var(--nacara-text)"
            Css.font CssValue.All.``inherit``
            Css.fontSize (Css.rem 0.875)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__filter::placeholder") [
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__filter:focus-visible") [
            Declaration.create "outline" "2px solid var(--nacara-primary)"
            Css.outlineOffset (Css.px 1.0)
        ]
        Rule.style (Selector.className "nacara-sidebar__sections") [
            Css.display CssValue.Display.none
            Css.flexDirection CssValue.Flex.Direction.column
            Css.gap (Css.px 1.0)
            Css.listStyle CssValue.ListStyle.none
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "padding" "0 0 var(--nacara-space-4)"
            Declaration.create "border-bottom" "1px solid var(--nacara-border)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__sections .nacara-sidebar__link[aria-current]") [
            Declaration.create "background" "var(--nacara-primary-subtle)"
            Declaration.create "color" "var(--nacara-primary)"
            Css.fontWeight (Css.number 600.0)
        ]
        Rule.style (Selector.className "nacara-sidebar--drawer") [
            Css.display CssValue.Display.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar--drawer .nacara-sidebar__sections") [
            Declaration.create "margin-bottom" "0"
            Declaration.create "padding-bottom" "0"
            Css.borderBottom CssValue.Border.Bottom.none
        ]
        Rule.style (Selector.className "nacara-sidebar__section") [
            Declaration.create "margin-bottom" "var(--nacara-space-6)"
        ]
        Rule.style (Selector.className "nacara-sidebar__title") [
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "margin" "0 0 var(--nacara-space-2)"
            Css.fontSize (Css.rem 0.75)
            Css.fontWeight (Css.number 700.0)
            Css.letterSpacing (Css.em 0.06)
            Css.textTransform CssValue.TextTransform.uppercase
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.className "nacara-sidebar__list") [
            Css.display CssValue.Display.flex
            Css.flexDirection CssValue.Flex.Direction.column
            Css.gap (Css.px 1.0)
            Css.listStyle CssValue.ListStyle.none
            Declaration.create "margin" "0"
            Declaration.create "padding" "0"
        ]
        Rule.style (Selector.className "nacara-sidebar__link") [
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-3)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.fontSize (Css.rem 0.9375)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__link:hover") [
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "color" "var(--nacara-text)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__link[aria-current=\"page\"]") [
            Declaration.create "background" "var(--nacara-primary-subtle)"
            Declaration.create "color" "var(--nacara-primary)"
            Css.fontWeight (Css.number 600.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__link > span:first-child,\u000a.nacara-sidebar__group-link > span:first-child") [
            Css.flex (Css.number 1.0)
            Declaration.create "min-width" "0"
        ]
        Rule.style (Selector.className "nacara-sidebar__group") [
            Declaration.create "margin" "var(--nacara-space-1) 0"
        ]
        Rule.style (Selector.className "nacara-sidebar__group-title") [
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-2)"
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-3)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.fontSize (Css.rem 0.9375)
            Css.cursor CssValue.Cursor.pointer
            Css.listStyle CssValue.ListStyle.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group-title::-webkit-details-marker") [
            Css.display CssValue.Display.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group-title::after") [
            Css.content (Css.string "")
            Css.flex CssValue.Flex.none
            Css.marginLeft CssValue.Margin.Left.auto
            Css.width (Css.rem 1.6)
            Css.height (Css.rem 1.6)
            Declaration.create "background" "currentColor"
            Declaration.create "mask" "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23000' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m9 6 6 6-6 6'/%3E%3C/svg%3E\")\u000a        center / 1.2rem no-repeat"
            Declaration.create "transition" "transform 0.15s ease"
            Css.opacity (Css.number 0.55)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group-title:hover::after") [
            Css.opacity (Css.number 1.0)
            Declaration.create "background" "var(--nacara-primary)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group[open] > .nacara-sidebar__group-title::after") [
            Declaration.create "transform" "rotate(90deg)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group-title:hover .nacara-sidebar__group-link:not(:hover)") [
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.className "nacara-sidebar__group-link") [
            Declaration.create "min-width" "0"
            Css.overflowWrap CssValue.Overflow.Wrap.anywhere
            Declaration.create "margin" "calc(-1 * var(--nacara-space-1)) calc(-1 * var(--nacara-space-2))"
            Declaration.create "padding" "var(--nacara-space-1) var(--nacara-space-2)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Css.color CssValue.All.``inherit``
            Css.font CssValue.All.``inherit``
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group-link:hover") [
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "color" "var(--nacara-text)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group-link[aria-current=\"page\"]") [
            Declaration.create "background" "var(--nacara-primary-subtle)"
            Declaration.create "color" "var(--nacara-primary)"
            Css.fontWeight (Css.number 600.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-sidebar__group > .nacara-sidebar__list") [
            Declaration.create "margin-left" "calc(var(--nacara-space-3) + 0.25rem)"
            Declaration.create "padding-left" "var(--nacara-space-2)"
            Declaration.create "border-left" "1px solid var(--nacara-border)"
        ]
        Rule.style (Selector.className "nacara-badge") [
            Css.flex CssValue.Flex.none
            Declaration.create "margin-left" "var(--nacara-space-2)"
            Declaration.create "padding" "0.05rem 0.4rem"
            Css.borderRadius (Css.px 999.0)
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.fontSize (Css.rem 0.6875)
            Css.fontWeight (Css.number 600.0)
            Css.letterSpacing (Css.em 0.02)
            Css.textTransform CssValue.TextTransform.uppercase
            Css.verticalAlign CssValue.VerticalAlign.middle
        ]
        Rule.style (Selector.unsafeFromText ".nacara-badge[data-kind=\"new\"],\u000a.nacara-badge[data-kind=\"updated\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-tip) 18%, transparent)"
            Declaration.create "color" "var(--nacara-tip)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-badge[data-kind=\"experimental\"],\u000a.nacara-badge[data-kind=\"beta\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-warning) 18%, transparent)"
            Declaration.create "color" "var(--nacara-warning)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-badge[data-kind=\"deprecated\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-danger) 18%, transparent)"
            Declaration.create "color" "var(--nacara-danger)"
        ]
        Rule.style (Selector.className "nacara-content") [
            Declaration.create "min-width" "0"
            Css.width (Css.percent 100.0)
            Declaration.create "max-width" "var(--nacara-content-width)"
            Css.marginInline CssValue.Margin.Inline.auto
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content :is(h1, h2, h3, h4, h5, h6)") [
            Declaration.create "color" "var(--nacara-heading)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content h1") [
            Css.fontSize (Css.rem 2.25)
            Css.lineHeight (Css.number 1.2)
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Css.letterSpacing (Css.em (-0.02))
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content h2") [
            Css.fontSize (Css.rem 1.5)
            Declaration.create "margin" "var(--nacara-space-12) 0 var(--nacara-space-3)"
            Css.letterSpacing (Css.em (-0.01))
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content h3") [
            Css.fontSize (Css.rem 1.1875)
            Declaration.create "margin" "var(--nacara-space-8) 0 var(--nacara-space-2)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content p,\u000a.nacara-content ul,\u000a.nacara-content ol") [
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content ul,\u000a.nacara-content ol") [
            Declaration.create "padding-left" "var(--nacara-space-6)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content li + li") [
            Declaration.create "margin-top" "var(--nacara-space-1)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content img") [
            Css.maxWidth (Css.percent 100.0)
            Css.height CssValue.Height.auto
            Declaration.create "border-radius" "var(--nacara-radius)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content table") [
            Css.display CssValue.Display.block
            Css.width (Css.percent 100.0)
            Declaration.create "margin" "0 0 var(--nacara-space-6)"
            Css.borderCollapse CssValue.Border.Collapse.collapse
            Css.fontSize (Css.rem 0.9375)
            Css.overflowX CssValue.Overflow.X.auto
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content th,\u000a.nacara-content td") [
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-3)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Css.textAlign CssValue.TextAlign.left
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content th") [
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Css.fontWeight (Css.number 650.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content :is(th, td) > :last-child") [
            Declaration.create "margin-bottom" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content blockquote") [
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-4)"
            Declaration.create "border-left" "3px solid var(--nacara-border)"
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content :not(pre) > code") [
            Declaration.create "padding" "0.125em 0.375em"
            Declaration.create "border" "1px solid var(--nacara-code-inline-border)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "background" "var(--nacara-code-inline-bg)"
            Declaration.create "color" "var(--nacara-code-inline-text)"
            Declaration.create "font-family" "var(--nacara-font-mono)"
            Css.fontSize (Css.em 0.875)
            Css.fontVariantLigatures CssValue.Font.Variant.Ligatures.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content :is(h2, h3, h4, h5, h6)") [
            Css.position CssValue.Position.relative
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content .nacara-heading-anchor") [
            Css.position CssValue.Position.absolute
            Css.left (Css.em (-1.5))
            Css.top (Css.em 0.15)
            Css.width (Css.em 1.0)
            Css.height (Css.em 1.0)
            Css.opacity (Css.number 0.0)
            Declaration.create "background" "currentColor"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.textDecoration CssValue.TextDecoration.none
            Declaration.create "transition" "opacity 0.12s"
            Declaration.create "mask" "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23000' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71'/%3E%3Cpath d='M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71'/%3E%3C/svg%3E\")\u000a        no-repeat center / 0.85em"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content :is(h2, h3, h4, h5, h6):hover .nacara-heading-anchor,\u000a.nacara-content .nacara-heading-anchor:focus-visible") [
            Css.opacity (Css.number 1.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content .nacara-heading-anchor:hover") [
            Declaration.create "color" "var(--nacara-primary)"
        ]
        Rule.media "(max-width: 1000px)" [
            Rule.style (Selector.unsafeFromText ".nacara-content .nacara-heading-anchor") [
                Css.position CssValue.Position.``static``
                Css.display CssValue.Display.inlineBlock
                Declaration.create "margin-left" "var(--nacara-space-2)"
                Css.verticalAlign CssValue.VerticalAlign.middle
            ]
        ]
        Rule.media "(hover: none)" [
            Rule.style (Selector.unsafeFromText ".nacara-content .nacara-heading-anchor") [
                Css.opacity (Css.number 0.5)
            ]
        ]
        Rule.style (Selector.className "nacara-toc") [
            Css.position CssValue.Position.sticky
            Declaration.create "top" "calc(var(--nacara-navbar-height) + var(--nacara-space-4))"
            Css.fontSize (Css.rem 0.875)
        ]
        Rule.style (Selector.className "nacara-toc__title") [
            Declaration.create "margin" "0 0 var(--nacara-space-2)"
            Css.fontSize (Css.rem 0.75)
            Css.fontWeight (Css.number 700.0)
            Css.letterSpacing (Css.em 0.06)
            Css.textTransform CssValue.TextTransform.uppercase
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.className "nacara-toc__list") [
            Css.display CssValue.Display.flex
            Css.flexDirection CssValue.Flex.Direction.column
            Declaration.create "gap" "var(--nacara-space-1)"
            Css.listStyle CssValue.ListStyle.none
            Declaration.create "margin" "0"
            Declaration.create "padding" "0"
            Declaration.create "border-left" "1px solid var(--nacara-border)"
        ]
        Rule.style (Selector.className "nacara-toc__link") [
            Css.display CssValue.Display.block
            Css.marginLeft (Css.px (-1.0))
            Declaration.create "padding" "0.125rem var(--nacara-space-3)"
            Declaration.create "border-left" "2px solid transparent"
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-toc__link:hover") [
            Declaration.create "color" "var(--nacara-text)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-toc__link[data-active=\"true\"]") [
            Declaration.create "color" "var(--nacara-primary)"
            Declaration.create "border-left-color" "var(--nacara-primary)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-toc__link[data-level=\"3\"]") [
            Declaration.create "padding-left" "calc(var(--nacara-space-3) * 2)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-toc__link[data-level=\"4\"]") [
            Declaration.create "padding-left" "calc(var(--nacara-space-3) * 3)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-toc__link[data-level=\"5\"]") [
            Declaration.create "padding-left" "calc(var(--nacara-space-3) * 4)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-toc__link[data-level=\"6\"]") [
            Declaration.create "padding-left" "calc(var(--nacara-space-3) * 5)"
        ]
        Rule.style (Selector.className "nacara-page-nav") [
            Css.display CssValue.Display.flex
            Declaration.create "gap" "var(--nacara-space-4)"
            Declaration.create "margin-top" "var(--nacara-space-12)"
            Declaration.create "padding-top" "var(--nacara-space-6)"
            Declaration.create "border-top" "1px solid var(--nacara-border)"
        ]
        Rule.style (Selector.className "nacara-page-nav__link") [
            Css.flex (Css.number 1.0)
            Declaration.create "padding" "var(--nacara-space-3) var(--nacara-space-4)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-page-nav__link:hover") [
            Declaration.create "border-color" "var(--nacara-primary)"
            Css.textDecoration CssValue.TextDecoration.none
        ]
        Rule.style (Selector.className "nacara-page-nav__link--next") [
            Css.textAlign CssValue.TextAlign.right
        ]
        Rule.style (Selector.className "nacara-page-nav__label") [
            Css.display CssValue.Display.block
            Css.fontSize (Css.rem 0.75)
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.className "nacara-edit-link") [
            Css.display CssValue.Display.inlineFlex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-1)"
            Declaration.create "margin-top" "var(--nacara-space-8)"
            Css.fontSize (Css.rem 0.875)
            Declaration.create "color" "var(--nacara-text-muted)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-edit-link svg") [
            Css.width (Css.rem 0.9)
            Css.height (Css.rem 0.9)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-edit-link:hover") [
            Declaration.create "color" "var(--nacara-primary)"
        ]
        Rule.style (Selector.className "nacara-footer") [
            Declaration.create "padding" "var(--nacara-space-8) var(--nacara-space-4)"
            Declaration.create "background" "var(--nacara-bg)"
            Declaration.create "border-top" "1px solid var(--nacara-border)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.fontSize (Css.rem 0.875)
            Css.textAlign CssValue.TextAlign.center
        ]
    ]

    /// <summary>Default components rules in source order.</summary>
    let components: Rule list = [
        Rule.style (Selector.className "nacara-callout") [
            Declaration.create "--callout-color" "var(--nacara-note)"
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "padding" "var(--nacara-space-3) var(--nacara-space-4)"
            Declaration.create "border" "1px solid color-mix(in srgb, var(--callout-color) 35%, var(--nacara-border))"
            Css.borderLeftWidth (Css.px 3.0)
            Declaration.create "border-left-color" "var(--callout-color)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Declaration.create "background" "color-mix(in srgb, var(--callout-color) 6%, var(--nacara-bg))"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-callout[data-kind=\"tip\"]") [
            Declaration.create "--callout-color" "var(--nacara-tip)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-callout[data-kind=\"warning\"],\u000a.nacara-callout[data-kind=\"caution\"]") [
            Declaration.create "--callout-color" "var(--nacara-warning)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-callout[data-kind=\"danger\"]") [
            Declaration.create "--callout-color" "var(--nacara-danger)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-callout > :last-child") [
            Declaration.create "margin-bottom" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-callout[data-title]::before") [
            Declaration.create "content" "attr(data-title)"
            Css.display CssValue.Display.block
            Declaration.create "margin-bottom" "var(--nacara-space-1)"
            Css.fontWeight (Css.number 650.0)
            Declaration.create "color" "var(--callout-color)"
        ]
        Rule.style (Selector.className "nacara-steps") [
            Css.display CssValue.Display.flex
            Css.flexDirection CssValue.Flex.Direction.column
            Declaration.create "gap" "var(--nacara-space-6)"
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "padding-left" "0"
            Css.listStyle CssValue.ListStyle.none
        ]
        Rule.style (Selector.className "nacara-steps__step") [
            Declaration.create "border-left" "2px solid var(--nacara-border)"
            Declaration.create "padding-left" "var(--nacara-space-4)"
        ]
        Rule.style (Selector.className "nacara-steps__number") [
            Css.display CssValue.Display.block
            Declaration.create "margin-bottom" "var(--nacara-space-1)"
            Declaration.create "color" "var(--nacara-primary)"
            Css.fontSize (Css.rem 0.625)
            Css.fontWeight (Css.number 500.0)
            Css.letterSpacing (Css.em 0.12)
            Css.textTransform CssValue.TextTransform.uppercase
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content .nacara-steps__title") [
            Declaration.create "margin" "0 0 var(--nacara-space-2)"
            Css.fontSize (Css.rem 1.125)
            Css.lineHeight (Css.number 1.2)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content .nacara-steps__step > :last-child") [
            Declaration.create "margin-bottom" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-content .nacara-steps__title .nacara-heading-anchor") [
            Css.position CssValue.Position.``static``
            Css.display CssValue.Display.inlineBlock
            Declaration.create "margin-left" "var(--nacara-space-2)"
            Css.verticalAlign CssValue.VerticalAlign.middle
        ]
        Rule.style (Selector.element "nacara-tabs") [
            Css.display CssValue.Display.block
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Css.overflow CssValue.Overflow.hidden
        ]
        Rule.style (Selector.className "nacara-tabs__list") [
            Css.display CssValue.Display.flex
            Declaration.create "gap" "var(--nacara-space-1)"
            Declaration.create "padding" "var(--nacara-space-1)"
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "border-bottom" "1px solid var(--nacara-border)"
            Css.overflowX CssValue.Overflow.X.auto
        ]
        Rule.style (Selector.className "nacara-tabs__tab") [
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-3)"
            Css.border CssValue.Border.none
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "background" "none"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.font CssValue.All.``inherit``
            Css.fontSize (Css.rem 0.9375)
            Css.whiteSpace CssValue.WhiteSpace.nowrap
            Css.cursor CssValue.Cursor.pointer
        ]
        Rule.style (Selector.unsafeFromText ".nacara-tabs__tab[aria-selected=\"true\"]") [
            Declaration.create "background" "var(--nacara-bg)"
            Declaration.create "color" "var(--nacara-text)"
            Css.fontWeight (Css.number 600.0)
            Declaration.create "box-shadow" "var(--nacara-shadow)"
        ]
        Rule.style (Selector.element "nacara-tab") [
            Css.display CssValue.Display.block
            Declaration.create "padding" "var(--nacara-space-4)"
        ]
        Rule.style (Selector.unsafeFromText "nacara-tab[hidden]") [
            Css.display CssValue.Display.none
        ]
        Rule.style (Selector.unsafeFromText "nacara-tab > :last-child") [
            Declaration.create "margin-bottom" "0"
        ]
        Rule.style (Selector.className "nacara-preview") [
            Declaration.create "margin" "var(--nacara-space-6) 0"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Css.overflow CssValue.Overflow.hidden
        ]
        Rule.style (Selector.unsafeFromText ".nacara-preview > .nacara-code") [
            Declaration.create "margin" "0"
            Css.border CssValue.Border.none
            Declaration.create "border-radius" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-preview > pre") [
            Declaration.create "margin" "0"
            Declaration.create "border-radius" "0"
        ]
        Rule.style (Selector.className "nacara-preview__result") [
            Declaration.create "padding" "var(--nacara-space-4)"
            Declaration.create "border-top" "1px solid var(--nacara-border)"
            Declaration.create "background" "var(--nacara-bg)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-preview__result::before") [
            Css.content (Css.string "Preview")
            Css.display CssValue.Display.block
            Declaration.create "margin-bottom" "var(--nacara-space-3)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.fontSize (Css.rem 0.75)
            Css.fontWeight (Css.number 700.0)
            Css.letterSpacing (Css.em 0.06)
            Css.textTransform CssValue.TextTransform.uppercase
        ]
        Rule.style (Selector.unsafeFromText ".nacara-preview__result > :first-child") [
            Declaration.create "margin-top" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-preview__result > :last-child") [
            Declaration.create "margin-bottom" "0"
        ]
        Rule.style (Selector.className "nacara-details") [
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Declaration.create "background" "var(--nacara-bg-subtle)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details > summary") [
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-2)"
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-4)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Declaration.create "color" "var(--nacara-text)"
            Css.listStyle CssValue.ListStyle.none
            Css.cursor CssValue.Cursor.pointer
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details > summary::-webkit-details-marker") [
            Css.display CssValue.Display.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details > summary::before") [
            Css.content (Css.string "")
            Css.flex CssValue.Flex.none
            Css.width (Css.rem 0.45)
            Css.height (Css.rem 0.45)
            Declaration.create "border-right" "1.5px solid currentColor"
            Declaration.create "border-bottom" "1.5px solid currentColor"
            Declaration.create "transform" "rotate(-45deg)"
            Declaration.create "transition" "transform 0.15s ease"
            Css.opacity (Css.number 0.6)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details[open] > summary::before") [
            Declaration.create "transform" "rotate(45deg)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details > summary:hover") [
            Declaration.create "background" "var(--nacara-bg)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details[open] > summary") [
            Declaration.create "border-bottom" "1px solid var(--nacara-border)"
            Declaration.create "border-end-start-radius" "0"
            Declaration.create "border-end-end-radius" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details > :not(summary)") [
            Declaration.create "margin-inline" "var(--nacara-space-4)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details > summary + *") [
            Declaration.create "margin-top" "var(--nacara-space-3)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-details > :last-child") [
            Declaration.create "margin-bottom" "var(--nacara-space-3)"
        ]
        Rule.style (Selector.className "nacara-file-tree") [
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "padding" "var(--nacara-space-3) var(--nacara-space-4)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Declaration.create "font-family" "var(--nacara-font-mono)"
            Css.fontSize (Css.rem 0.875)
            Css.lineHeight (Css.number 1.7)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree .nacara-file-tree__list") [
            Css.listStyle CssValue.ListStyle.none
            Declaration.create "margin" "0"
            Declaration.create "padding" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree li,\u000a.nacara-file-tree li + li") [
            Declaration.create "margin" "0"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree__list .nacara-file-tree__list") [
            Css.marginLeft (Css.rem 0.55)
            Declaration.create "padding-left" "var(--nacara-space-4)"
            Declaration.create "border-left" "1px solid var(--nacara-border)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree__folder > summary") [
            Css.listStyle CssValue.ListStyle.none
            Css.cursor CssValue.Cursor.pointer
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree__folder > summary::-webkit-details-marker") [
            Css.display CssValue.Display.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree__folder > summary::after") [
            Css.content (Css.string "")
            Css.flex CssValue.Flex.none
            Css.width (Css.rem 0.75)
            Css.height (Css.rem 0.75)
            Declaration.create "background" "currentColor"
            Css.opacity (Css.number 0.5)
            Declaration.create "transition" "transform 0.15s ease"
            Declaration.create "mask" "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23000' stroke-width='3' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E\")\u000a        center / contain no-repeat"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree__folder:not([open]) > summary::after") [
            Declaration.create "transform" "rotate(-90deg)"
        ]
        Rule.style (Selector.className "nacara-file-tree__name") [
            Css.display CssValue.Display.inlineFlex
            Css.alignItems CssValue.AlignItems.center
            Declaration.create "gap" "var(--nacara-space-2)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree__name::before") [
            Css.content (Css.string "")
            Css.flex CssValue.Flex.none
            Css.width (Css.rem 1.0)
            Css.height (Css.rem 1.0)
            Declaration.create "background" "currentColor"
            Css.opacity (Css.number 0.65)
        ]
        Rule.style (Selector.unsafeFromText "[data-kind=\"directory\"] .nacara-file-tree__name::before") [
            Declaration.create "mask" "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23000' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M4 20a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h5l2 3h7a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2Z'/%3E%3C/svg%3E\")\u000a        center / contain no-repeat"
            Declaration.create "color" "var(--nacara-primary)"
            Css.opacity (Css.number 0.85)
        ]
        Rule.style (Selector.unsafeFromText "[data-kind=\"file\"] .nacara-file-tree__name::before") [
            Declaration.create "mask" "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23000' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8Z'/%3E%3Cpath d='M14 3v5h5'/%3E%3C/svg%3E\")\u000a        center / contain no-repeat"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-file-tree strong") [
            Declaration.create "color" "var(--nacara-primary)"
            Css.fontWeight (Css.number 700.0)
        ]
    ]

    /// <summary>Default code rules in source order.</summary>
    let code: Rule list = [
        Rule.style (Selector.className "nacara-code") [
            Declaration.create "margin" "0 0 var(--nacara-space-4)"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius)"
            Declaration.create "background" "var(--nacara-bg-subtle)"
            Css.overflow CssValue.Overflow.hidden
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code[data-frame=\"none\"]") [
            Css.border CssValue.Border.none
            Declaration.create "background" "none"
        ]
        Rule.style (Selector.className "nacara-code__title") [
            Declaration.create "padding" "var(--nacara-space-2) var(--nacara-space-4)"
            Declaration.create "border-bottom" "1px solid var(--nacara-border)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Declaration.create "font-family" "var(--nacara-font-mono)"
            Css.fontSize (Css.rem 0.8125)
        ]
        Rule.style (Selector.className "nacara-code__body") [
            Css.position CssValue.Position.relative
            Declaration.create "padding" "var(--nacara-space-4) 0"
            Declaration.create "max-height" "var(--nacara-code-max-height, none)"
            Css.overflowY CssValue.Overflow.Y.auto
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code pre") [
            Declaration.create "margin" "0"
            Declaration.create "padding" "0"
            Css.overflowX CssValue.Overflow.X.auto
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code code") [
            Css.display CssValue.Display.block
            Declaration.create "font-family" "var(--nacara-font-mono)"
            Css.fontSize (Css.rem 0.875)
            Css.lineHeight (Css.number 1.6)
            Css.fontVariantLigatures CssValue.Font.Variant.Ligatures.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code[data-frame=\"terminal\"] .nacara-code__title") [
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Css.justifyContent CssValue.JustifyContent.center
            Declaration.create "gap" "var(--nacara-space-2)"
            Css.position CssValue.Position.relative
            Css.paddingLeft (Css.rem 4.25)
            Css.paddingRight (Css.rem 4.25)
            Declaration.create "font-family" "var(--nacara-font-sans)"
            Css.fontSize (Css.rem 0.75)
            Css.letterSpacing (Css.em 0.02)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code[data-frame=\"terminal\"] .nacara-code__title::before") [
            Css.content (Css.string "")
            Css.position CssValue.Position.absolute
            Declaration.create "left" "var(--nacara-space-4)"
            Css.width (Css.rem 0.6)
            Css.height (Css.rem 0.6)
            Css.borderRadius (Css.percent 50.0)
            Declaration.create "background" "var(--nacara-danger)"
            Declaration.create "box-shadow" "0.95rem 0 0 var(--nacara-warning),\u000a        1.9rem 0 0 var(--nacara-tip)"
            Css.opacity (Css.number 0.75)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code[data-frame=\"terminal\"] .nacara-code__body") [
            Declaration.create "background" "var(--nacara-bg)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code[data-frame=\"terminal\"] pre") [
            Declaration.create "padding-left" "var(--nacara-space-2)"
        ]
        Rule.style (Selector.className "nacara-code__line") [
            Css.position CssValue.Position.relative
            Css.display CssValue.Display.block
            Declaration.create "padding" "0 var(--nacara-space-4)"
            Declaration.create "border-left" "2px solid transparent"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code[data-line-numbers=\"true\"] .nacara-code__line::before") [
            Declaration.create "content" "attr(data-line)"
            Css.display CssValue.Display.inlineBlock
            Css.width (Css.em 2.5)
            Css.marginLeft (Css.em (-1.0))
            Declaration.create "padding-right" "var(--nacara-space-3)"
            Css.textAlign CssValue.TextAlign.right
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.userSelect CssValue.UserSelect.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code[data-wrap=\"true\"] .nacara-code__line") [
            Css.whiteSpace CssValue.WhiteSpace.preWrap
            Css.wordBreak CssValue.WordBreak.breakWord
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__line[data-marker=\"mark\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-primary) 12%, transparent)"
            Declaration.create "border-left-color" "var(--nacara-primary)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__line[data-marker=\"ins\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-tip) 14%, transparent)"
            Declaration.create "border-left-color" "var(--nacara-tip)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__line[data-marker=\"del\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-danger) 14%, transparent)"
            Declaration.create "border-left-color" "var(--nacara-danger)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__line[data-marker=\"ins\"]::after,\u000a.nacara-code__line[data-marker=\"del\"]::after") [
            Css.position CssValue.Position.absolute
            Declaration.create "top" "0"
            Declaration.create "right" "var(--nacara-space-2)"
            Css.fontWeight (Css.number 700.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__line[data-marker=\"ins\"]::after") [
            Css.content (Css.string "+")
            Declaration.create "color" "var(--nacara-tip)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__line[data-marker=\"del\"]::after") [
            Css.content (Css.string "−")
            Declaration.create "color" "var(--nacara-danger)"
        ]
        Rule.style (Selector.className "nacara-code__word") [
            Declaration.create "padding" "0.1em 0.2em"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "background" "color-mix(in srgb, var(--nacara-primary) 18%, transparent)"
            Css.color CssValue.All.``inherit``
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__word[data-marker=\"ins\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-tip) 22%, transparent)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__word[data-marker=\"del\"]") [
            Declaration.create "background" "color-mix(in srgb, var(--nacara-danger) 22%, transparent)"
        ]
        Rule.nested (Selector.unsafeFromText ".nacara-code__collapsed summary") [
            Declaration.create "padding" "var(--nacara-space-1) var(--nacara-space-4)"
            Declaration.create "border-top" "1px solid var(--nacara-border)"
            Declaration.create "border-bottom" "1px solid var(--nacara-border)"
            Declaration.create "background" "var(--nacara-border)"
            Declaration.create "color" "var(--nacara-text)"
            Css.fontSize (Css.rem 0.8125)
            Css.cursor CssValue.Cursor.pointer
        ] [
            Rule.style (Selector.unsafeFromText "&:hover") [
                Declaration.create "background" "color-mix(in srgb, var(--nacara-text-muted) 25%, var(--nacara-border))"
            ]
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__collapsed[open] summary") [
            Css.borderBottomColor CssValue.Color.transparent
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__collapsed[open] pre") [
            Declaration.create "border-bottom" "1px solid var(--nacara-border)"
            Declaration.create "background" "color-mix(in srgb, var(--nacara-border) 50%, transparent)"
        ]
        Rule.style (Selector.className "nacara-code__copy") [
            Css.position CssValue.Position.absolute
            Declaration.create "top" "var(--nacara-space-2)"
            Declaration.create "right" "var(--nacara-space-2)"
            Css.display CssValue.Display.flex
            Css.alignItems CssValue.AlignItems.center
            Css.justifyContent CssValue.JustifyContent.center
            Css.width (Css.rem 2.0)
            Css.height (Css.rem 2.0)
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "background" "var(--nacara-bg)"
            Declaration.create "color" "var(--nacara-text-muted)"
            Css.opacity (Css.number 0.0)
            Declaration.create "transition" "opacity 0.12s ease"
            Css.cursor CssValue.Cursor.pointer
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__body:hover .nacara-code__copy,\u000a.nacara-code__copy:focus-visible") [
            Css.opacity (Css.number 1.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__copy:hover") [
            Declaration.create "color" "var(--nacara-text)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__copy svg") [
            Css.width (Css.rem 1.125)
            Css.height (Css.rem 1.125)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__copy .nacara-code__copy-done,\u000a.nacara-code__copy[data-copied=\"true\"] .nacara-code__copy-idle") [
            Css.display CssValue.Display.none
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__copy[data-copied=\"true\"] .nacara-code__copy-done") [
            Css.display CssValue.Display.block
            Declaration.create "color" "var(--nacara-tip)"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__copy[data-copied]") [
            Css.opacity (Css.number 1.0)
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__copy[data-copied=\"false\"]") [
            Declaration.create "color" "var(--nacara-danger, var(--nacara-text))"
        ]
        Rule.style (Selector.unsafeFromText ".nacara-code__copy[data-feedback]::after") [
            Declaration.create "content" "attr(data-feedback)"
            Css.position CssValue.Position.absolute
            Declaration.create "right" "calc(100% + var(--nacara-space-2))"
            Declaration.create "padding" "0.125rem 0.5rem"
            Declaration.create "border" "1px solid var(--nacara-border)"
            Declaration.create "border-radius" "var(--nacara-radius-sm)"
            Declaration.create "background" "var(--nacara-bg)"
            Declaration.create "color" "var(--nacara-text)"
            Css.fontSize (Css.rem 0.75)
            Css.lineHeight (Css.number 1.6)
            Css.whiteSpace CssValue.WhiteSpace.nowrap
        ]
        Rule.style (Selector.className "tok-comment") [
            Declaration.create "color" "var(--tok-comment)"
            Css.fontStyle CssValue.Font.Style.italic
        ]
        Rule.style (Selector.className "tok-string") [
            Declaration.create "color" "var(--tok-string)"
        ]
        Rule.style (Selector.className "tok-number") [
            Declaration.create "color" "var(--tok-number)"
        ]
        Rule.style (Selector.className "tok-constant") [
            Declaration.create "color" "var(--tok-constant)"
        ]
        Rule.style (Selector.className "tok-constructor") [
            Declaration.create "color" "var(--tok-constructor)"
        ]
        Rule.style (Selector.className "tok-property") [
            Declaration.create "color" "var(--tok-property)"
        ]
        Rule.style (Selector.className "tok-escape") [
            Declaration.create "color" "var(--tok-escape)"
        ]
        Rule.style (Selector.className "tok-keyword") [
            Declaration.create "color" "var(--tok-keyword)"
        ]
        Rule.style (Selector.className "tok-operator") [
            Declaration.create "color" "var(--tok-operator)"
        ]
        Rule.style (Selector.className "tok-function") [
            Declaration.create "color" "var(--tok-function)"
        ]
        Rule.style (Selector.className "tok-type") [
            Declaration.create "color" "var(--tok-type)"
        ]
        Rule.style (Selector.className "tok-namespace") [
            Declaration.create "color" "var(--tok-namespace)"
        ]
        Rule.style (Selector.className "tok-variable") [
            Declaration.create "color" "var(--tok-variable)"
        ]
        Rule.style (Selector.className "tok-parameter") [
            Declaration.create "color" "var(--tok-parameter)"
        ]
        Rule.style (Selector.className "tok-punctuation") [
            Declaration.create "color" "var(--tok-punctuation)"
        ]
        Rule.style (Selector.className "tok-tag") [
            Declaration.create "color" "var(--tok-tag)"
        ]
        Rule.style (Selector.className "tok-attribute") [
            Declaration.create "color" "var(--tok-attribute)"
        ]
        Rule.style (Selector.className "tok-preprocessor") [
            Declaration.create "color" "var(--tok-preprocessor)"
        ]
        Rule.style (Selector.className "tok-invalid") [
            Declaration.create "color" "var(--tok-invalid)"
            Declaration.create "text-decoration" "underline wavy"
        ]
        Rule.style (Selector.className "tok-inserted") [
            Declaration.create "color" "var(--tok-inserted)"
        ]
        Rule.style (Selector.className "tok-deleted") [
            Declaration.create "color" "var(--tok-deleted)"
        ]
        Rule.style (Selector.className "tok-bold") [
            Css.fontWeight (Css.number 700.0)
        ]
        Rule.style (Selector.className "tok-italic") [
            Css.fontStyle CssValue.Font.Style.italic
        ]
        Rule.style (Selector.className "tok-heading") [
            Css.fontWeight (Css.number 700.0)
            Declaration.create "color" "var(--nacara-primary)"
        ]
    ]

    /// <summary>Default responsive rules in source order.</summary>
    let responsive: Rule list = [
        Rule.media "(min-width: 1600px)" [
            Rule.style (Selector.unsafeFromText ":root") [
                Declaration.create "--nacara-content-width" "88ch"
                Declaration.create "--nacara-layout-gap" "var(--nacara-space-12)"
            ]
        ]
        Rule.media "(max-width: 1200px)" [
            Rule.style (Selector.unsafeFromText ".nacara-layout:not(.nacara-layout--splash, .nacara-layout--bare)") [
                Declaration.create "grid-template-columns" "var(--nacara-sidebar-width)\u000a            minmax(0, var(--nacara-content-width))"
            ]
            Rule.style (Selector.className "nacara-toc") [
                Css.display CssValue.Display.none
            ]
        ]
        Rule.media "(max-width: 860px)" [
            Rule.style (Selector.unsafeFromText ".nacara-layout:not(.nacara-layout--bare)") [
                Declaration.create "grid-template-columns" "minmax(0, 1fr)"
                Declaration.create "padding" "var(--nacara-space-4)"
            ]
            Rule.style (Selector.className "nacara-sidebar") [
                Css.position CssValue.Position.``static``
                Css.maxHeight CssValue.MaxHeight.none
            ]
            Rule.style (Selector.unsafeFromText ".nacara-sidebar:not([data-open=\"true\"])") [
                Css.display CssValue.Display.none
            ]
            Rule.style (Selector.unsafeFromText ".nacara-sidebar[data-open=\"true\"]") [
                Css.position CssValue.Position.``fixed``
                Declaration.create "inset" "var(--nacara-navbar-height) 0 auto 0"
                Css.zIndex (Css.integer 20)
                Declaration.create "max-height" "calc(100dvh - var(--nacara-navbar-height))"
                Declaration.create "padding" "var(--nacara-space-4)"
                Declaration.create "background" "var(--nacara-bg)"
                Declaration.create "border-bottom" "1px solid var(--nacara-border)"
                Declaration.create "box-shadow" "var(--nacara-shadow)"
                Css.overflowY CssValue.Overflow.Y.auto
            ]
            Rule.style (Selector.unsafeFromText ".nacara-sidebar[data-open=\"true\"]:has(.nacara-sidebar__widgets)") [
                Declaration.create "padding-bottom" "0"
            ]
            Rule.style (Selector.unsafeFromText ".nacara-layout:has(.nacara-sidebar[data-open=\"true\"])::before") [
                Css.content (Css.string "")
                Css.position CssValue.Position.``fixed``
                Declaration.create "inset" "var(--nacara-navbar-height) 0 0"
                Css.zIndex (Css.integer 19)
                Declaration.create "background" "rgb(0 0 0 / 0.4)"
            ]
            Rule.style (Selector.unsafeFromText ".nacara-navbar__items:not(.nacara-navbar__items--end)") [
                Css.display CssValue.Display.none
            ]
            Rule.style (Selector.className "nacara-sidebar__sections") [
                Css.display CssValue.Display.flex
            ]
            Rule.style (Selector.unsafeFromText ".nacara-sidebar--drawer[data-open=\"true\"]") [
                Css.display CssValue.Display.block
            ]
            Rule.style (Selector.className "nacara-sidebar__widgets") [
                Css.position CssValue.Position.sticky
                Declaration.create "bottom" "0"
                Css.display CssValue.Display.flex
                Css.alignItems CssValue.AlignItems.center
                Declaration.create "gap" "var(--nacara-space-3)"
                Css.listStyle CssValue.ListStyle.none
                Declaration.create "margin" "0 calc(-1 * var(--nacara-space-4))"
                Declaration.create "padding" "var(--nacara-space-4)"
                Declaration.create "border-top" "1px solid var(--nacara-border)"
                Declaration.create "background" "var(--nacara-bg)"
            ]
            Rule.style (Selector.unsafeFromText ".nacara-sidebar__widgets > li:first-child") [
                Css.marginRight CssValue.Margin.Right.auto
            ]
            Rule.style (Selector.unsafeFromText "[data-nacara-menu-toggle]") [
                Css.display CssValue.Display.inlineFlex
            ]
        ]
        Rule.media "(prefers-reduced-motion: reduce)" [
            Rule.style (Selector.unsafeFromText "*,\u000a    *::before,\u000a    *::after") [
                (Css.animationDuration (Css.ms 0.01) |> Declaration.important)
                (Css.transitionDuration (Css.ms 0.01) |> Declaration.important)
            ]
        ]
    ]

    /// <summary>Complete default stylesheet with tokens preceding all consuming rules.</summary>
    let rules = tokens @ baseRules @ navbar @ layout @ components @ code @ responsive

    /// <summary>Renders the complete default stylesheet for a server-generated asset.</summary>
    let render () = Stylesheet.render rules
