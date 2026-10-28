namespace Nacara.Plugins.Ranvier.Theme

open Nacara.Core

/// <summary>Renders escaped code tokens and line markers through static Ranvier views.</summary>
type NacaraCodeBlockRenderer() =
    let element = Markup.view

    let copyButton =
        let icon className paths =
            element
                "svg"
                [
                    "class", className
                    "viewBox", "0 0 24 24"
                    "fill", "none"
                    "stroke", "currentColor"
                    "stroke-width", "2"
                    "stroke-linecap", "round"
                    "stroke-linejoin", "round"
                    "aria-hidden", "true"
                ]
                paths

        element
            "button"
            [ "class", "nacara-code__copy"; "type", "button"; "aria-label", "Copy code" ]
            [
                icon
                    "nacara-code__copy-idle"
                    [
                        element "rect" [ "x", "9"; "y", "9"; "width", "11"; "height", "11"; "rx", "2" ] []
                        element "path" [ "d", "M5 15V5a2 2 0 0 1 2-2h10" ] []
                    ]
                icon "nacara-code__copy-done" [ element "path" [ "d", "M20 6 9 17l-5-5" ] [] ]
            ]

    let renderLine (meta: CodeBlockMeta) (line: CodeLine) =
        let runs =
            line.Pieces
            |> List.fold
                (fun groups piece ->
                    match groups with
                    | (marker, pieces) :: rest when marker = piece.Marker -> (marker, piece :: pieces) :: rest
                    | _ -> (piece.Marker, [ piece ]) :: groups)
                []
            |> List.rev

        let pieces =
            [
                for marker, pieces in runs do
                    let children =
                        [
                            for piece in List.rev pieces ->
                                match piece.ClassName with
                                | None -> Markup.text piece.Text
                                | Some className -> element "span" [ "class", className ] [ Markup.text piece.Text ]
                        ]

                    match marker with
                    | None -> yield! children
                    | Some marker -> yield element "mark" [ "class", "nacara-code__word"; "data-marker", marker.Name ] children
                yield Markup.text "\n"
            ]

        element
            "span"
            [
                yield "class", "nacara-code__line"
                match line.Marker with
                | Some marker -> yield "data-marker", marker.Name
                | None -> ()
                if meta.ShowLineNumbers then
                    yield "data-line", string (line.Number + meta.StartLineNumber - 1)
            ]
            pieces

    interface ICodeBlockRenderer with
        member _.Name = "theme.ranvier"

        member _.Render(block: PreparedCodeBlock) =
            let frame =
                match block.Meta.Frame with
                | CodeFrame -> "code"
                | TerminalFrame -> "terminal"
                | NoFrame -> "none"

            let runs =
                block.Lines
                |> List.fold
                    (fun groups line ->
                        match groups with
                        | (collapsed, lines) :: rest when collapsed = line.IsCollapsed -> (collapsed, line :: lines) :: rest
                        | _ -> (line.IsCollapsed, [ line ]) :: groups)
                    []
                |> List.rev

            let code lines =
                element "pre" [] [ element "code" [] (lines |> List.map (renderLine block.Meta)) ]

            element
                "figure"
                [
                    yield "class", "nacara-code"
                    yield "data-language", block.Language |> Option.defaultValue "text"
                    yield "data-frame", frame
                    if block.Meta.Wrap then
                        yield "data-wrap", "true"
                    if block.Meta.ShowLineNumbers then
                        yield "data-line-numbers", "true"
                    match block.Meta.MaxHeight with
                    | Some height -> yield "style", "--nacara-code-max-height:" + height
                    | None -> ()
                    if not block.Meta.Unknown.IsEmpty then
                        yield "data-meta", String.concat " " block.Meta.Unknown
                    yield "data-source", CodeBlock.source block
                ]
                [
                    match block.Meta.Title, frame with
                    | Some title, _ -> yield element "figcaption" [ "class", "nacara-code__title" ] [ Markup.text title ]
                    | None, "terminal" -> yield element "figcaption" [ "class", "nacara-code__title" ] [ Markup.text "Terminal" ]
                    | _ -> ()
                    yield
                        element
                            "div"
                            [ "class", "nacara-code__body" ]
                            [
                                if runs.IsEmpty then
                                    yield code []
                                for collapsed, lines in runs do
                                    let lines = List.rev lines

                                    if collapsed then
                                        yield
                                            element
                                                "details"
                                                [ "class", "nacara-code__collapsed" ]
                                                [
                                                    element "summary" [] [ Markup.text $"{lines.Length} collapsed lines" ]
                                                    code lines
                                                ]
                                    else
                                        yield code lines
                                yield copyButton
                            ]
                ]
            |> Markup.render
