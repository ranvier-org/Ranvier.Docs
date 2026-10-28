module Nacara.Plugin.Ranvier.Theme

open Ranvier.Css

let fontSans = Css.fontFamily

let baseStyle = [
    Rule.style (Selector.unsafeFromText "*,*::before,*::after") [
        Css.boxSizing CssValue.BoxSizing.borderBox
    ]
    Rule.style (Selector.element "html") [
        Css.fontFamily (CssValue.var "--nacara-font-sans")
    ]
]
