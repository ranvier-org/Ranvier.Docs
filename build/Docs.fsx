#i "nuget: https://nuget.cloudsmith.io/shayanhabibi/shayanhabibi-partas-build/v3/index.json"
#r "nuget: Partas.Build, 0.8.2"

open System
open System.IO
open System.Text.Json
open System.Security.Cryptography
open System.Threading
open Partas.Build

let root = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, ".."))
let full path = Path.Combine(root, path)
let node args = Cmd.ofList "node" args
let dotnet args = Cmd.ofList "dotnet" args
let environment = [
    "RanvierTrace", "true"
    "RANVIER_DOCS_ROOT", root
    "CustomBeforeMicrosoftCommonTargets", full "src/SourceDependencies.targets"
    "MSBUILDDISABLENODEREUSE", "1"
    "DOTNET_CLI_USE_MSBUILD_SERVER", "0"
]

let executeStage (stageDefinition: StageContext) =
    let definition = Command.root {
        workingDir root
        envVars environment
        command "run" { stageDefinition }
    }
    let result = Command.invoke [| "run" |] definition
    if result.ExitCode <> 0 then failwith $"Documentation stage failed ({result.ExitCode})."

let hashFile path = File.ReadAllBytes path |> SHA256.HashData |> Convert.ToHexString
let current marker inputs =
    File.Exists(full marker)
    && File.ReadAllText(full marker) = (inputs |> List.map (full >> hashFile) |> String.concat ":")
let stamp marker inputs =
    Directory.CreateDirectory(Path.GetDirectoryName(full marker)) |> ignore
    File.WriteAllText(full marker, inputs |> List.map (full >> hashFile) |> String.concat ":")

let npm args =
    if OperatingSystem.IsWindows() then
        let cli =
            Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator)
            |> Array.map (fun directory -> Path.Combine(directory.Trim('"'), "node_modules/npm/bin/npm-cli.js"))
            |> Array.tryFind File.Exists
        match cli with
        | Some cli -> node (cli :: args)
        | None -> failwith "Node 26 with its npm dependency manager is required. Install Node, then rerun the same docs command."
    else Cmd.ofList "npm" args

let bootstrap = stage "restore documentation tooling" {
    workingDir root
    envVars environment
    stage "check Node compiler runtime" { run (node ["--eval"; "if(Number(process.versions.node.split('.')[0])<26)throw Error('Documentation requires Node 26 or newer')"]) }
    stage "restore pinned browser dependencies" {
        run (fun _ ->
            if not (["@playwright/test"; "animejs"; "parse5"; "pagefind"; "mdast-util-from-markdown"] |> List.forall (fun package -> Directory.Exists(full ("node_modules/" + package)))) || not (current ".nacara/bootstrap/npm.lock" ["package-lock.json"]) then
                Some(npm ["ci"; "--no-audit"; "--no-fund"])
            else None)
        run (fun _ -> stamp ".nacara/bootstrap/npm.lock" ["package-lock.json"])
    }
    stage "restore pinned .NET tools" {
        run (fun _ ->
            if not (current ".nacara/bootstrap/tools.lock" [".config/dotnet-tools.json"; "global.json"]) then Some(dotnet ["tool"; "restore"])
            else None)
        run (fun _ -> stamp ".nacara/bootstrap/tools.lock" [".config/dotnet-tools.json"; "global.json"])
    }
}

module Options =
    let source = Input.option<string list> "--source" |> Input.arity Arity.ZeroOrMore |> Input.allowMultipleArgumentsPerToken |> Input.description "Preview a local repository: --source view=../Ranvier.View (repeatable)"
    let local = Input.option<bool> "--local" |> Input.description "Use the registered local checkouts"
    let registry = Input.option<string> "--registry" |> Input.def "sources.json" |> Input.description "Source registry path"
    let watch = Input.option<bool> "--watch" |> Input.description "Serve and rebuild on source changes; never runs tests"
    let refresh = Input.option<string> "--refresh" |> Input.def "" |> Input.description "Accept updated source revisions: all, or comma-separated source ids"
    let suite = Input.option<string> "--suite" |> Input.def "node" |> Input.acceptOnlyFromAmong ["node"; "examples"; "theme"; "metadata"; "browser"] |> Input.description "Run only the selected component's tests"

let sourceArguments sources local registry =
    ["--registry"; registry] @ (if local then ["--local"] else [])
    @ (sources |> List.collect (fun source -> ["--source"; source]))

let acquireWith arguments refresh = stage "acquire locked documentation sources" {
    workingDir root
    envVars environment
    run (node (["scripts/sources.mjs"] @ arguments @ (if refresh = "" then [] elif refresh = "all" then ["--refresh"] else ["--refresh"; refresh])))
}
let acquire registry refresh = acquireWith ["--registry"; registry] refresh

let text (name: string) (element: JsonElement) : string = element.GetProperty(name).GetString()
let readPlan () = JsonDocument.Parse(File.ReadAllText(full ".nacara/build-plan.json"))

let buildPreparedAssets () =
    use plan = readPlan ()
    for bundle in plan.RootElement.GetProperty("bundles").EnumerateArray() do
        if not (bundle.GetProperty("cached").GetBoolean()) then
            let project = text "projectRoot" bundle
            let vars = bundle.GetProperty("env").EnumerateObject() |> Seq.map (fun property -> property.Name, property.Value.GetString()) |> Seq.toList
            let server = if Path.GetFileName(project) = "Ranvier.Motion" then "Server/ServerChecks.fsproj" else "Server/Server.fsproj"
            executeStage (stage $"build browser example {Path.GetFileName(project)}" {
                workingDir project
                envVars vars
                run (fun _ -> npm ["ci"; "--no-audit"; "--no-fund"])
                run (dotnet ["build"; server; "-c"; "Release"; "-m:1"; "-p:BuildInParallel=false"])
                run (dotnet ["run"; "--project"; server; "-c"; "Release"; "--no-build"; "--"; "--write-pages"])
                run (dotnet ["fickl"; "Client.fsproj"; "--outDir"; "generated"; "--configuration"; "Release"])
                run (node [Path.Combine(project, "node_modules/vite/bin/vite.js"); "build"])
            })
    let performance = plan.RootElement.GetProperty("performance")
    if performance.ValueKind <> JsonValueKind.Null && not (performance.GetProperty("cached").GetBoolean()) then
        executeStage (stage "generate recorded performance charts" {
            workingDir (text "coreRoot" performance)
            run (node [text "generator" performance; text "runsDirectory" performance; text "outputFile" performance])
        })

let prepare arguments = stage "prepare source content and runnable assets" {
    workingDir root
    envVars environment
    run (node (["scripts/prepare.mjs"; "plan"] @ arguments))
    run (fun _ -> buildPreparedAssets ())
    run (node (["scripts/prepare.mjs"; "finish"] @ arguments))
}

let compile = stage "compile documentation host" {
    workingDir root
    envVars environment
    run (dotnet ["build"; "src/Ranvier.Docs.Site/Ranvier.Docs.Site.fsproj"; "-c"; "Release"; "--nologo"])
}
let render = stage "render shared documentation" {
    workingDir root
    envVars environment
    run (dotnet ["src/Ranvier.Docs.Site/bin/Release/net10.0/Ranvier.Docs.Site.dll"; "build"])
}
let check = stage "validate published links, anchors and assets" { workingDir root; envVars environment; run (node ["scripts/check-output.mjs"]) }
let serve = stage "serve documentation" { workingDir root; envVars environment; run (node ["scripts/serve.mjs"]) }
let buildSolution = stage "compile verification projects" { workingDir root; envVars environment; run (dotnet ["build"; "Ranvier.Docs.slnx"; "-c"; "Release"; "--nologo"]) }

let installBrowser = stage "prepare browser verification runtime" {
    workingDir root
    envVars environment
    run (node (["node_modules/@playwright/test/cli.js"; "install"] @ (if OperatingSystem.IsLinux() then ["--with-deps"] else []) @ ["chromium"]))
}
let testSuite suite =
    let command () =
        match suite with
        | "node" -> node (["--test"] @ (["tests"; "tests/site"] |> List.collect (fun directory -> Directory.GetFiles(full directory, "*.test.mjs") |> Array.toList)))
        | "examples" -> dotnet ["run"; "--project"; "tests/examples/Ranvier.Docs.Maps.Tests.fsproj"; "-c"; "Release"; "--no-build"]
        | "theme" -> dotnet ["run"; "--project"; "tests/Nacara.Plugin.Ranvier.Tests/Nacara.Plugin.Ranvier.Tests.fsproj"; "-c"; "Release"; "--no-build"]
        | "metadata" -> dotnet ["fsi"; "tests/examples/MetadataTests.fsx"]
        | "browser" -> node ["node_modules/@playwright/test/cli.js"; "test"]
        | _ -> invalidArg "suite" suite
    stage $"test {suite}" { workingDir root; envVars environment; run (fun _ -> command ()) }

let mutable rebuild = fun (_: string list) -> ()
let watchSources arguments = Operation.ofAsync (async {
    let! cancellation = Async.CancellationToken
    use changed = new AutoResetEvent(false)
    let mutable watchers: FileSystemWatcher list = []
    let ignored (path: string) =
        path.Replace('\\', '/').Split('/')
        |> Array.exists (fun segment -> ["bin"; "obj"; "node_modules"; "output"; "dist"; "generated"; "generated-tests"; ".nacara"; ".git"; ".generated"] |> List.contains segment)
    let observe () =
        watchers |> List.iter _.Dispose()
        use metadata = JsonDocument.Parse(File.ReadAllText(full ".generated/watch-roots.json"))
        let folders = metadata.RootElement.EnumerateArray() |> Seq.map _.GetString() |> Seq.filter Directory.Exists |> Seq.distinct |> Seq.toList
        watchers <- [
            for folder in folders do
                let watcher = new FileSystemWatcher(folder, IncludeSubdirectories = (folder <> root))
                let signal (event: FileSystemEventArgs) = if not (ignored event.FullPath) then changed.Set() |> ignore
                watcher.Changed.Add signal
                watcher.Created.Add signal
                watcher.Deleted.Add signal
                watcher.Renamed.Add(fun event -> signal event)
                watcher.Error.Add(fun _ -> changed.Set() |> ignore)
                watcher.EnableRaisingEvents <- true
                yield watcher
        ]
    try
        observe ()
        while not cancellation.IsCancellationRequested do
            let index = WaitHandle.WaitAny [| changed :> WaitHandle; cancellation.WaitHandle |]
            if index = 0 then
                do! Async.Sleep 300
                changed.Reset() |> ignore
                try
                    rebuild arguments
                    observe ()
                    printfn "Documentation refreshed. Reload the browser to see changes."
                with error -> eprintfn "Preview rebuild failed: %s\nFix the source and save to retry." error.Message
    finally watchers |> List.iter _.Dispose()
})

let documentation = input {
    let! sources = Options.source
    and! local = Options.local
    and! registry = Options.registry
    and! watching = Options.watch
    let arguments = sourceArguments sources local registry
    return stage "documentation" {
        bootstrap
        acquireWith arguments ""
        prepare arguments
        compile
        render
        check
        stage "preview" {
            when' watching
            parallel'
            serve
            stage "watch source repositories" { runOperation (watchSources arguments) }
        }
    }
}

let definition = Command.root {
    description "Ranvier documentation automation powered by Partas.Build"
    workingDir root
    envVars environment
    command "docs" { description "Build the docs, or preview them with --watch"; documentation }
    command "prepare" {
        description "Restore and prepare registered documentation without rendering"
        input {
            let! sources = Options.source
            and! local = Options.local
            and! registry = Options.registry
            let arguments = sourceArguments sources local registry
            return stage "prepare documentation" { bootstrap; acquireWith arguments ""; prepare arguments }
        }
    }
    command "sources" {
        description "Fetch locked sources or deliberately refresh selected revisions"
        input {
            let! registry = Options.registry
            and! refresh = Options.refresh
            return acquire registry refresh
        }
    }
    command "check" { description "Validate generated links and assets"; bootstrap; check }
    command "serve" { description "Serve an already built site"; bootstrap; serve }
    command "test" {
        description "Run a focused suite without rebuilding the whole site"
        bootstrap
        input {
            let! suite = Options.suite
            return stage "selected verification" {
                stage "prepare missing source inputs" {
                    when' (suite <> "node" && not (File.Exists(full ".generated/DependencyRoots.props")))
                    acquire "sources.json" ""
                    prepare []
                }
                stage "prepare compiled test projects" { when' (["examples"; "theme"; "metadata"] |> List.contains suite); buildSolution }
                stage "prepare missing browser output" {
                    when' (suite = "browser" && not (File.Exists(full "output/index.html")))
                    compile
                    render
                }
                stage "prepare browser tests" { when' (suite = "browser"); installBrowser }
                testSuite suite
            }
        }
    }
    command "verify" {
        description "Run the full acceptance gate once, including browser setup"
        bootstrap
        testSuite "node"
        acquire "sources.json" ""
        prepare []
        compile
        render
        buildSolution
        testSuite "examples"
        testSuite "theme"
        testSuite "metadata"
        check
        installBrowser
        testSuite "browser"
    }
    command "publish" {
        description "Build, verify and deploy the published main revision through GitHub Actions"
        stage "publish central documentation" { run (Cmd.ofList "gh" ["workflow"; "run"; "docs.yml"; "--repo"; "ranvier-org/Ranvier.Docs"; "--ref"; "main"; "-f"; "deploy=true"]) }
    }
}

rebuild <- fun arguments ->
    executeStage (stage "refresh documentation" { prepare arguments; compile; render; check })

let run arguments = (Command.invoke arguments definition).ExitCode
