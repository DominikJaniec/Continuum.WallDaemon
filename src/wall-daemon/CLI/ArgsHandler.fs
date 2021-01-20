namespace Continuum.WallDaemon.CLI

open Argu
open Arguments
open Continuum.Common
open Continuum.WallDaemon.Core
open Continuum.WallDaemon.Core.Source
open Continuum.WallDaemon.Sources


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


    let private todo message = failwith $"TODO | %s{message}"
    let private todoImpl what = todo $"Implement %s{what}"

    let private argsError message =
        (message, 13) |> Error

    let private or' value = function
        | None -> value
        | Some x -> x


    let execute (env: IEnv) (args: ArgsApp) : ArgsResult =

        let handleArgs (args: ArgsApp) : ArgsResult =
            todoImpl "base args"

        let handleService (args: ArgsService) =
            todoImpl "service"

        let handleProfile (args: ArgsProfile) =
            todoImpl "profile"

        let handleWall (args: ArgsWall) =
            let next =
                args.TryGetResult <@ Next @>
                |> Option.map (or' wallConst.NextMode)

            let fit =
                args.TryGetResult <@ WallArgs.Fit @>
                |> Option.map (or' wallConst.WallStyle)

            let source = args.TryGetResult <@ By @>
            let config = args.TryGetResult <@ By_Config @>

            let handleJustNext next = todoImpl "just next"
            let handleJustFit fit = todoImpl "just fit"

            let handleBy srcId =
                let byIdentity =
                    Identity.from srcId
                    |> Index.sourceBy

                match byIdentity with
                | None ->
                    let knownProviders =
                        Index.identities
                        |> List.map (sprintf "  %s")

                    [ "Cannot find requested source provider"
                        + $" by '%s{srcId}' identity."
                    ; "Known providers' type-name identities:"
                    ; yield! knownProviders
                    ]
                    |> (String.lines >> argsError)

                | Some src ->
                    "Found source providers' by type-name"
                    + $" with %A{src.Identity} identity"
                    |> env.printOut

                    if next |> Option.isSome then todoImpl "by with some next"
                    if fit |> Option.isSome then todoImpl "by with some fit"
                    let cfgItems = config |> or' []

                    let rnd = System.Random()
                    if rnd.NextDouble () < 0.69 then
                        Daemon.setWallpaperDemo ()
                        |> Ok
                    else
                        ("Ups... Try your lucky again.", 42)
                        |> Error

            [ "Handling `Wall` sub-command with:"
            ; sprintf "%A"
                {| next = next
                ;  fit = fit
                ;  source = source
                ;  config = config
                |}
            ]
            |> (String.lines >> env.debugOut)

            match source, config with
            | None, None ->
                match next, fit with
                | None, Some fit -> handleJustFit fit
                | Some next, _ -> handleJustNext next
                | None, None ->
                    "The `wall` sub-command require some parameters"
                    |> argsError

            | Some source, _ -> handleBy source
            | None, Some _ ->
                "Args `--by-config` can be only provided"
                + " with source identity via `--by`."
                |> argsError

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
