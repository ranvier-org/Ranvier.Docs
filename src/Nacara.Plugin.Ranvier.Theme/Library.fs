module Nacara.Plugins.Ranvier.Theme.Css

open Ranvier.Css

/// <summary>Typed custom properties for the default theme typography.</summary>
module Tokens =
    /// <summary>A font-family entry accepted by the typed family builder.</summary>
    type FamilyEntry = CssValue.Categories.Font.Sequence.Family.Input.Family.Value
    /// <summary>Default typography declarations in definition order.</summary>
    let tokenDeclarations: ResizeArray<Declaration> = ResizeArray()
    /// <summary>Records the variable's default declaration and returns the variable.</summary>
    let inline (==>) (a: CssValue.Variable<#Generated.CssGrammar.IValue>) b: CssValue.Variable<#Generated.CssGrammar.IValue> = CssValue.Variable.declaration a b |> tokenDeclarations.Add; a
    /// <summary>The proportional font stack used by body text.</summary>
    let fontSans = Css.variable "--nacara-font-sans" ==> CssValue.Font.Family.create [
        CssValue.Font.Family.uiSansSerif :> FamilyEntry
        CssValue.Font.Family.systemUi
        Css.customIdent "-apple-system"
        Css.string "Segoe UI"
        Css.customIdent "Roboto"
        CssValue.Font.Family.sansSerif
    ]
    /// <summary>The monospace font stack used by source code.</summary>
    let fontMono = Css.variable "--nacara-font-mono" ==> CssValue.Font.Family.create [
        CssValue.Font.Family.uiMonospace :> FamilyEntry
        CssValue.string "JetBrains Mono"
        CssValue.string "Fira Code"
        CssValue.string "SFMono-Regular"
        CssValue.customIdent "Menlo"
        CssValue.Font.Family.monospace
    ]
    /// <summary>The base font-size variable, defaulting to one rem.</summary>
    let fontSize = Css.variable "--nacara-font-size" ==> Css.rem 1
    /// <summary>The smallest font-size variable, defaulting to three quarters of a rem.</summary>
    let fontSize2xs = Css.variable "--nacara-font-size-2xs" ==> Css.rem 0.75
    /// <summary>The extra-small font-size variable, defaulting to seven eighths of a rem.</summary>
    let fontSizeXs = Css.variable "--nacara-font-size-xs" ==> Css.rem 0.875
    /// <summary>The small font-size variable, defaulting to fifteen sixteenths of a rem.</summary>
    let fontSizeSm = Css.variable "--nacara-font-size-sm" ==> Css.rem 0.9375
    /// <summary>The large font-size variable, defaulting to nineteen sixteenths of a rem.</summary>
    let fontSizeLg = Css.variable "--nacara-font-size-lg" ==> Css.rem 1.1875
