#load "../../build/Docs.fsx"

open System
open System.IO

let stages = [ Docs.compile; Docs.render; Docs.buildSolution; Docs.testSuite "examples"; Docs.testSuite "metadata" ]
for stage in stages do
    if stage.EnvVars.TryFind "RanvierTrace" <> Some "true" then
        failwith $"{stage.Name}: traced dependency compilation must be configured on the stage."
    if stage.EnvVars.TryFind "CustomBeforeMicrosoftCommonTargets" <> Some(Docs.full "src/SourceDependencies.targets") then
        failwith $"{stage.Name}: the coordinated source dependency bridge is missing."
    if stage.WorkingDir <> ValueSome Docs.root then
        failwith $"{stage.Name}: relative paths must resolve from the repository, including invocation from another directory."
printfn "Coordinated compilation environment verified."
