/// <summary>Renders the center Markdown container through native Ranvier templates.</summary>
module internal Ranvier.Docs.CompatibilityDirectives

open Nacara.Core
open Nacara.Plugins
open Ranvier.View
open Ranvier.View.Template

let private centered html (template: TemplateBuilder<unit>) =
    template.element<obj>
        Representation.Tag.Div
        [| template.Attrs.attribute<obj> ("style", "display:flex;justify-content:center")
           template.Unsafe.html<obj> (UnsafeHtml.Trust html) |]
        [||]

let private centerDirective =
    Directive.create "center" Decode.node
    |> Directive.render (fun _ _ contents ->
        Feliz.ViewEngine.Render.htmlView contents
        |> centered
        |> Nacara.Plugins.Ranvier.Theme.Markup.toLayout)

/// <summary>Adds the center directive with its original flex centering and a fragment safe for native layouts.</summary>
let register site = Nacara.Plugins.Directives.register [ centerDirective ] site
