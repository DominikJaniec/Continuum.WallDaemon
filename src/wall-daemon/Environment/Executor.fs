namespace Continuum.WallDaemon.Environment

open Continuum.Common
open Continuum.WallDaemon.Core


module Executor =

    let private ensureNotEmpty xs =
        match xs with
        | [] -> failwith "Unexpected empty list."
        | [ x ] -> [ x ]
        | _ -> todoImpl "multi-display setup support"


    let private setWallpapers (env: IEnv) (wallpapers: Wallpapers) =
        let extractFilePathFrom wallpaper =
            match wallpaper with
            | ImageFile path ->
                async { return path }

        let setWallpaper (wallpaper, target) =
            async {
                let! path = extractFilePathFrom wallpaper
                do! path |> Desktop.setWallpaperAsync env target
                return ()
            }

        wallpapers
        |> ensureNotEmpty
        |> List.map setWallpaper
        |> Async.Sequential
        |> Async.Ignore


    let getStyles env =
        let getStyleAt target =
            async {
                let! current = Desktop.getStyleAsync env target
                return (current, target)
            }

        WallTarget.allOf env
        |> ensureNotEmpty
        |> Seq.map getStyleAt
        |> Async.SequentialList


    let setStyles env (wallStyles: WallStyles) =
        let setStyle targetedWallStyle =
            let (wallStyle, target) = targetedWallStyle
            async {
                do! Desktop.setStyleAsync env target wallStyle
                return targetedWallStyle
            }

        wallStyles
        |> ensureNotEmpty
        |> Seq.map setStyle
        |> Async.SequentialList


    let makeDesktopSettup
        env
        (wallStylesSource: Async<WallStyles>)
        (wallpapersSource: WallStyles -> Async<Wallpapers>) =
        async {
            let! styles = wallStylesSource
            let! walls = wallpapersSource styles
            do! walls |> setWallpapers env
            return ()
        }
        |> Async.RunSynchronously
        // TODO: handle errors?
        |> Ok
