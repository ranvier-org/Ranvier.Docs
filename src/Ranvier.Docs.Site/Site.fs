module private Ranvier.Docs.Site

open System
open System.IO
open System.Text.Json
open Nacara.Core
open Nacara.Plugins
open Nacara.Plugins.Ranvier.Theme

let private stringValue (name: string) (element: JsonElement) =
    match element.TryGetProperty name with
    | true, value when value.ValueKind = JsonValueKind.String -> value.GetString()
    | _ -> ""

let private arrayValue (name: string) (element: JsonElement) =
    match element.TryGetProperty name with
    | true, value when value.ValueKind = JsonValueKind.Array -> value.EnumerateArray() |> Seq.toList
    | _ -> []

let private pageUrl (route: string) (path: string) =
    let page = Path.ChangeExtension(path, null).Replace('\\', '/').ToLowerInvariant()
    let suffix = if page = "index" then "" elif page.EndsWith("/index") then page[..page.Length-6] else page
    "/" + String.concat "/" ([route.Trim('/'); suffix.Trim('/')] |> List.filter (String.IsNullOrEmpty >> not)) + "/"

let private configure root (metadata: JsonElement) =
    let sources = arrayValue "sources" metadata
    let baseUrl = stringValue "baseUrl" metadata
    let siteTitle =
        match stringValue "title" metadata with
        | "" -> "Ranvier documentation"
        | title -> title
    let displayTitle (page: Page) =
        sources
        |> List.tryFind (fun source -> stringValue "id" source = page.Collection)
        |> Option.bind (fun source ->
            page.SourceFile
            |> Option.bind (fun file ->
                let relative = Path.GetRelativePath(stringValue "contentRoot" source, AbsolutePath.value file).Replace('\\', '/')
                arrayValue "sections" source
                |> List.collect (arrayValue "pages")
                |> List.tryFind (fun entry -> stringValue "path" entry = relative)
                |> Option.map (fun entry -> { page with Title = stringValue "title" entry })))
        |> Option.defaultValue page
    let theme =
        Theme.defaults
        |> (fun options ->
            let options =
                options
                |> Theme.navbar [ for source in sources -> NavbarSection(stringValue "title" source, stringValue "route" source, pageUrl (stringValue "route" source) (stringValue "index" source)) ]
                |> Theme.navbarEnd [ NavbarDynamicWidget Search.trigger ]
            (options, sources)
            ||> List.fold (fun options source ->
                let pages =
                    [ for section in arrayValue "sections" source ->
                        Menu.section (stringValue "title" section)
                            [ for page in arrayValue "pages" section ->
                                Menu.page (stringValue "id" source + ":" + stringValue "path" page) ] ]
                options |> Theme.menu (stringValue "route" source) pages))
    let collection source =
        let sourceTheme = theme |> Theme.editUrl ((stringValue "editUrl" source).TrimEnd('/'))
        Collection.create (stringValue "id" source) DocFrontMatter.decoder
        |> Collection.sourceAll (stringValue "contentRoot" source)
        |> Collection.routePrefix (stringValue "route" source)
        |> Collection.title _.Title
        |> Collection.order (fun frontMatter -> defaultArg frontMatter.Order 0)
        |> Collection.layout (fun context ->
            let sourcePath =
                context.Page.SourceFile
                |> Option.map (AbsolutePath.value >> fun file -> Path.GetRelativePath(stringValue "contentRoot" source, file).Replace('\\', '/') |> RelativePath.create)
            Theme.layout sourceTheme { context with Page = { displayTitle context.Page with ProjectPath = sourcePath }; Pages = List.map displayTitle context.Pages })
    let landing = Path.Combine(root, ".generated", "landing")
    Directory.CreateDirectory landing |> ignore
    let lines =
        [ yield! [ "---"; "title: " + JsonSerializer.Serialize(siteTitle); "---"; "# Build with Ranvier"; "";
                   "Signals, views, typed styles and application services share one documentation home. Choose a library to start, or search across all guides."; "" ]
          for source in sources do
              let title = stringValue "title" source
              let url = Path.GetRelativePath(landing, Path.Combine(stringValue "contentRoot" source, stringValue "index" source)).Replace('\\', '/')
              yield $"- [{title}]({url})"
          yield ""
          yield "Each page links back to its source repository. Documentation and runnable examples stay with the libraries that own them." ]
    File.WriteAllLines(Path.Combine(landing, "index.md"), lines)
    let examplesProject = Path.Combine(root, ".generated", "Examples.fsproj")
    let contentAssets =
        { new IPlugin with
            member _.Name = "repository-content-assets"
            member _.Configure registry =
                (registry, sources)
                ||> List.fold (fun registry source ->
                    let contentRoot = stringValue "contentRoot" source
                    (registry, Directory.EnumerateFiles(contentRoot, "*", SearchOption.AllDirectories))
                    ||> Seq.fold (fun registry file ->
                        if [ ".md"; ".fsx" ] |> List.contains (Path.GetExtension(file).ToLowerInvariant()) then registry
                        else
                            let relative = Path.GetRelativePath(contentRoot, file).Replace('\\', '/')
                            registry |> Registry.asset (Asset.CopyFile(AbsolutePath.create file, RelativePath.create (stringValue "route" source + "/" + relative))))) }
    let packages =
        [ yield "animejs", "4.5.0"
          for source in sources do
              for package in arrayValue "packages" source do
                  let pair = package.EnumerateArray() |> Seq.toArray
                  yield pair[0].GetString(), pair[1].GetString() ]
        |> List.distinct
    let site =
        Site.create siteTitle
        |> Site.description "Documentation for the Ranvier ecosystem"
        |> Site.baseUrl baseUrl
        |> Site.origin (stringValue "origin" metadata)
        |> Site.output "output"
        |> Site.staticFiles (Path.Combine(root, ".generated", "static"))
        |> Markdown.register
        |> Ranvier.Docs.CompatibilityDirectives.register
        |> Literate.registerWith (fun options -> { options with Extensions = [ ".fsx" ] })
        |> TreeSitter.register
        |> Sitemap.register
        |> LinkValidator.registerWith (LinkValidator.checkExternal false)
        |> Site.plugin contentAssets
        |> Search.register
        |> LiveExample.registerWith (LiveExample.preset (LiveExamplePreset.create "shared" |> LiveExamplePreset.project examplesProject |> LiveExamplePreset.template (Path.Combine(root, ".generated", "live", "snippet.html")) |> LiveExamplePreset.asDefault))
        |> LiveExampleMetadata.register
            [ LiveExampleMetadata.nuget (Path.Combine(root, "src", "Ranvier.Docs.Site", "obj", "project.assets.json")) "Xantham.Fable.Core.TS" "0.2.1" "net8.0" "Xantham.Fable.Core.TS"
              LiveExampleMetadata.nuget (Path.Combine(root, "src", "Ranvier.Docs.Site", "obj", "project.assets.json")) "Xantham.Fable.Core" "0.2.0" "net8.0" "Xantham.Fable.Core" ]
        |> LightningCss.register
        |> Esbuild.register
        |> Nuglify.minifyHtml
        |> RanvierExamples.registerWith { RanvierExamples.defaults with Project = examplesProject; Repository = root; Packages = packages }
        |> Theme.register theme
        |> Site.collection (Collection.create "landing" DocFrontMatter.decoder |> Collection.sourceAll landing |> Collection.title _.Title |> Collection.layout (Theme.layout theme))
    (site, sources) ||> List.fold (fun site source -> site |> Site.collection (collection source))

[<EntryPoint>]
let main argv =
    try
        let root = Environment.GetEnvironmentVariable "RANVIER_DOCS_ROOT" |> Option.ofObj |> Option.defaultValue (Directory.GetCurrentDirectory())
        use metadata = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, ".generated", "site.json")))
        Nacara.run (configure root metadata.RootElement) (Array.append argv [| "--root"; root |])
    with error ->
        eprintfn "Documentation build failed: %s" error.Message
        1
