/// <summary>Renders Ranvier documentation Markdown directives through native Ranvier templates.</summary>
module internal Ranvier.Docs.CompatibilityDirectives

open System
open Nacara.Core
open Nacara.Plugins
open Nacara.Plugins.Ranvier.Theme
open Ranvier.View

let private elementWithTrustedHtml (tag: string) (attributes: (string * string) list) (children: StaticView list) (trustedHtml: string) : StaticView =
    fun builder ->
        let attrParts =
            attributes
            |> List.map (fun (name, value) -> builder.Attrs.attribute<obj>(name, value))
        let htmlParts =
            if String.IsNullOrEmpty trustedHtml then []
            else [ builder.Unsafe.html<obj>(UnsafeHtml.Trust trustedHtml) ]
        let childViews =
            children
            |> List.map (fun child -> child builder)
            |> List.toArray
        builder.element<obj>
            (Representation.HtmlTag tag)
            (List.toArray (attrParts @ htmlParts))
            childViews

let private renderNode (contents: Feliz.ViewEngine.ReactElement) =
    Feliz.ViewEngine.Render.htmlView contents

let private centerDirective =
    Directive.create "center" Decode.node
    |> Directive.render (fun _ _ contents ->
        elementWithTrustedHtml "div" [ "style", "display:flex;justify-content:center" ] [] (renderNode contents)
        |> Markup.toLayout)

let private cardsDirective =
    Directive.create "cards" Decode.node
    |> Directive.render (fun _ _ contents ->
        elementWithTrustedHtml "div" [ "class", "rv-cards" ] [] (renderNode contents)
        |> Markup.toLayout)

let private cardDirective =
    Directive.create
        "card"
        (Decode.object (fun get ->
            {| title = get.Required.Field "title" Decode.string
               href = get.Optional.Field "href" Decode.string
               badge = get.Optional.Field "badge" Decode.string |}))
    |> Directive.render (fun _ args contents ->
        let headerChildren =
            [ yield Markup.text args.title
              match args.badge with
              | Some badge when not (String.IsNullOrWhiteSpace badge) ->
                  yield Markup.view "span" [ "class", "rv-badge" ] [ Markup.text badge ]
              | _ -> () ]

        let titleNode = Markup.view "strong" [ "class", "rv-card__title" ] headerChildren
        let bodyNode = elementWithTrustedHtml "div" [ "class", "rv-card__body" ] [] (renderNode contents)

        let view =
            match args.href with
            | Some href when not (String.IsNullOrWhiteSpace href) ->
                Markup.view "a" [ "class", "rv-card"; "href", href ] [ titleNode; bodyNode ]
            | _ ->
                Markup.view "div" [ "class", "rv-card" ] [ titleNode; bodyNode ]

        Markup.toLayout view)

let private statesDirective =
    Directive.create "states" Decode.node
    |> Directive.render (fun _ _ contents ->
        elementWithTrustedHtml "div" [ "class", "rv-states" ] [] (renderNode contents)
        |> Markup.toLayout)

let private validStateKinds =
    set [ "ready"; "pending"; "retained"; "fallback"; "failed"; "recovered" ]

let private stateDirective =
    Directive.create
        "state"
        (Decode.object (fun get ->
            {| kind = get.Required.Field "kind" Decode.string
               label = get.Optional.Field "label" Decode.string
               size = get.Optional.Field "size" Decode.string
               plain = get.Optional.Field "plain" Decode.string |}))
    |> Directive.render (fun _ args _ ->
        let kind = args.kind.Trim().ToLowerInvariant()
        let normalizedKind = if validStateKinds.Contains kind then kind else "ready"
        let defaultLabel =
            if String.IsNullOrEmpty normalizedKind then "Ready"
            else string (Char.ToUpperInvariant normalizedKind[0]) + normalizedKind.Substring(1)
        let label =
            match args.label with
            | Some l when not (String.IsNullOrWhiteSpace l) -> l
            | _ -> defaultLabel
        let classes =
            [ yield "rv-state"
              yield $"rv-state--{normalizedKind}"
              match args.size with
              | Some s when s.Trim().Equals("sm", StringComparison.OrdinalIgnoreCase) -> yield "rv-state--sm"
              | _ -> ()
              match args.plain with
              | Some p when p.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) -> yield "rv-state--plain"
              | _ -> () ]
            |> String.concat " "

        Markup.view "span" [ "class", classes ] [ Markup.text label ]
        |> Markup.toLayout)

let private badgeDirective =
    Directive.create
        "badge"
        (Decode.object (fun get ->
            {| label = get.Required.Field "label" Decode.string |}))
    |> Directive.render (fun _ args _ ->
        Markup.view "span" [ "class", "rv-badge" ] [ Markup.text args.label ]
        |> Markup.toLayout)

let private directives =
    [ centerDirective
      cardsDirective
      cardDirective
      statesDirective
      stateDirective
      badgeDirective ]

/// <summary>Registers all shared Ranvier authoring directives with native server-rendered markup.</summary>
let register site = Nacara.Plugins.Directives.register directives site
