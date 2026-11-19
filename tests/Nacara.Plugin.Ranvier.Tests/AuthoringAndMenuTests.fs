/// <summary>Verifies rich authoring directives, recursive menu mapping, categorized navbars, and Ranvier brand styling.</summary>
module Nacara.Plugins.Ranvier.Tests.AuthoringAndMenuTests

open System.IO
open System.Text.Json
open Expecto
open Nacara.Core
open Nacara.Plugins
open Nacara.Plugins.Ranvier.Theme

[<Tests>]
let tests =
    testList "rich authoring directives, menus and brand theme" [
        testCase "cards, card, states, state and badge directives render clean Ranvier HTML without boundary markers" <| fun _ ->
            let directory = Directory.CreateTempSubdirectory("ranvier-authoring-directives-")
            try
                let markdown =
                    String.concat "\n" [
                        "---"
                        "title: Authoring Showcase"
                        "---"
                        ":::badge label=\"Pre-1.0\""
                        ":::"
                        ""
                        "::::states"
                        ":::state kind=\"pending\" label=\"Pending\" size=\"sm\""
                        ":::"
                        ":::state kind=\"recovered\" label=\"Recovered\" plain=\"true\""
                        ":::"
                        "::::"
                        ""
                        "::::cards"
                        ":::card title=\"Signals\" href=\"/core/guide/signals/\" badge=\"Updated\""
                        "Store reactive values and notify readers."
                        ":::"
                        ":::card title=\"Static note\""
                        "Card without link."
                        ":::"
                        "::::"
                    ]
                let contentDir = Path.Combine(directory.FullName, "content")
                Directory.CreateDirectory(contentDir) |> ignore
                File.WriteAllText(Path.Combine(contentDir, "index.md"), markdown)
                let theme = Ranvier.Docs.BrandTheme.apply "/" None Theme.defaults
                let collection =
                    Collection.create "guide" DocFrontMatter.decoder
                    |> Collection.sourceAll contentDir
                    |> Collection.title _.Title
                    |> Collection.layout (Theme.layout theme)
                let site =
                    Site.create "Authoring check"
                    |> Site.noStaticFiles
                    |> Site.output "output"
                    |> Markdown.register
                    |> Ranvier.Docs.CompatibilityDirectives.register
                    |> Theme.register theme
                    |> Site.collection collection
                let result = Build.run (AbsolutePath.create directory.FullName) site
                Expect.isTrue result.Succeeded (sprintf "Build diagnostics: %A" result.Diagnostics)
                Expect.isEmpty result.Diagnostics "Rich directives must render with zero diagnostics."
                let html = File.ReadAllText(Path.Combine(directory.FullName, "output", "index.html"))
                Expect.stringContains html "class=\"rv-badge\"" "Badge directive renders rv-badge."
                Expect.stringContains html "Pre-1.0" "Badge label renders."
                Expect.stringContains html "class=\"rv-states\"" "States container renders."
                Expect.stringContains html "rv-state rv-state--pending rv-state--sm" "Small pending state renders."
                Expect.stringContains html "rv-state rv-state--recovered rv-state--plain" "Plain recovered state renders."
                Expect.stringContains html "class=\"rv-cards\"" "Cards grid renders."
                Expect.stringContains html "href=\"/core/guide/signals/\"" "Linked card renders anchor href."
                Expect.stringContains html "class=\"rv-card__title\"" "Card title class renders."
                Expect.stringContains html "Updated" "Card badge renders."
                Expect.stringContains html "--rv-contour" "Shared Ranvier brand tokens are applied."
                Expect.stringContains html "fonts.googleapis.com" "Geist font links are included."
                Expect.isFalse (html.Contains(".rv-hero {")) "Non-splash pages must not inline splash hero CSS."
                Expect.isFalse (html.Contains("<!--[")) "Rendered fragments must not leak Ranvier SSR boundary markers."
            finally
                directory.Delete(true)

        testCase "SiteModel maps recursive sections, groups, links, badges and categorized navbar dropdowns" <| fun _ ->
            let json =
                """{
                  "sources": [
                    {
                      "id": "core",
                      "category": "Core & Reactive",
                      "title": "Ranvier",
                      "description": "Fine-grained reactive signals and memos.",
                      "route": "core",
                      "index": "index.md",
                      "sections": [
                        {
                          "title": "Guide",
                          "items": [
                            {
                              "kind": "section",
                              "title": "Fundamentals",
                              "items": [
                                { "kind": "page", "path": "guide/signals.fsx", "title": "Signals", "explicitTitle": true, "badge": "updated" }
                              ]
                            },
                            {
                              "kind": "group",
                              "path": "analyzers/index.md",
                              "title": "Analyzers",
                              "explicitTitle": false,
                              "badge": "new",
                              "items": [
                                { "kind": "page", "path": "analyzers/ran001.md", "title": "RAN001", "explicitTitle": false }
                              ]
                            },
                            {
                              "kind": "link",
                              "title": "Proofs",
                              "url": "https://ranvier-org.github.io/Ranvier/proofs/",
                              "badge": "experimental"
                            }
                          ]
                        }
                      ]
                    },
                    {
                      "id": "view",
                      "category": "Views & Styling",
                      "title": "Ranvier.View",
                      "description": "Typed templates, hydration and motion.",
                      "route": "view",
                      "index": "index.md",
                      "sections": []
                    }
                  ]
                }"""
            use doc = JsonDocument.Parse(json)
            let sources = doc.RootElement.GetProperty("sources") |> fun el -> [ for item in el.EnumerateArray() -> item ]
            let menu = Ranvier.Docs.SiteModel.buildSourceMenu sources.Head
            Expect.equal menu.Length 1 "One top-level section."
            match menu.Head.Entry with
            | MenuSection("Guide", [ subSection; groupItem; linkItem ]) ->
                match subSection.Entry with
                | MenuSection("Fundamentals", [ pageItem ]) ->
                    Expect.equal pageItem.Entry (MenuPage "core:guide/signals.fsx") "Nested page uses source-prefixed id."
                    Expect.equal pageItem.Badge (Some { Label = "Updated"; Kind = "updated" }) "Badge kind and capitalized label preserved."
                | other -> failtestf "Expected nested MenuSection, got %A" other
                match groupItem.Entry with
                | MenuGroup("core:analyzers/index.md", [ child ]) ->
                    Expect.equal child.Entry (MenuPage "core:analyzers/ran001.md") "Group child mapped."
                    Expect.equal groupItem.Badge (Some { Label = "New"; Kind = "new" }) "Group badge mapped."
                | other -> failtestf "Expected MenuGroup, got %A" other
                Expect.equal linkItem.Entry (MenuLink("Proofs", "https://ranvier-org.github.io/Ranvier/proofs/")) "External MenuLink mapped."
                Expect.equal linkItem.Badge (Some { Label = "Experimental"; Kind = "experimental" }) "Link badge mapped."
            | other -> failtestf "Unexpected menu shape: %A" other

            let navbar = Ranvier.Docs.SiteModel.buildNavbar true sources
            Expect.equal navbar.Length 3 "Two category dropdowns plus API Reference."
            match navbar.Head with
            | NavbarDropdown("Core & Reactive", [ NavbarDescribed("Ranvier", "Fine-grained reactive signals and memos.", "/core/") ]) -> ()
            | other -> failtestf "Expected categorized NavbarDropdown, got %A" other

        testCase "resolveCollectionDocPage honors toc: false, preserves layout: bare, and suppresses duplicate H1s" <| fun _ ->
            let overview : DocFrontMatter =
                { Title = "Ranvier.Keyboard"
                  Description = Some "Keyboard primitives"
                  Order = None
                  Layout = None
                  PageNav = None
                  MenuFilter = None
                  MenuMemory = None
                  Toc = Some TocOff
                  Main = [] }
            let resolvedOverview =
                Ranvier.Docs.SiteModel.resolveCollectionDocPage
                    overview
                    "Ranvier.Keyboard"
                    "<p>Lead copy.</p>"
            Expect.isTrue resolvedOverview.Styled "Collection overview with toc: false is styled (.nacara-content)."
            Expect.isTrue resolvedOverview.ShowMenu "Collection overview retains its sidebar menu."
            Expect.isFalse resolvedOverview.ShowToc "toc: false omits right-hand TOC."
            Expect.isTrue resolvedOverview.ShowTitle "Page without body <h1> renders shell <h1>."

            let resolvedBodyH1 =
                Ranvier.Docs.SiteModel.resolveCollectionDocPage
                    { overview with Toc = None }
                    "Ranvier.BAREWire"
                    "<h1 id=\"ranvier-barewire\">Ranvier.BAREWire</h1><p>Lead copy.</p>"
            Expect.isTrue resolvedBodyH1.Styled "Standard page is styled."
            Expect.isFalse resolvedBodyH1.ShowTitle "Body starting with <h1> suppresses duplicate shell <h1>."

            let resolvedBare =
                Ranvier.Docs.SiteModel.resolveCollectionDocPage
                    { overview with Layout = Some "bare"; Toc = None }
                    "Canvas"
                    "<section>Custom canvas</section>"
            Expect.isFalse resolvedBare.Styled "Explicit layout: bare preserves unstyled canvas semantics."
            Expect.isFalse resolvedBare.ShowMenu "Explicit layout: bare hides sidebar."
    ]
