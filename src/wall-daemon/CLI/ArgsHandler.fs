namespace Continuum.WallDaemon.CLI

open Argu
open Arguments
open Continuum.Common
open Continuum.WallDaemon.Core
open Continuum.WallDaemon.Core.Source
open Continuum.WallDaemon.Sources
open Continuum.WallDaemon.Environment


module ArgsHandler =

    type ArgsApp = ParseResults<Args>
    type ArgsWall = ParseResults<WallArgs>
    type ArgsService = ParseResults<ServiceArgs>
    type ArgsProfile = ParseResults<ProfileArgs>
    type ArgsResult = Result<unit, string * int>


    let private argsError message =
        (message, 13) |> Error

    let private argsError' messageLines =
        messageLines |> String.lines |> argsError

    let private execError message =
        (message, 31)

    let private or' value =
        function
        | None -> value
        | Some x -> x

    let private valueOr defaultVal =
        function
        | None -> ValNone
        | Some None -> ValDef defaultVal
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
                    |> List.map (sprintf "  * %s")

                [ "Cannot find requested source provider"
                    + $" by '%s{srcId}' identity."
                ; "Known providers' type-name identities:"
                ; yield! knownProviders
                ]
                |> argsError'

            | Some src ->
                "Found source providers' by type-name"
                    + $" with identity: %A{src.Identity}"
                |> env.printOut

                let config  = config |> or' []
                $"Parsing config item from: %A{config}"
                |> env.debugOut

                match config |> src.Parse with
                | Error err ->
                    [ $"Source could not parse given config: %A{config}"
                    ; "Parse result: " + err
                    ]
                    |> argsError'

                | Ok configItem ->
                    let wallStylesSource =
                        match fit with
                        | ValNone ->
                            Executor.getStyles env

                        | ValDef fitStyle
                        | ValGiven fitStyle ->
                            WallTarget.allOf env
                            |> List.map (fun t -> (fitStyle, t))
                            |> Executor.setStyles env

                    let wallpapersSource styles =
                        let config =
                            { mode = next
                            ; styles = styles
                            ; item = configItem
                            }

                        [ $"Calling ISource.SetWallpaper of '%A{src.Identity}' instance with config:"
                        ; $"%A{config}"
                        ]
                        |> (String.lines >> env.debugOut)

                        config
                        |> src.Wallpapers env

                    (wallStylesSource, wallpapersSource)
                    ||> Executor.makeDesktopSettup env
                    |> Result.mapError execError

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
            | ValNone, ValDef fit -> handleFit fit
            | ValGiven next, _
            | ValDef next, _ -> handleNext next fit

            | ValNone, ValNone ->
                "The `wall` sub-command require some parameters."
                |> argsError

        | None, Some _ ->
            "Args `--by-config` can be only provided"
                + " with source identity via `--by`."
            |> argsError


    let execute (env: IEnv) (args: ArgsApp) : ArgsResult =

        let rnd = System.Random()
        if  rnd.NextDouble() < 0.069 then
            ("Ups... Try your lucky again.", 42)
            |> Error

        else
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
