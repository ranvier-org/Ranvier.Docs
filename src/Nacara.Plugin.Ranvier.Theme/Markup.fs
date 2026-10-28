namespace Nacara.Plugins.Ranvier.Theme

open Ranvier.View
open System
open System.Text
open System.Text.RegularExpressions

/// <summary>A static view evaluated with the builder of the server-rendered component.</summary>
type StaticView = TemplateBuilder<unit> -> TemplateView<unit>

/// <summary>Constructs static Ranvier views and renders them without a browser runtime.</summary>
module Markup =
    /// <summary>Creates an element. Attribute values and text children are escaped by Ranvier.</summary>
    let view (tag: string) (attributes: (string * string) list) (children: StaticView list) : StaticView =
        fun builder ->
            builder.element<obj>
                (Representation.HtmlTag tag)
                (attributes
                 |> List.map (fun (name, value) -> builder.Attrs.attribute (name, value))
                 |> List.toArray)
                (children
                 |> List.map (fun child -> child builder)
                 |> List.toArray)

    /// <summary>Creates escaped text, including when its value contains HTML delimiters.</summary>
    let text (value: string) : StaticView =
        fun builder -> builder.text value

    // Ranvier.View rejects script/style template nodes. A meta placeholder keeps the document
    // structurally valid during SSR; the final serialization boundary writes only these two tags.
    let internal rawText tag attributes (content: string) : StaticView =
        if tag <> "script" && tag <> "style" then
            invalidArg (nameof tag) "Expected script or style."

        let attributes =
            attributes
            |> List.map (fun (name, value) -> $" {name}=\"{Representation.Html.escape true value}\"")
            |> String.concat ""

        let content =
            Regex.Replace (
                content,
                "</" + tag,
                MatchEvaluator (fun matched ->
                    if tag = "script" then "<\\/" + matched.Value.Substring(2)
                    else "\\3c " + matched.Value.Substring(1)),
                RegexOptions.IgnoreCase
            )

        let html = $"<{tag}{attributes}>{content}</{tag}>"
        let encoded = Convert.ToBase64String (Encoding.UTF8.GetBytes html)
        view "meta" [ "data-ranvier-raw-text", encoded ] []

    /// <summary>Creates a script from trusted source, escaping closing script delimiters.</summary>
    let script attributes source =
        rawText "script" attributes source

    /// <summary>Creates an inline stylesheet from trusted CSS, escaping closing style delimiters.</summary>
    let style source =
        rawText "style" [] source

    /// <summary>Renders a static view through Ranvier's request-scoped server renderer.</summary>
    let render (view: StaticView) =
        let definition = Component.define (fun _ () -> ()) view
        let html = Server.render Server.Options.defaults definition ()
        let opening, closing = "<!--[r-->", "<!--]-->"

        let html =
            if html.StartsWith opening && html.EndsWith closing then
                html.Substring (opening.Length, html.Length - opening.Length - closing.Length)
            else
                html

        Regex.Replace (
            html,
            "<meta data-ranvier-raw-text=\"([A-Za-z0-9+/=]*)\">",
            MatchEvaluator (fun matched -> Encoding.UTF8.GetString (Convert.FromBase64String matched.Groups[1].Value))
        )

    /// <summary>Adapts completed Ranvier markup to the layout result required by Nacara.</summary>
    let toLayout view =
        Feliz.ViewEngine.Html.rawText (render view)

module internal MarkupSyntax =
    type Attribute =
        | Attribute of string * string
        | Children of StaticView list
        | Trusted of string

    let private element tag attributes : StaticView =
        fun builder ->
            let rawTextElement = tag = "script" || tag = "style"

            let parts =
                attributes
                |> List.choose (function
                    | Attribute (name, value) -> Some (builder.Attrs.attribute<obj>(name, value))
                    | Trusted html when not rawTextElement -> Some (builder.Unsafe.html<obj>(UnsafeHtml.Trust html))
                    | Trusted _ -> None
                    | Children _ -> None)
                |> List.toArray

            let children =
                attributes
                |> List.collect (function
                    | Children views -> views
                    | _ -> [])
                |> List.map (fun child -> child builder)
                |> List.toArray

            if rawTextElement then
                let attrs =
                    attributes
                    |> List.choose (function
                        | Attribute (name, value) -> Some (name, value)
                        | _ -> None)

                let content =
                    attributes
                    |> List.choose (function
                        | Trusted html -> Some html
                        | _ -> None)
                    |> String.concat ""

                Markup.rawText tag attrs content builder
            else
                builder.element<obj> (Representation.HtmlTag tag) parts children

    module prop =
        let custom (name, value) =
            Attribute (name, value)

        let children value =
            Children value

        let text value =
            Children [ Markup.text value ]

        let dangerouslySetInnerHTML value =
            Trusted value

        let className value =
            Attribute ("class", value)

        let ariaExpanded value =
            Attribute ("aria-expanded", if value then "true" else "false")

        let ariaLabel value =
            Attribute ("aria-label", value)

        let tabIndex value =
            Attribute ("tabindex", string value)

        let content value =
            Attribute ("content", value)

        let href value =
            Attribute ("href", value)

        let id value =
            Attribute ("id", value)

        let lang value =
            Attribute ("lang", value)

        let name value =
            Attribute ("name", value)

        let placeholder value =
            Attribute ("placeholder", value)

        let rel value =
            Attribute ("rel", value)

        let role value =
            Attribute ("role", value)

        let src value =
            Attribute ("src", value)

        let title value =
            Attribute ("title", value)

        let type' value =
            Attribute ("type", value)

        let value value =
            Attribute ("value", value)

        module charset =
            let utf8 = Attribute ("charset", "utf-8")

    type Html =
        static member none: StaticView = fun builder -> builder.fragment [||]

        static member span(attributes: Attribute list) =
            element "span" attributes

        static member span(children: StaticView list) =
            element "span" [ Children children ]

        static member span(value: string) =
            element "span" [ Children [ Markup.text value ] ]

        static member li(attributes: Attribute list) =
            element "li" attributes

        static member li(children: StaticView list) =
            element "li" [ Children children ]

        static member li(value: string) =
            element "li" [ Children [ Markup.text value ] ]

        static member a(attributes: Attribute list) =
            element "a" attributes

        static member a(children: StaticView list) =
            element "a" [ Children children ]

        static member a(value: string) =
            element "a" [ Children [ Markup.text value ] ]

        static member button(attributes: Attribute list) =
            element "button" attributes

        static member button(children: StaticView list) =
            element "button" [ Children children ]

        static member button(value: string) =
            element "button" [ Children [ Markup.text value ] ]

        static member ul(attributes: Attribute list) =
            element "ul" attributes

        static member ul(children: StaticView list) =
            element "ul" [ Children children ]

        static member ul(value: string) =
            element "ul" [ Children [ Markup.text value ] ]

        static member header(attributes: Attribute list) =
            element "header" attributes

        static member header(children: StaticView list) =
            element "header" [ Children children ]

        static member header(value: string) =
            element "header" [ Children [ Markup.text value ] ]

        static member select(attributes: Attribute list) =
            element "select" attributes

        static member select(children: StaticView list) =
            element "select" [ Children children ]

        static member select(value: string) =
            element "select" [ Children [ Markup.text value ] ]

        static member option(attributes: Attribute list) =
            element "option" attributes

        static member option(children: StaticView list) =
            element "option" [ Children children ]

        static member option(value: string) =
            element "option" [ Children [ Markup.text value ] ]

        static member details(attributes: Attribute list) =
            element "details" attributes

        static member details(children: StaticView list) =
            element "details" [ Children children ]

        static member details(value: string) =
            element "details" [ Children [ Markup.text value ] ]

        static member summary(attributes: Attribute list) =
            element "summary" attributes

        static member summary(children: StaticView list) =
            element "summary" [ Children children ]

        static member summary(value: string) =
            element "summary" [ Children [ Markup.text value ] ]

        static member div(attributes: Attribute list) =
            element "div" attributes

        static member div(children: StaticView list) =
            element "div" [ Children children ]

        static member div(value: string) =
            element "div" [ Children [ Markup.text value ] ]

        static member p(attributes: Attribute list) =
            element "p" attributes

        static member p(children: StaticView list) =
            element "p" [ Children children ]

        static member p(value: string) =
            element "p" [ Children [ Markup.text value ] ]

        static member nav(attributes: Attribute list) =
            element "nav" attributes

        static member nav(children: StaticView list) =
            element "nav" [ Children children ]

        static member nav(value: string) =
            element "nav" [ Children [ Markup.text value ] ]

        static member input(attributes: Attribute list) =
            element "input" attributes

        static member input(children: StaticView list) =
            element "input" [ Children children ]

        static member input(value: string) =
            element "input" [ Children [ Markup.text value ] ]

        static member html(attributes: Attribute list) =
            element "html" attributes

        static member html(children: StaticView list) =
            element "html" [ Children children ]

        static member html(value: string) =
            element "html" [ Children [ Markup.text value ] ]

        static member head(attributes: Attribute list) =
            element "head" attributes

        static member head(children: StaticView list) =
            element "head" [ Children children ]

        static member head(value: string) =
            element "head" [ Children [ Markup.text value ] ]

        static member meta(attributes: Attribute list) =
            element "meta" attributes

        static member meta(children: StaticView list) =
            element "meta" [ Children children ]

        static member meta(value: string) =
            element "meta" [ Children [ Markup.text value ] ]

        static member title(attributes: Attribute list) =
            element "title" attributes

        static member title(children: StaticView list) =
            element "title" [ Children children ]

        static member title(value: string) =
            element "title" [ Children [ Markup.text value ] ]

        static member link(attributes: Attribute list) =
            element "link" attributes

        static member link(children: StaticView list) =
            element "link" [ Children children ]

        static member link(value: string) =
            element "link" [ Children [ Markup.text value ] ]

        static member script(attributes: Attribute list) =
            element "script" attributes

        static member script(children: StaticView list) =
            element "script" [ Children children ]

        static member script(value: string) =
            element "script" [ Children [ Markup.text value ] ]

        static member style(attributes: Attribute list) =
            element "style" attributes

        static member style(children: StaticView list) =
            element "style" [ Children children ]

        static member style(value: string) =
            element "style" [ Children [ Markup.text value ] ]

        static member body(attributes: Attribute list) =
            element "body" attributes

        static member body(children: StaticView list) =
            element "body" [ Children children ]

        static member body(value: string) =
            element "body" [ Children [ Markup.text value ] ]

        static member main(attributes: Attribute list) =
            element "main" attributes

        static member main(children: StaticView list) =
            element "main" [ Children children ]

        static member main(value: string) =
            element "main" [ Children [ Markup.text value ] ]

        static member aside(attributes: Attribute list) =
            element "aside" attributes

        static member aside(children: StaticView list) =
            element "aside" [ Children children ]

        static member aside(value: string) =
            element "aside" [ Children [ Markup.text value ] ]

        static member h1(attributes: Attribute list) =
            element "h1" attributes

        static member h1(children: StaticView list) =
            element "h1" [ Children children ]

        static member h1(value: string) =
            element "h1" [ Children [ Markup.text value ] ]

        static member footer(attributes: Attribute list) =
            element "footer" attributes

        static member footer(children: StaticView list) =
            element "footer" [ Children children ]

        static member footer(value: string) =
            element "footer" [ Children [ Markup.text value ] ]
