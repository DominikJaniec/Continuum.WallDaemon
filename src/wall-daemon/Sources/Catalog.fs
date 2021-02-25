namespace Continuum.WallDaemon.Sources

open System
open System.IO
open System.Text.RegularExpressions

open Continuum.Common
open Continuum.WallDaemon.Core
open Continuum.WallDaemon.Core.Source


module Catalog =

    let identity = "catalog"

    let private ensureDirectoryExists dir =
        match dir |> Directory.Exists with
        | true -> dir
        | _ -> failwith $"Expected to get existing directory at '%s{dir}'."


    type private Model =
        { source: string
        ; seed: int64
        ; n: int
        }

    let private init dir =
        let s = DateTime.UtcNow.Ticks
        { source = dir
        ; seed = s
        ; n = 0
        }

    let private exec (model: Model) matches =
        let rng =
            int64 (model.n + 1)
            |> (*) model.seed
            |> (int >> Rng)

        let wallpapers =
            let items =
                model.source
                |> ensureDirectoryExists
                |> Directory.GetFiles

            Seq.initInfinite (fun _ ->
                items |> Rng.randomItem rng
            )

        async {
            return
                Seq.distinct wallpapers
                |> Seq.zip matches
                |> List.ofSeq
        }

    type private ConfigItem =
        { dir: string
        }
        with
            interface IConfigItem
    module private ConfigItem =

        let private from (value: string) =
            match value |> String.IsNullOrWhiteSpace with
            | true -> None
            | false ->
                Path.GetFullPath value
                |> (fun path -> { dir = path })
                :> IConfigItem
                |> Some

        let parse config =
            match config with
            | [ path ] -> Some path
            | _ -> None
            |> Option.bind from


    let private getOrders (env: IEnv) =
        env.displays
        |> List.map (fun d -> d.order)

    let private asWallpapers =
        let asOWall (order, path) : OWall =
            (order, ImageFile path)

        List.map asOWall


    type private Impl() =
        interface ISource with
            member val Identity =
                Id identity with get

            member x.Parse (config: string list) =
                match config |> ConfigItem.parse with
                | Some configItem -> configItem |> Ok
                | None ->
                    "Expected to find a single configuration parameter,"
                        + " which represents Wallpapers base catalog path."
                    |> Error

            member x.SetWallpaper (env: IEnv) (config: WallConfig) =
                // TODO: Use config: next-mode and styles

                let baseDir =
                    (config.item :?> ConfigItem).dir
                    |> ensureDirectoryExists

                async {
                    let model = init baseDir
                    return!
                        getOrders env
                        |> exec model
                        |> Async.map asWallpapers
                }

    type private Provider() =
        interface IProvider with

            member val Identity =
                Id identity with get

            member __.Instance() =
                Impl() :> ISource

    let provider () =
        Provider() :> IProvider
