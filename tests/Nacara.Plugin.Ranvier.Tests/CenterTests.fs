/// <summary>Checks centered Markdown within the complete native documentation layout.</summary>
module Nacara.Plugins.Ranvier.Tests.CenterIntegration

open System.IO
open Expecto
open Nacara.Core
open Nacara.Plugins
open Nacara.Plugins.Ranvier.Theme

/// <summary>Checks that the native directive can be embedded in the complete native documentation layout.</summary>
[<Tests>]
let tests =
    testCase "a native centered directive embeds inside the actual Ranvier documentation theme" <| fun _ ->
        let directory = Directory.CreateTempSubdirectory("ranvier-center-integration-")
        try
            File.WriteAllText(
                Path.Combine(directory.FullName, "index.md"),
                "---\ntitle: Supported targets\n---\n:::center\n\n| Target | Supported |\n| --- | --- |\n| net10 | Yes |\n\n:::"
            )
            let theme = Theme.defaults
            let collection =
                Collection.create "guide" DocFrontMatter.decoder
                |> Collection.sourceAll directory.FullName
                |> Collection.title _.Title
                |> Collection.layout (Theme.layout theme)
            let site =
                Site.create "Center integration"
                |> Site.noStaticFiles
                |> Markdown.register
                |> Ranvier.Docs.CompatibilityDirectives.register
                |> Theme.register theme
                |> Site.collection collection
            let result = Build.check (AbsolutePath.create directory.FullName) site
            Expect.equal result.Pages.Length 1 "The source page must render through the full native theme."
            Expect.isEmpty result.Diagnostics "The supported directive must produce no warnings or errors."
            let body = result.Pages.Head.Html
            Expect.stringContains body "style=\"display:flex;justify-content:center\"" "The original wrapper layout must remain."
            Expect.stringContains body "<table>" "The rendered Markdown body must remain HTML."
            Expect.stringContains body "net10" "The original table content must remain."
            Expect.isFalse (body.Contains("<!--[")) "An embedded static fragment must not carry reserved Ranvier boundary comments."
        finally
            directory.Delete(true)
