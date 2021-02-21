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



    let private todo message = failwith $"TODO | %s{message}"
    let private todoImpl what = todo $"Implement %s{what}"

    let private argsError message =
        (message, 13) |> Error

    let private asExecError message =
        (message, 31)

    let private or' value =
        function
        | None -> value
        | Some x -> x

    let private valueOr defaultVal =
        function
        | None -> ValNone
        | Some None -> ValMiss defaultVal
        | Some (Some given) -> ValGiven given


    let private handleArgs (env: IEnv) (args: ArgsApp) : ArgsResult =
        todoImpl "base args"

    let private handleService (env: IEnv) (args: ArgsService) =
        todoImpl "service"

    let private handleProfile (env: IEnv) (args: ArgsProfile) =
        todoImpl "profile"

    let private handleWall (env: IEnv) (args: ArgsWall) : ArgsResult =

        let handleFit fit = todoImpl "just fit"
        let handleNext next fit = todoImpl "just next"

        let handleBy srcId config next fit =
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

                let rnd = System.Random()
                if  rnd.NextDouble() < 0.169 then
                    ("Ups... Try your lucky again.", 42)
                    |> Error

                else
                    let fitHandler =
                        match fit with
                        | ValNone -> None
                        | ValMiss fitStyle
                        | ValGiven fitStyle ->
                            fitStyle
                            |> Executor.letStyle env
                            |> Some

                    let wallHandler =
                        let cfgItems = config |> or' []
                        src.SetWallpaper env
                            { mode = next
                            ; items = cfgItems
                            }

                    (wallHandler, fitHandler)
                    ||> Executor.makeDesktop env
                    |> Result.mapError asExecError

        let next =
            args.TryGetResult <@ Next @>
            |> valueOr wallConst.NextMode

        let fit =
            args.TryGetResult <@ WallArgs.Fit @>
            |> valueOr wallConst.WallStyle

        let source = args.TryGetResult <@ By @>
        let config = args.TryGetResult <@ By_Config @>

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
        | Some source, _ ->
            handleBy source config next fit

        | None, None ->
            match next, fit with
            | ValNone, ValGiven fit
            | ValNone, ValMiss fit -> handleFit fit
            | ValGiven next, _
            | ValMiss next, _ -> handleNext next fit

            | ValNone, ValNone ->
                "The `wall` sub-command require some parameters."
                |> argsError

        | None, Some _ ->
            "Args `--by-config` can be only provided"
                + " with source identity via `--by`."
            |> argsError


    let execute (env: IEnv) (args: ArgsApp) : ArgsResult =

        match args.TryGetSubCommand () with
        | None -> handleArgs env args
        | Some cmd ->
            match cmd with
            | Service args -> handleService env args
            | Profile args -> handleProfile env args
            | Wall args-> handleWall env args
            | _ ->
                "Expected to find a know Sub-Command"
                    + $", instead got args: %A{cmd}."
                |> failwith
