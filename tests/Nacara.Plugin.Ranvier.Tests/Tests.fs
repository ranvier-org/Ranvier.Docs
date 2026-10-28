module Nacara.Plugins.Ranvier.Tests.Theme

open System
open System.IO
open Expecto
open Nacara.Core
open Nacara.Plugins
open Nacara.Plugins.Ranvier.Theme

[<Tests>]
let tests =
    testList
        "Ranvier theme"
        [
            testCase "Ranvier escapes text and attributes exactly once"
            <| fun _ ->
                let html =
                    Markup.view "p" [ "title", "<&\"" ] [ Markup.text "<hello> & goodbye" ]
                    |> Markup.render

                Expect.equal html "<p title=\"&lt;&amp;&quot;\">&lt;hello&gt; &amp; goodbye</p>" "Escaping belongs to the renderer."

                Expect.equal
                    (html
                     |> Feliz.ViewEngine.Html.rawText
                     |> Feliz.ViewEngine.Render.htmlView)
                    html
                    "The Nacara adapter preserves rendered markup."

            testCase "layout variants keep their content contract"
            <| fun _ ->
                let page = DocPage.create "Example"
                let bare = DocPage.bare page
                Expect.isFalse bare.Styled "Bare pages own their content layout."
                Expect.isFalse bare.ShowMenu "Bare pages have no sidebar."
                Expect.isFalse bare.ShowTitle "Bare pages own their title."
                let splash = DocPage.splash page
                Expect.isTrue splash.Styled "Splash pages retain prose styling."
                Expect.isFalse splash.ShowToc "Splash pages have no TOC."

            testCase "raw-text serialization preserves code and contains closing delimiters"
            <| fun _ ->
                let source = "if (1 < 2 && 2 > 1) { console.log('</ScRiPt>'); }"

                let html =
                    Markup.script [ "data-label", "<&\"" ] source
                    |> Markup.render

                Expect.stringContains html "1 < 2 && 2 > 1" "JavaScript operators remain executable source."
                Expect.stringContains html "<\\/ScRiPt>" "A string cannot terminate its containing script and retains its case."
                Expect.stringContains html "data-label=\"&lt;&amp;&quot;\"" "Script attributes are escaped."
                Expect.isFalse (html.Contains "data-ranvier-raw-text") "Scripts contain no serialization placeholder."

                let external = Markup.script [ "src", "/app.js?x=1&y=\"2\"" ] "" |> Markup.render
                Expect.equal external "<script src=\"/app.js?x=1&amp;y=&quot;2&quot;\"></script>" "External script attributes are escaped."

                let style =
                    Markup.style "a > b { content: '</StYlE>'; }"
                    |> Markup.render

                Expect.stringContains style "a > b" "CSS selectors retain their syntax."
                Expect.stringContains style "\\3c /StYlE>" "Mixed-case closing style delimiters remain CSS text."
                Expect.isFalse (style.Contains "data-ranvier-raw-text") "Serialization placeholders are internal."

            testCase "standalone Nacara builds emit complete HTML and fingerprinted assets"
            <| fun _ ->
                let root =
                    Path.Combine (Path.GetTempPath (), "ranvier-theme-" + Guid.NewGuid().ToString("N"))

                Directory.CreateDirectory (Path.Combine (root, "content", "guide"))
                |> ignore

                try
                    let write name (text: string) =
                        File.WriteAllText (Path.Combine (root, "content", name), text)

                    write "index.md" "---\ntitle: Home\nlayout: bare\n---\n<main class=\"landing\">Complete before JavaScript</main>\n"
                    write "guide/index.md" "---\ntitle: Guide\n---\nOverview.\n"

                    write
                        "guide/first.md"
                        "---\ntitle: First & More\n---\n## A heading\n\n<em data-trusted=\"yes\">Trusted Markdown HTML</em>\n\n```fsharp\nlet x = \"<unsafe>\"\n```\n"

                    write "guide/second.md" "---\ntitle: Second\nlayout: splash\n---\nSecond page.\n"

                    let theme =
                        Theme.defaults
                        |> Theme.menu
                            "guide"
                            [
                                Menu.group "guide/index.md" [ Menu.page "guide/first.md" |> Menu.badge "New"; Menu.page "guide/second.md" ]
                            ]
                        |> Theme.menuLimit 1
                        |> Theme.menuGroupLimit 1
                        |> Theme.moduleScripts [ "assets/live-example/live-example.js" ]
                        |> Theme.navbar [ NavbarSection ("Guide", "guide", "/guide/") ]
                        |> Theme.editUrl "https://example.com/edit/main/docs"
                        |> Theme.footer (Markup.view "p" [] [ Markup.text "Rendered by Ranvier" ])

                    let site =
                        Site.create "Test & Site"
                        |> Site.baseUrl "/manual/"
                        |> Site.origin "https://example.com"
                        |> Site.noStaticFiles
                        |> Site.output "output"
                        |> Markdown.register
                        |> Theme.register theme
                        |> Site.collection (
                            Theme.docs theme "content"
                            |> Collection.layout (fun context ->
                                let site =
                                    { context.Site with
                                        PageAssets =
                                            [ Script("assets/live-example/live-example.js", true)
                                              Script("assets/classic.js", false)
                                              Script("assets/deferred-classic.js", true) ] }
                                Theme.layout theme { context with Site = site })
                        )

                    let result = Build.run (AbsolutePath.create root) site
                    Expect.isTrue result.Succeeded (sprintf "Build diagnostics: %A" result.Diagnostics)
                    let output = Path.Combine (root, "output")
                    let html = File.ReadAllText (Path.Combine (output, "guide", "first", "index.html"))

                    for expected in
                        [
                            "<!DOCTYPE html>"
                            "First &amp; More"
                            "data-trusted=\"yes\""
                            "nacara-sidebar"
                            "nacara-toc"
                            "nacara-page-nav"
                            "nacara-code__copy"
                            "/manual/assets/css/ranvier."
                            "https://example.com/manual/guide/first/"
                            "Rendered by Ranvier"
                            "data-kind=\"new\""
                        ] do
                        Expect.stringContains html expected "The static document carries content, navigation, metadata and controls."

                    Expect.isFalse (html.Contains "&amp;lt;unsafe") "Source code must not be double escaped."
                    Expect.isFalse (html.Contains "ranvier.js") "No client Ranvier runtime is needed."
                    let scriptAttributes path =
                        let pattern = "<script([^>]*src=\"/manual/" + System.Text.RegularExpressions.Regex.Escape(path) + "\"[^>]*)>"
                        let matched = System.Text.RegularExpressions.Regex.Match(html, pattern)
                        Expect.isTrue matched.Success "The registered script must be present."
                        matched.Groups[1].Value
                    let moduleAttributes = scriptAttributes "assets/live-example/live-example.js"
                    Expect.stringContains moduleAttributes "type=\"module\"" "Explicit module assets execute as ESM."
                    Expect.stringContains moduleAttributes "defer=\"defer\"" "The Core defer flag retains its meaning."
                    Expect.isFalse ((scriptAttributes "assets/classic.js").Contains "type=\"module\"") "Classic scripts remain classic."
                    Expect.isFalse ((scriptAttributes "assets/deferred-classic.js").Contains "type=\"module\"") "Deferred classic scripts remain classic."
                    Expect.stringContains (scriptAttributes "assets/deferred-classic.js") "defer=\"defer\"" "Classic deferred scripts retain defer."
                    let landing = File.ReadAllText (Path.Combine (output, "index.html"))
                    Expect.stringContains landing "nacara-layout--bare" "Bare layout is selected by front matter."

                    let splash =
                        File.ReadAllText (Path.Combine (output, "guide", "second", "index.html"))

                    Expect.stringContains splash "nacara-layout--splash" "Splash layout is selected by front matter."

                    let css =
                        Directory.GetFiles (Path.Combine (output, "assets", "css"), "ranvier.*.css")

                    Expect.equal css.Length 1 "One content-addressed stylesheet is emitted."
                    Expect.stringContains (File.ReadAllText css[0]) "--nacara-bg" "The asset contains the rendered Ranvier.Css cascade."
                    let scripts = Directory.GetFiles (Path.Combine (output, "assets"), "nacara.*.js")
                    Expect.equal scripts.Length 1 "Progressive behavior is embedded and emitted."
                    Expect.stringContains (File.ReadAllText scripts[0]) "customElements.define" "Widgets are available without a Node build."
                    Expect.isTrue (File.Exists (Path.Combine (output, "404.html"))) "The default missing-page document is generated."
                    Expect.isTrue (Directory.Exists (Path.Combine (output, "assets", "menu"))) "Pruned menus retain their complete searchable source."
                finally
                    if root.StartsWith (Path.Combine (Path.GetTempPath (), "ranvier-theme-"), StringComparison.OrdinalIgnoreCase) then
                        Directory.Delete (root, true)
        ]
