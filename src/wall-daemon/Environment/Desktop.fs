namespace Continuum.WallDaemon.Environment

open Continuum.Common
open Continuum.WallDaemon.Core


module Desktop =

    let inline private todoWinErrors msg =
        "Implement error handler for Windows"
        |> todo' [ $"Got: %s{msg}"]

    let private getAsyncAs name (env: IEnv) action =
        async {
            env.debugOut $"getting Windows' current desktop %s{name}"
            let style = action ()
            return
                match style with
                | Error msg -> todoWinErrors msg
                | Ok result ->
                    env.debugOut $"found desktop %s{name}: '%A{result}'"
                    result
        }
        |> Async.StartAsync

    let private setAsyncAs name (env: IEnv) value action =
        async {
            env.debugOut $"setting Windows' current desktop %s{name}"
            let change = action value
            match change with
            | Error msg -> todoWinErrors msg
            | Ok () ->
                env.debugOut $"desktop %s{name} changed to: '%A{value}'"
        }
        |> Async.StartAsync

    let getStyleAsync env =
        Windows.getWallpaperStyle
        |> getAsyncAs "style" env

    let getWallpaperAsync env =
        Windows.getWallpaperImagePath
        >> asOk' "No path found."
        |> getAsyncAs "wallpaper" env

    let setStyleAsync env style =
        (style, Windows.setWallpaperStyle)
        ||> setAsyncAs "style" env

    let setWallpaperAsync env imgPath =
        (imgPath, Windows.setWallpaperImage)
        ||> setAsyncAs "wallpaper" env
