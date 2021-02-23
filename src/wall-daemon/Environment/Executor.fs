namespace Continuum.WallDaemon.Environment

open Continuum.Common
open Continuum.WallDaemon.Core


module Executor =

    let private getStyle = Desktop.getStyleAsync
    let private setStyle = Desktop.setStyleAsync

    // TODO: Not tested yet!
    let private getWallpaper = Desktop.getWallpaperAsync
    let private setWallpaper = Desktop.setWallpaperAsync

    let private allDisplays (env: IEnv) x =
        env.displays
        |> List.map (fun d -> d.order, x)

    type LetStyle = Async<OWallStyle list>
    type LetWall = Async<OWall list>

    let letStyle (env: IEnv) (style: WallStyle) : LetStyle =
        async {
            let! current = getStyle env
            do!
                match style = current with
                | true -> async { return () }
                | false -> setStyle env style

            return allDisplays env style
        }

    let makeDesktop (env: IEnv)
        (wallSource: OWallStyle list -> LetWall)
        (styleSetter: LetStyle option) =

        let styleChangeAction =
            match styleSetter with
            | None -> async { return [] }
            | Some changeStyle -> async {
                let! styles = changeStyle
                do! Async.sleepOneSecond

                env.debugOut $"displays got styles: %A{styles}"
                return styles
            }

        async {
            let! styles = styleChangeAction
            let! _ = wallSource styles
            return ()
        }
        |> Async.RunSynchronously
        |> Ok
