namespace Continuum.WallDaemon.Sources

open System
open System.IO

open Continuum.Common
open Continuum.WallDaemon.Core
open Continuum.WallDaemon.Core.Source


module Catalog =

    let identity = "catalog"

    let private ensureDirectoryExists dirPath =
        async {
            return
                match dirPath |> Directory.Exists with
                | true -> dirPath
                | _ ->
                    $"Expected to get existing directory at '%s{dirPath}'."
                    |> failwith
        }

    let private getFilesAt dirPath =
        async {
            let! path =
                dirPath
                |> ensureDirectoryExists

            return path
                |> Directory.GetFiles
        }

    type private ConfigItem =
        { dir: string
        ; seed: int
        }
        with
            interface IConfigItem

    module private ConfigItem =
        let private asConfigItem dirPath =
            let s = DateTime.UtcNow.Ticks
            { dir = dirPath
            ; seed = int s
            }
            :> IConfigItem

        let private from (value: string) =
            match value |> String.IsNullOrWhiteSpace with
            | true -> None
            | false ->
                Path.GetFullPath value
                |> asConfigItem
                |> Some

        let parse config =
            match config with
            | [ path ] -> Some path
            | _ -> None
            |> Option.bind from


    let private shuffledFiles (item: ConfigItem) =
        let randomItems source =
            let rng =
                int item.seed
                |> Rng

            Seq.initInfinite (fun _ ->
                source |> Rng.randomItem rng
            )

        async {
            let! allAvailableFiles =
                getFilesAt item.dir

            return
                allAvailableFiles
                |> randomItems
                |> Seq.distinct
        }


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

            member x.Wallpapers (env: IEnv) (config: WallConfig) =
                // TODO: Handle all settups not only random stream
                // TODO: Use config: next-mode and styles

                let asWallpaper (wallpaper, target) : TargetedWallpaper =
                    (ImageFile wallpaper, target)

                async {
                    let! wallpapers =
                        config.item
                        :?> ConfigItem
                        |> shuffledFiles

                    return
                        WallTarget.allOf env
                        |> Seq.zip wallpapers
                        |> Seq.map asWallpaper
                        |> List.ofSeq
                }


    type private Provider() =
        interface IProvider with

            member val Identity =
                Id identity with get

            member __.Instance() =
                Impl() :> ISource

    let provider () =
        Provider() :> IProvider
