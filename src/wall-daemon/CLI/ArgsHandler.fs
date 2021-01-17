namespace Continuum.WallDaemon.CLI

open Argu
open Arguments
open Continuum.WallDaemon.Core


module ArgsHandler =

    type ArgsApp = ParseResults<Args>
    type ArgsWall = ParseResults<WallArgs>
    type ArgsService = ParseResults<ServiceArgs>
    type ArgsProfile = ParseResults<ProfileArgs>
    type ArgsResult = Result<unit, string * int>

    type IEnv =
        abstract member printOut : string -> unit
        abstract member printErr : string -> unit
        abstract member debugOut : string -> unit


    let execute (env: IEnv) (args: ArgsApp) : ArgsResult =

        let handleArgs (args: ArgsApp) : ArgsResult =
            failwith "TODO | implement it"

        let handleService (args: ArgsService) =
            failwith "TODO | implement it"

        let handleProfile (args: ArgsProfile) =
            failwith "TODO | implement it"

        let handleWall (args: ArgsWall) =
            let rnd = System.Random()
            if rnd.NextDouble () < 0.69 then
                Daemon.setWallpaperDemo ()
                |> Ok
            else
                ("Ups... Try your lucky again.", 42)
                |> Error

        match args.TryGetSubCommand () with
        | None -> handleArgs args
        | Some cmd ->
            match cmd with
            | Service args -> handleService args
            | Profile args -> handleProfile args
            | Wall args-> handleWall args
            | _ ->
                "Expected to find a know Sub-Command"
                + $", instead got args: %A{cmd}."
                |> failwith
