namespace Continuum.WallDaemon.Sources

open Continuum.WallDaemon.Core
open Continuum.WallDaemon.Core.Source
open Continuum.WallDaemon.Environment


module Catalog =

    let identity = "catalog"

    type private Impl() =
        interface ISource with
            member val Identity =
                Id identity with get

            member x.SetWallpaper (env: IEnv) (config: WallConfig) =
                async {
                    Daemon.setWallpaperDemo()
                    return []
                }

    type private Provider() =
        interface IProvider with

            member val Identity =
                Id identity with get

            member __.Instance() =
                Impl() :> ISource

    let provider () =
        Provider() :> IProvider
