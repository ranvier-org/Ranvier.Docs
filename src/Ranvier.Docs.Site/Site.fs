module private Ranvier.Docs.Site

open System
open System.IO
open System.Text.Json
open System.Text.RegularExpressions
open Nacara.Core
open Nacara.Plugins
open Nacara.Plugins.Ranvier.Theme
open Ranvier.Docs.SiteModel

let private relativeHtmlAssetAttr = Regex("\\b(href|src)\\s*=\\s*\"([^\"]*)\"", RegexOptions.Compiled)
let private schemePrefix = Regex("^[a-zA-Z][a-zA-Z0-9+.-]*:", RegexOptions.Compiled)

let private csharpGrammar (options: TreeSitterOptions) =
    let grammar = TreeSitter.bundled "c_sharp"
    options
    |> TreeSitter.grammars (TreeSitter.aliases [ "csharp"; "cs"; "c#" ] grammar :: options.Grammars)

let private specBindingAssemblies =
    System.Collections.Generic.HashSet<string>(
        [ "Ranvier.Css.Bindings"; "Ranvier.Css.View"; "Ranvier.View.Dom" ],
        StringComparer.OrdinalIgnoreCase
    )

let private resolveApiSources (root: string) (sources: JsonElement list) : FSharpApiSource list * string list =
    let sharedBin = Path.Combine(root, ".generated", "bin", "Release", "net10.0")
    let apiBin = Path.Combine(root, ".generated", "api-reference", "bin", "Release", "net10.0")
    let mutable excludedNamespaces = [ "Ranvier.Css.Spec"; "Ranvier.Css.Properties" ]
    let seen = System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
    let resolved = ResizeArray<FSharpApiSource>()

    for source in sources do
        match source.TryGetProperty "apiReference" with
        | true, api when api.ValueKind = JsonValueKind.Object ->
            let projectDirs =
                [ for proj in arrayValue "projects" api -> Path.GetDirectoryName(proj.GetString()) ]
            for ns in arrayValue "exclude" api do
                excludedNamespaces <- ns.GetString() :: excludedNamespaces
            for asmEl in arrayValue "assemblies" api do
                let asmName = asmEl.GetString()
                if not (specBindingAssemblies.Contains asmName) && seen.Add asmName then
                    let fromProject =
                        projectDirs
                        |> List.map (fun dir -> Path.Combine(dir, "bin", "Release", "net10.0", $"{asmName}.dll"))
                        |> List.tryFind (fun dll -> File.Exists dll && File.Exists(Path.ChangeExtension(dll, ".xml")))
                    let fromApiBin = Path.Combine(apiBin, $"{asmName}.dll")
                    let fallback = Path.Combine(sharedBin, $"{asmName}.dll")
                    let chosen =
                        match fromProject with
                        | Some dll -> Some dll
                        | None when File.Exists fromApiBin -> Some fromApiBin
                        | None when File.Exists fallback -> Some fallback
                        | None -> None
                    match chosen with
                    | Some dll ->
                        let searchDirs =
                            [ if Directory.Exists apiBin then yield apiBin
                              if Directory.Exists sharedBin then yield sharedBin
                              yield Path.GetDirectoryName dll ]
                            |> List.distinct
                        resolved.Add(
                            FSharpApiSource.create dll
                            |> FSharpApiSource.searchPaths searchDirs
                        )
                    | None -> ()
        | _ -> ()

    Seq.toList resolved, List.distinct excludedNamespaces

let private configureOgImages (sources: JsonElement list) (options: OgImageOptions) =
    let resolveCard (page: Page) =
        sources
        |> List.tryFind (fun source -> stringValue "id" source = page.Collection)
        |> Option.bind (fun source ->
            let routePrefix = "/" + (stringValue "route" source).Trim('/') + "/"
            let segments =
                page.Id.Split([| '/'; '\\'; ':' |])
                |> Array.map (fun s -> Path.ChangeExtension(s, null))
            let matched =
                arrayValue "ogCards" source
                |> List.tryFind (fun card -> Array.contains (stringValue "segment" card) segments)
                |> Option.map (fun card ->
                    let img = (stringValue "image" card).TrimStart('/')
                    OgImage.image (routePrefix + img)
                    |> OgImage.withAlt (stringValue "alt" card)
                    |> OgImage.withSize 1200 630)
            match matched with
            | Some card -> Some card
            | None ->
                match source.TryGetProperty "defaultOgImage" with
                | true, def when def.ValueKind = JsonValueKind.Object ->
                    let img = (stringValue "image" def).TrimStart('/')
                    Some(
                        OgImage.image (routePrefix + img)
                        |> OgImage.withAlt (stringValue "alt" def)
                        |> OgImage.withSize 1200 630
                    )
                | _ -> None)

    options |> OgImage.fallback resolveCard

let private configure root (metadata: JsonElement) =
    let sources = arrayValue "sources" metadata
    let baseUrl = stringValue "baseUrl" metadata
    let siteTitle =
        match stringValue "title" metadata with
        | "" -> "Ranvier"
        | title -> title

    let apiSources, apiExclude = resolveApiSources root sources
    let hasApiReference = not apiSources.IsEmpty
    let apiOptions =
        { FSharpApi.defaults with
            Root = "reference"
            Title = "API reference"
            Sources = apiSources
            Exclude = apiExclude }

    let displayTitle = explicitDisplayTitle sources

    let baseTheme =
        Theme.defaults
        |> Theme.navbar (buildNavbar hasApiReference sources)
        |> Theme.navbarEnd
            [ NavbarDynamicWidget Search.trigger
              NavbarIcon("GitHub", "https://github.com/ranvier-org", Icons.github) ]
        |> fun options ->
            (options, sources)
            ||> List.fold (fun options source ->
                options |> Theme.menu (stringValue "route" source) (buildSourceMenu source))

    let sharedBrandTheme = BrandTheme.apply baseUrl None baseTheme
    let landingTheme = sharedBrandTheme |> Theme.css BrandTheme.splashLandingCss

    let collection source =
        let route = stringValue "route" source
        let assetBase = baseUrl.TrimEnd('/') + "/" + route.Trim('/') + "/"
        let hasOwnBrandStates =
            File.Exists(Path.Combine(root, ".generated", "static", route, "brand", "states", "fallback-light.svg"))
        let stateBase = if hasOwnBrandStates then Some assetBase else None

        let extraStyles =
            [ for styleEl in arrayValue "styles" source do
                  let styleFile = styleEl.GetString()
                  if File.Exists styleFile then
                      yield
                          File.ReadAllText(styleFile)
                              .Replace("__BASE__", assetBase)
                              .Replace("__SITE_BASE__", baseUrl) ]

        let extraScripts =
            [ for scriptEl in arrayValue "scripts" source do
                  let src = assetBase + (stringValue "src" scriptEl).TrimStart('/')
                  let attrs =
                      [ yield "src", src
                        if boolValue "defer" scriptEl then yield "defer", "defer"
                        if boolValue "module" scriptEl then yield "type", "module" ]
                  yield Markup.script attrs "" ]

        let sourceTheme =
            baseTheme
            |> Theme.editUrl ((stringValue "editUrl" source).TrimEnd('/'))
            |> BrandTheme.apply baseUrl stateBase
            |> fun opts -> (opts, extraStyles) ||> List.fold (fun acc css -> acc |> Theme.css css)
            |> fun opts ->
                if extraScripts.IsEmpty then opts
                else { opts with HeadExtra = opts.HeadExtra @ extraScripts }

        Collection.create (stringValue "id" source) DocFrontMatter.decoder
        |> Collection.sourceAll (stringValue "contentRoot" source)
        |> Collection.routePrefix route
        |> Collection.title _.Title
        |> Collection.order (fun frontMatter -> defaultArg frontMatter.Order 0)
        |> Collection.toc (fun frontMatter ->
            match frontMatter.Toc with
            | Some(TocLevels range) -> Some range
            | Some TocOff
            | None -> None)
        |> Collection.layout (fun context ->
            let sourcePath =
                context.Page.SourceFile
                |> Option.map (
                    AbsolutePath.value
                    >> fun file ->
                        Path.GetRelativePath(stringValue "contentRoot" source, file).Replace('\\', '/')
                        |> RelativePath.create
                )
            let updatedPage = { displayTitle context.Page with ProjectPath = sourcePath }
            let doc = resolveCollectionDocPage context.FrontMatter updatedPage.Title context.Content
            Theme.shell
                sourceTheme
                doc
                { context with
                    Page = updatedPage
                    Pages = List.map displayTitle context.Pages }
            |> Markup.toLayout)

    let landing = Path.Combine(root, ".generated", "landing")
    Directory.CreateDirectory landing |> ignore
    let lines = renderLandingMarkdown siteTitle baseUrl hasApiReference landing sources
    File.WriteAllLines(Path.Combine(landing, "index.md"), lines)

    let livePreviewCssPath = Path.Combine(root, ".generated", "live", "brand-preview.css")
    Directory.CreateDirectory(Path.GetDirectoryName livePreviewCssPath) |> ignore
    File.WriteAllText(livePreviewCssPath, BrandTheme.livePreviewCss)

    let examplesProject = Path.Combine(root, ".generated", "Examples.fsproj")
    let contentAssets =
        { new IPlugin with
            member _.Name = "repository-content-assets"
            member _.Configure registry =
                let withAssets =
                    (registry, sources)
                    ||> List.fold (fun registry source ->
                        let contentRoot = stringValue "contentRoot" source
                        (registry, Directory.EnumerateFiles(contentRoot, "*", SearchOption.AllDirectories))
                        ||> Seq.fold (fun registry file ->
                            if [ ".md"; ".fsx" ] |> List.contains (Path.GetExtension(file).ToLowerInvariant()) then
                                registry
                            else
                                let relative = Path.GetRelativePath(contentRoot, file).Replace('\\', '/')
                                registry
                                |> Registry.asset (
                                    Asset.CopyFile(
                                        AbsolutePath.create file,
                                        RelativePath.create (stringValue "route" source + "/" + relative)
                                    )
                                )))

                withAssets
                |> Registry.transform
                    { Name = "repository-content-asset-links"
                      Extensions = [ ".md"; ".markdown"; ".fsx" ]
                      Transform =
                          fun _ page ->
                              match sources |> List.tryFind (fun s -> stringValue "id" s = page.Collection), page.SourceFile with
                              | Some source, Some sourceFile when not (String.IsNullOrEmpty page.Html) ->
                                  let contentRoot = Path.GetFullPath(stringValue "contentRoot" source)
                                  let route = (stringValue "route" source).Trim('/')
                                  let staticRoot = Path.GetFullPath(Path.Combine(root, ".generated", "static", route))
                                  let pageDir = Path.GetDirectoryName(AbsolutePath.value sourceFile)
                                  let rewritten =
                                      relativeHtmlAssetAttr.Replace(
                                          page.Html,
                                          MatchEvaluator(fun m ->
                                              let attr = m.Groups[1].Value
                                              let rawUrl = m.Groups[2].Value
                                              if String.IsNullOrEmpty rawUrl
                                                 || rawUrl.StartsWith("/", StringComparison.Ordinal)
                                                 || rawUrl.StartsWith("#", StringComparison.Ordinal)
                                                 || rawUrl.StartsWith("?", StringComparison.Ordinal)
                                                 || schemePrefix.IsMatch rawUrl then
                                                  m.Value
                                              else
                                                  let qIdx = rawUrl.IndexOfAny([| '?'; '#' |])
                                                  let pathPart, suffix =
                                                      if qIdx >= 0 then rawUrl.Substring(0, qIdx), rawUrl.Substring(qIdx)
                                                      else rawUrl, ""
                                                  let unescaped =
                                                      try Uri.UnescapeDataString pathPart
                                                      with _ -> pathPart
                                                  let candidateInContent =
                                                      Path.GetFullPath(Path.Combine(pageDir, unescaped.Replace('/', Path.DirectorySeparatorChar)))
                                                  let relContent = Path.GetRelativePath(contentRoot, candidateInContent).Replace('\\', '/')
                                                  let isContentAsset =
                                                      not (relContent.StartsWith("..", StringComparison.Ordinal))
                                                      && not (Path.IsPathRooted relContent)
                                                      && File.Exists candidateInContent
                                                      && not ([ ".md"; ".fsx" ] |> List.contains (Path.GetExtension(candidateInContent).ToLowerInvariant()))
                                                  if isContentAsset then
                                                      let encodedRel = relContent.Split('/') |> Array.map Uri.EscapeDataString |> String.concat "/"
                                                      $"{attr}=\"{baseUrl}{route}/{encodedRel}{suffix}\""
                                                  else
                                                      let candidateInStatic =
                                                          Path.GetFullPath(Path.Combine(staticRoot, unescaped.Replace('/', Path.DirectorySeparatorChar)))
                                                      let relStatic = Path.GetRelativePath(staticRoot, candidateInStatic).Replace('\\', '/')
                                                      let isStaticAsset =
                                                          Directory.Exists staticRoot
                                                          && not (relStatic.StartsWith("..", StringComparison.Ordinal))
                                                          && not (Path.IsPathRooted relStatic)
                                                          && File.Exists candidateInStatic
                                                      if isStaticAsset then
                                                          let encodedRel = relStatic.Split('/') |> Array.map Uri.EscapeDataString |> String.concat "/"
                                                          $"{attr}=\"{baseUrl}{route}/{encodedRel}{suffix}\""
                                                      else
                                                          m.Value)
                                      )
                                  { page with Html = rewritten }
                              | _ -> page } }

    let packages =
        [ yield "animejs", "4.5.0"
          for source in sources do
              for package in arrayValue "packages" source do
                  let pair = package.EnumerateArray() |> Seq.toArray
                  yield pair[0].GetString(), pair[1].GetString() ]
        |> List.distinct

    let agentDetails =
        sources
        |> List.map (fun s ->
            let title = stringValue "title" s
            let desc = stringValue "description" s
            if String.IsNullOrEmpty desc then title else $"{title}: {desc}")
        |> String.concat " "

    let site =
        Site.create siteTitle
        |> Site.description "Unified documentation, interactive examples and API reference for the Ranvier ecosystem"
        |> Site.baseUrl baseUrl
        |> Site.origin (stringValue "origin" metadata)
        |> Site.output "output"
        |> Site.staticFiles (Path.Combine(root, ".generated", "static"))
        |> Markdown.register
        |> CompatibilityDirectives.register
        |> Literate.registerWith (fun options -> { options with Extensions = [ ".fsx" ] })
        |> TreeSitter.registerWith csharpGrammar
        |> Sitemap.register
        |> fun s -> if hasApiReference then s |> FSharpApi.register apiOptions else s
        |> LinkValidator.registerWith (LinkValidator.checkExternal false)
        |> Site.plugin contentAssets
        |> Search.register
        |> LiveExample.registerWith (
            LiveExample.preset (
                LiveExamplePreset.create "shared"
                |> LiveExamplePreset.project examplesProject
                |> LiveExamplePreset.css livePreviewCssPath
                |> LiveExamplePreset.template (Path.Combine(root, ".generated", "live", "snippet.html"))
                |> LiveExamplePreset.asDefault
            )
        )
        |> LiveExampleMetadata.register
            [ LiveExampleMetadata.nuget (Path.Combine(root, "src", "Ranvier.Docs.Site", "obj", "project.assets.json")) "Xantham.Fable.Core.TS" "0.2.1" "net8.0" "Xantham.Fable.Core.TS"
              LiveExampleMetadata.nuget (Path.Combine(root, "src", "Ranvier.Docs.Site", "obj", "project.assets.json")) "Xantham.Fable.Core" "0.2.0" "net8.0" "Xantham.Fable.Core" ]
        |> OgImage.registerWith (configureOgImages sources)
        |> AgentFriendly.registerWith (
            AgentFriendly.summary "Fine-grained reactive computation, views, typed CSS, animation, persistence and native UIs for .NET and Fable."
            >> AgentFriendly.details agentDetails
        )
        |> LightningCss.register
        |> Esbuild.register
        |> Nuglify.minifyHtml
        |> RanvierExamples.registerWith { RanvierExamples.defaults with Project = examplesProject; Repository = root; Packages = packages }
        |> Theme.register sharedBrandTheme
        |> Site.collection (
            Collection.create "landing" DocFrontMatter.decoder
            |> Collection.sourceAll landing
            |> Collection.title _.Title
            |> Collection.layout (Theme.layout landingTheme)
        )
        |> fun s ->
            if hasApiReference then
                s
                |> Site.collection (
                    FSharpApi.collection "reference" DocFrontMatter.decoder apiOptions
                    |> Collection.title _.Title
                    |> Collection.layout (Theme.layout sharedBrandTheme)
                )
            else
                s

    (site, sources) ||> List.fold (fun site source -> site |> Site.collection (collection source))

[<EntryPoint>]
let main argv =
    try
        let root =
            Environment.GetEnvironmentVariable "RANVIER_DOCS_ROOT"
            |> Option.ofObj
            |> Option.defaultValue (Directory.GetCurrentDirectory())
        use metadata = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".generated", "site.json")))
        Nacara.run (configure root metadata.RootElement) (Array.append argv [| "--root"; root |])
    with error ->
        eprintfn "Documentation build failed: %s" error.Message
        1
