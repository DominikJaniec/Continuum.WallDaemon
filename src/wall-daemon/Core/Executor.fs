namespace Continuum.WallDaemon.Core

module Async =
    let sleepOneSecond =
        System.TimeSpan.FromSeconds 1.0
        |> Async.Sleep

    let StartAsync a =
        async {
            let! r = a
            return r
        }

module Executor =

    let private getStyle (env: IEnv) =
        async {
            env.debugOut "getting Windows' current desktop style"
            let style = Windows.getWallpaperStyle ()
            return
                match style with
                | Error msg -> failwith msg
                | Ok result ->
                    env.debugOut $"found desktop style: '%A{result}'"
                    result
        }
        |> Async.StartAsync

    let private setStyle (env: IEnv) style =
        async {
            env.debugOut "setting Windows' current desktop style"
            let change = Windows.setWallpaperStyle style
            match change with
            | Error msg -> failwith msg
            | Ok () ->
                env.debugOut $"desktop style changed to: '%A{style}'"
        }
        |> Async.StartAsync


    type LetStyle = Async<OWallStyle list>
    type LetWall = Async<OWall list>

    let letStyle (env: IEnv) (style: WallStyle) : LetStyle =
        let assumeAllDisplays x =
            env.displays
            |> List.map (fun d -> d.order, x)

        async {
            let! current = getStyle env
            do!
                match style = current with
                | true -> async { return () }
                | false -> setStyle env style

            return assumeAllDisplays style
        }

    let makeDesktop (env: IEnv)
        (wallSetter: OWallStyle list -> LetWall)
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
            let! _ = wallSetter styles
            return ()
        }
        |> Async.RunSynchronously
        |> Ok
