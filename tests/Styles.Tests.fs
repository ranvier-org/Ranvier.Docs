module Nacara.Plugins.Ranvier.Tests.Styles

open System
open System.IO
open System.Text.RegularExpressions
open Expecto

let private canonicalCss css =
    let strings = ResizeArray<string>()
    let text =
        Regex.Matches(css, """"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|/\*[\s\S]*?\*/|[^"'/]+|/""")
        |> Seq.cast<Match>
        |> Seq.choose (fun matched ->
            let value = matched.Value
            if value.StartsWith("/*", StringComparison.Ordinal) then None
            elif value[0] = '"' || value[0] = '\'' then
                let index = strings.Count
                strings.Add value
                Some $"\uE000{index}\uE001"
            else Some value)
        |> String.concat ""
        |> fun value -> Regex.Replace(value, @"\s+", " ")
        |> fun value -> Regex.Replace(value, @"\s*([{}:;,()>+~=])\s*", "$1")
        |> fun value -> Regex.Replace(value, @";}", "}").Trim()
    Regex.Replace(text, "\uE000([0-9]+)\uE001", MatchEvaluator(fun matched -> strings[int matched.Groups[1].Value]))

/// <summary>Checks the complete default cascade and its structured stylesheet representation.</summary>
[<Tests>]
let tests =
    testList "default theme stylesheet" [
        testCase "all declarations and nested rules match the default cascade" <| fun _ ->
            let fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "default-theme.css")
            let expected = File.ReadAllText fixture |> canonicalCss
            let actual = Nacara.Plugins.Ranvier.Theme.Styles.render () |> canonicalCss
            Expect.equal actual expected "Selectors, declaration order, token defaults and responsive rules must match."

        testCase "stylesheet uses structured rules" <| fun _ ->
            let rec structured rules =
                rules |> List.forall (function
                    | Ranvier.Css.Rule.RawRule _ -> false
                    | Ranvier.Css.Rule.StyleRule (_, _, children)
                    | Ranvier.Css.Rule.GroupRule (_, _, children) -> structured children
                    | _ -> true)
            Expect.isTrue (structured Nacara.Plugins.Ranvier.Theme.Styles.rules) "Default rules must remain inspectable Ranvier.Css values."
    ]
