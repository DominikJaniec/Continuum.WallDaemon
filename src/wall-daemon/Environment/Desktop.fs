namespace Continuum.WallDaemon.Environment

open Continuum.Common
open Continuum.WallDaemon.Core


module Desktop =

    let inline private todoWinErrors msg =
        "Implement error handler for Windows"
        |> todo' [ $"Got: %s{msg}" ]

    let private progressSleep =
        // TODO: find better *progress indicator*
        Async.sleepOneSecond

    let private targetedAt (target: WallTarget) action =
        match target with
        | ({ order = No 1u }, { order = No 1u }) ->
            // TODO: pass a correct target
            Windows.DID 0u
            |> action

        | _ -> todoImpl "multi-display setup"


    let private getAsyncAs name (env: IEnv) target action =
        async {
            let targetName = target |> WallTarget.stringify
            $"getting Windows' current %s{name} of %s{targetName}"
            |> env.debugOut

            let style = action |> targetedAt target
            do! progressSleep

            return
                match style with
                | Error msg -> todoWinErrors msg
                | Ok result ->
                    $"found %s{name} of %s{targetName}: '%A{result}'"
                    |> env.debugOut

                    result
        }
        |> Async.StartAsync

    let private setAsyncAs name (env: IEnv) target value action =
        async {
            let targetName = target |> WallTarget.stringify
            $"setting Windows' current %s{name} of %s{targetName}"
            |> env.debugOut

            let change = action value |> targetedAt target
            do! progressSleep

            match change with
            | Error msg -> todoWinErrors msg
            | Ok () ->
                $"%s{name} of %s{targetName} changed to: '%A{value}'"
                |> env.debugOut
        }
        |> Async.StartAsync


    let getStyleAsync env target =
        (fun did -> Windows.getWallpaperStyle ())
        |> getAsyncAs "style" env target

    let getWallpaperAsync env target =
        (fun did -> Windows.getWallpaperImagePath ())
        >> asOk' "No wallpaper path found."
        |> getAsyncAs "wallpaper" env target

    let setStyleAsync env target style =
        (style, (fun s did -> Windows.setWallpaperStyle s))
        ||> setAsyncAs "style" env target

    let setWallpaperAsync env target imgPath =
        (imgPath, (fun s did -> Windows.setWallpaperImage s))
        ||> setAsyncAs "wallpaper" env target
