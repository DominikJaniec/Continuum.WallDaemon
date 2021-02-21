namespace Continuum.WallDaemon.Core

module Executor =

    let letStyle (env: IEnv) (style: WallStyle) =
        async {
            return []
        }

    let makeDesktop (env: IEnv) wallSetter styleSetter =
        let styleSetter =
            match styleSetter with
            | None -> async { return () }
            | Some style -> async {
                let! _ = style
                return ()
            }

        let wallSetter =
            async {
                let! _ = wallSetter
                return ()
            }

        [ wallSetter; styleSetter ]
        |> (Async.Sequential >> Async.Ignore)
        |> Async.RunSynchronously
        |> Ok
