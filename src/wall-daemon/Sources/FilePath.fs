namespace Continuum.WallDaemon.Sources

open Continuum.WallDaemon.Core.Source


module FilePath =

    let identity = "file-path"

    type private Impl() =
        interface ISource with
            member val Identity =
                Id identity with get

    type private Provider() =
        interface IProvider with

            member val Identity =
                Id identity with get

            member __.Instance() =
                Impl() :> ISource

    let provider () =
        Provider() :> IProvider
