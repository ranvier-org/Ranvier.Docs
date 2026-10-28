---
title: Shared runnable examples
---

These examples use the central preset. The source repository needs no example project.

## Server rendering and hydration

```fsharp ranvier setup
open Ranvier.Css
let sharedTypedStyle = Css.boxSizing CssValue.BoxSizing.borderBox
```

```fsharp ranvier id=shared-counter
Ranvier.Docs.Maps.PipelineFixture.counter
```

## Core signal map

```fsharp map id=shared-map
let count = createSignal 1
let doubled = createMemo (fun _ -> count.Value * 2)
createEffect (fun () -> printfn "%d" doubled.Value)
controls [ button "Increment map" (fun () -> count.Value <- count.Value + 1) ]
```

## Motion

```fsharp ranvier render=motion id=shared-motion
open Ranvier
open Ranvier.Anime
open Ranvier.View
open Ranvier.View.Sugar
open Ranvier.View.Anime

let motion =
    Component.template
        (fun _ () -> createSignal 0.)
        (div () {
            button (Events.onClick (fun (position: Signal<float>) _ -> position.Value <- position.Value + 40.)) { "Move" }
            div (Attrs.className "motion-target", Motion.follow "translateX" (AnimatableParams (duration = 100)) (fun (position: Signal<float>) -> position.Value)) { "Motion" }
        })
```

## Editable component

```fsharp live
let mounted = Ranvier.Docs.Maps.MapMount.body Ranvier.Docs.Maps.PipelineFixture.counter
```
