/// <summary>Maps site.json repository metadata onto recursive Nacara menus, categorized navbars, and the ecosystem landing page.</summary>
module internal Ranvier.Docs.SiteModel

open System
open System.IO
open System.Text.Json
open System.Text.RegularExpressions
open Nacara.Core
open Nacara.Plugins.Ranvier.Theme

let private leadingH1Pattern =
    Regex("^(?:\\s*<!--[\\s\\S]*?-->)*\\s*<h1[\\s>]", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

let stringValue (name: string) (element: JsonElement) =
    match element.TryGetProperty name with
    | true, value when value.ValueKind = JsonValueKind.String -> value.GetString()
    | _ -> ""

let boolValue (name: string) (element: JsonElement) =
    match element.TryGetProperty name with
    | true, value when value.ValueKind = JsonValueKind.True -> true
    | _ -> false

let arrayValue (name: string) (element: JsonElement) =
    match element.TryGetProperty name with
    | true, value when value.ValueKind = JsonValueKind.Array -> value.EnumerateArray() |> Seq.toList
    | _ -> []

let pageUrl (route: string) (path: string) =
    let page = Path.ChangeExtension(path, null).Replace('\\', '/').ToLowerInvariant()
    let suffix =
        if page = "index" then ""
        elif page.EndsWith("/index", StringComparison.Ordinal) then page[..page.Length - 6]
        else page
    "/" + String.concat "/" ([ route.Trim('/'); suffix.Trim('/') ] |> List.filter (String.IsNullOrEmpty >> not)) + "/"

let private badgeLabel (kind: string) =
    if String.IsNullOrEmpty kind then ""
    else string (Char.ToUpperInvariant kind[0]) + kind.Substring(1)

let private applyBadge (element: JsonElement) (item: MenuItem) =
    match stringValue "badge" element with
    | "" -> item
    | kind -> Menu.badgeOf kind (badgeLabel kind) item

let rec buildMenuItems (sourceId: string) (items: JsonElement list) : MenuItem list =
    [ for item in items do
          match stringValue "kind" item with
          | "section" ->
              yield Menu.section (stringValue "title" item) (buildMenuItems sourceId (arrayValue "items" item))
          | "group" ->
              let children = buildMenuItems sourceId (arrayValue "items" item)
              yield
                  Menu.group (sourceId + ":" + stringValue "path" item) children
                  |> applyBadge item
          | "link" ->
              yield
                  Menu.link (stringValue "title" item) (stringValue "url" item)
                  |> applyBadge item
          | _ ->
              let path = stringValue "path" item
              if not (String.IsNullOrEmpty path) then
                  yield
                      Menu.page (sourceId + ":" + path)
                      |> applyBadge item ]

let rec private containsUrl (targetUrl: string) (items: MenuItem list) =
    items
    |> List.exists (fun item ->
        match item.Entry with
        | MenuLink(_, url) -> String.Equals(url, targetUrl, StringComparison.OrdinalIgnoreCase)
        | MenuSection(_, children)
        | MenuGroup(_, children) -> containsUrl targetUrl children
        | MenuPage _ -> false)

let buildSourceMenu (source: JsonElement) : MenuItem list =
    let sourceId = stringValue "id" source
    let sections =
        [ for section in arrayValue "sections" source do
              let rawItems =
                  match arrayValue "items" section with
                  | [] -> arrayValue "pages" section
                  | items -> items
              yield Menu.section (stringValue "title" section) (buildMenuItems sourceId rawItems) ]

    let proofsUrl = stringValue "proofsUrl" source
    if not (String.IsNullOrEmpty proofsUrl) && not (containsUrl proofsUrl sections) then
        sections @ [ Menu.section "Formal Verification" [ Menu.link "Rocq Proofs" proofsUrl ] ]
    else
        sections

let rec private tryFindExplicitTitle (relativePath: string) (items: JsonElement list) : string option =
    items
    |> List.tryPick (fun item ->
        match stringValue "kind" item with
        | "section" -> tryFindExplicitTitle relativePath (arrayValue "items" item)
        | "group" ->
            if stringValue "path" item = relativePath && boolValue "explicitTitle" item then
                Some(stringValue "title" item)
            else
                tryFindExplicitTitle relativePath (arrayValue "items" item)
        | "link" -> None
        | _ ->
            let hasExplicit =
                match item.TryGetProperty "explicitTitle" with
                | true, v -> v.ValueKind = JsonValueKind.True
                | false, _ -> not (String.IsNullOrEmpty(stringValue "title" item))
            if stringValue "path" item = relativePath && hasExplicit then
                Some(stringValue "title" item)
            else
                None)

let explicitDisplayTitle (sources: JsonElement list) (page: Page) : Page =
    sources
    |> List.tryFind (fun source -> stringValue "id" source = page.Collection)
    |> Option.bind (fun source ->
        page.SourceFile
        |> Option.bind (fun file ->
            let relative = Path.GetRelativePath(stringValue "contentRoot" source, AbsolutePath.value file).Replace('\\', '/')
            arrayValue "sections" source
            |> List.tryPick (fun section ->
                let items =
                    match arrayValue "items" section with
                    | [] -> arrayValue "pages" section
                    | xs -> xs
                tryFindExplicitTitle relative items)
            |> Option.map (fun title -> { page with Title = title })))
    |> Option.defaultValue page

let resolveCollectionDocPage (frontMatter: DocFrontMatter) (resolvedTitle: string) (renderedHtml: string) : DocPage =
    let normalizedFrontMatter, hideOverviewToc =
        match frontMatter.Layout with
        | Some "bare" -> { frontMatter with Title = resolvedTitle; Layout = None }, true
        | _ -> { frontMatter with Title = resolvedTitle }, false

    let baseDoc =
        DocFrontMatter.toDocPage normalizedFrontMatter
        |> fun doc -> if hideOverviewToc then DocPage.withoutToc doc else doc

    if baseDoc.ShowTitle && not (String.IsNullOrEmpty renderedHtml) && leadingH1Pattern.IsMatch renderedHtml then
        { baseDoc with ShowTitle = false }
    else
        baseDoc

let buildNavbar (hasApiReference: bool) (sources: JsonElement list) : NavbarItem list =
    let hasCategories = sources |> List.exists (fun s -> not (String.IsNullOrEmpty(stringValue "category" s)))
    let packageNav =
        if hasCategories || sources.Length > 6 then
            let categories = ResizeArray<string * ResizeArray<JsonElement>>()
            for source in sources do
                let category =
                    match stringValue "category" source with
                    | "" -> "Packages"
                    | c -> c
                match categories |> Seq.tryFind (fun (name, _) -> name = category) with
                | Some(_, bucket) -> bucket.Add source
                | None ->
                    let bucket = ResizeArray<JsonElement>()
                    bucket.Add source
                    categories.Add(category, bucket)

            [ for category, bucket in categories ->
                  NavbarDropdown(
                      category,
                      [ for source in bucket ->
                            let title = stringValue "title" source
                            let desc = stringValue "description" source
                            let url = pageUrl (stringValue "route" source) (stringValue "index" source)
                            if String.IsNullOrEmpty desc then
                                NavbarSection(title, stringValue "route" source, url)
                            else
                                NavbarDescribed(title, desc, url) ]
                  ) ]
        else
            [ for source in sources ->
                  NavbarSection(
                      stringValue "title" source,
                      stringValue "route" source,
                      pageUrl (stringValue "route" source) (stringValue "index" source)
                  ) ]

    if hasApiReference then
        packageNav @ [ NavbarSection("API Reference", "reference", "/reference/") ]
    else
        packageNav

let renderLandingMarkdown (siteTitle: string) (baseUrl: string) (hasApiReference: bool) (_landingDir: string) (sources: JsonElement list) : string list =
    let sourceHref source =
        baseUrl.TrimEnd('/') + pageUrl (stringValue "route" source) (stringValue "index" source)

    let primaryHref =
        sources
        |> List.tryFind (fun s -> stringValue "id" s = "core")
        |> Option.orElse (List.tryHead sources)
        |> Option.map sourceHref
        |> Option.defaultValue (baseUrl.TrimEnd('/') + "/core/")

    let categories = ResizeArray<string * ResizeArray<JsonElement>>()
    for source in sources do
        let cat =
            match stringValue "category" source with
            | "" -> "Libraries & Guides"
            | c -> c
        match categories |> Seq.tryFind (fun (name, _) -> name = cat) with
        | Some(_, bucket) -> bucket.Add source
        | None ->
            let bucket = ResizeArray<JsonElement>()
            bucket.Add source
            categories.Add(cat, bucket)

    [ yield "---"
      yield "title: " + JsonSerializer.Serialize(siteTitle)
      yield "description: Unified documentation, runnable examples, interactive signal maps and API reference for the Ranvier ecosystem."
      yield "layout: splash"
      yield "---"
      yield ""
      yield "<section class=\"rv-hero\">"
      yield "<div class=\"rv-hero__copy\">"
      yield "<span class=\"rv-hero__status\"><span class=\"rv-badge\">Pre-1.0</span> Unified Ranvier ecosystem documentation.</span>"
      yield "<h1 class=\"rv-hero__title\">ranvier</h1>"
      yield "<p class=\"rv-hero__line\">Fine-grained reactive computation, views, styling and native UIs for .NET &amp; Fable.</p>"
      yield "<p class=\"rv-hero__sub\">Store state in signals, derive values with memos, handle async pending and errors at boundaries, render hydrated templates, and inspect live dependency graphs across every package in one place.</p>"
      yield "<div class=\"rv-hero__actions\">"
      yield $"<a class=\"rv-btn rv-btn--primary\" href=\"{primaryHref}\">Start with Core</a>"
      if hasApiReference then
          let refHref = baseUrl.TrimEnd('/') + "/reference/"
          yield $"<a class=\"rv-btn rv-btn--secondary\" href=\"{refHref}\">API Reference</a>"
      yield "</div>"
      yield "</div>"
      yield "<figure class=\"rv-demo\" aria-label=\"Reactive signals, memos and boundaries\">"
      yield "<div class=\"rv-demo__code\">"
      yield "<pre><code><span class=\"k\">use</span> graph = <span class=\"k\">new</span> <span class=\"t\">Graph</span> ()"
      yield "<span class=\"k\">use</span> _ = graph.Activate ()"
      yield ""
      yield "<span class=\"k\">let</span> count = createSignal <span class=\"n\">2</span>"
      yield "<span class=\"k\">let</span> doubled = createMemo (<span class=\"k\">fun</span> _ -&gt; count.Value * <span class=\"n\">2</span>)"
      yield "<span class=\"c\">// Views, CSS, Persistence, Terminal &amp; WebCore share one graph</span></code></pre>"
      yield "</div>"
      yield "<div class=\"rv-demo__out\">"
      yield "<div class=\"rv-demo__label\">Boundary states across the stack</div>"
      yield "<div class=\"rv-states\">"
      yield "<span class=\"rv-state rv-state--ready rv-state--sm\">Ready</span>"
      yield "<span class=\"rv-state rv-state--pending rv-state--sm\">Pending</span>"
      yield "<span class=\"rv-state rv-state--fallback rv-state--sm\">Fallback</span>"
      yield "<span class=\"rv-state rv-state--recovered rv-state--sm\">Recovered</span>"
      yield "</div>"
      yield "</div>"
      yield "</figure>"
      yield "</section>"
      yield ""
      for category, bucket in categories do
          yield $"## {category}"
          yield ""
          yield "::::cards"
          for source in bucket do
              let title = (stringValue "title" source).Replace("\"", "\\\"")
              let href = sourceHref source
              let badge = stringValue "badge" source
              let badgeAttr = if String.IsNullOrEmpty badge then "" else $" badge=\"{badgeLabel badge}\""
              let repoName = stringValue "repository" source
              let desc =
                  match stringValue "description" source with
                  | "" -> $"Documentation and guides for {repoName}."
                  | d -> d
              yield $":::card title=\"{title}\" href=\"{href}\"{badgeAttr}"
              yield ""
              yield desc
              yield ""
              yield ":::"
          yield "::::"
          yield "" ]
