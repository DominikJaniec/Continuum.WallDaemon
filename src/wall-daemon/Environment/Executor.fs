namespace Continuum.WallDaemon.Environment

open Continuum.Common
open Continuum.WallDaemon.Core


module Executor =

    type LetStyle = Async<OWallStyle list>
    type LetWall = Async<OWall list>


    let private getStyle = Desktop.getStyleAsync
    let private setStyle = Desktop.setStyleAsync

    let private getWallpaper = Desktop.getWallpaperAsync
    let private setWallpaper = Desktop.setWallpaperAsync

    let private forAllDisplays (env: IEnv) x =
        env.displays
        |> List.map (fun d -> d.order, x)

    let private makeStyle (env: IEnv) (styleSetter: LetStyle option) =
        match styleSetter with
        | None ->
            async {
                let! current = getStyle env
                env.debugOut $"displays have styles: %A{current}"

                return current
                |> forAllDisplays env
            }
        | Some changeStyle ->
            async {
                let! changed = changeStyle
                do! Async.sleepOneSecond

                env.debugOut $"displays got styles: %A{changed}"
                return changed
            }

    let private makeWall (env: IEnv) (wallpapers: OWall list) =
        let makeWallPath wallpaper =
            match wallpaper with
            | ImageFile path ->
                async { return path }

        match wallpapers with
        | [] -> failwith "Unexpected empty list of wallpapers."
        | [ No 1u, wallpaper ] ->
            async {
                let! path = makeWallPath wallpaper
                env.debugOut $"changing wallpaper to: '%s{path}'"

                do! path |> setWallpaper env
                env.debugOut "wallpaper successfully changed"

                return ()
            }
        | _ -> todoImpl "multi-display setup"


    let letStyle (env: IEnv) (style: WallStyle) : LetStyle =
        async {
            let! current = getStyle env
            do!
                match style = current with
                | true -> async { return () }
                | false -> setStyle env style

            return style
            |> forAllDisplays env
        }

    let makeDesktop (env: IEnv)
        (wallSource: OWallStyle list -> LetWall)
        (styleSetter: LetStyle option) =

        let styleSetAction =
            styleSetter
            |> makeStyle env

        async {
            let! styles = styleSetAction
            let! walls = wallSource styles
            do! walls |> makeWall env
            return ()
        }
        |> Async.RunSynchronously
        |> Ok
