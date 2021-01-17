namespace Continuum.WallDaemon.CLI

open Argu
open Continuum.WallDaemon.Core


module Arguments =

    let private defaultsTo value =
        $"\n^ Defaults to <%s{value}> when not present."

    let private defaultsToCase item =
        $"%A{item}".ToLowerInvariant() |> defaultsTo

    let private possibleItemsAs name items =
        let values =
            items
            |> String.concat "|"
            |> sprintf "<%s>"

        $"\n^ Possible values of %s{name}: {values}"


    type ServiceArgs =
        | Status

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | Status -> "show Daemon's background service status information."


    type ProfileArgs =
        | List_Tags

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | List_Tags -> "list known identity tags."


    type JumpMode =
        | Designed
        | Random
        | Shuffle
        | Sequential

    let wallConst =
        {| WallStyle = WallStyle.Fit
        ;  JumpMode = JumpMode.Designed
        ;  ByProviders = [ "catalog"; "file-path" ]
        |}

    type WallArgs =
        | Fit of WallStyle option
        | Next of JumpMode option
        | By of source_identity: string
        | By_Config of items: string list

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | Fit _ ->
                    "use given image fit style for desktop Wallpaper."
                    + defaultsToCase wallConst.WallStyle
                | Next _ ->
                    "set next Wallpaper using mode for current config."
                    + defaultsToCase wallConst.JumpMode
                | By _ ->
                    "identity of current Wallpapers source service."
                    + "\nIt is unique profile identity-tag of provider type-name."
                    + " An identity is build to match: <unique-name@provider> tag format."
                    + " Depending of provider, an additional configuration could be required."
                    + possibleItemsAs "providers" wallConst.ByProviders
                | By_Config _ ->
                    "provides an additional configuration for given source."


    type Args =
        | [<CliPrefix(CliPrefix.None)>] Service of ParseResults<ServiceArgs>
        | [<CliPrefix(CliPrefix.None)>] Profile of ParseResults<ProfileArgs>
        | [<CliPrefix(CliPrefix.None)>] Wall of ParseResults<WallArgs>
        | About

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | Service _ -> "manage Daemon background service."
                | Profile _ -> "manage Daemon source profiles."
                | Wall _ -> "manage your desktop Wallpaper."
                | About -> "learn more about this tool."


    let private parser = ArgumentParser.Create<Args>()

    let help () =
        parser.PrintUsage()

    let parse args =
        try
            parser.ParseCommandLine(args)
            |> Ok

        with
        | :? Argu.ArguParseException as arguEx ->
            arguEx.Message
            |> Error

        | ex ->
            let msg = $"Parser encounter a unexpected problem:"
            System.InvalidOperationException(msg, ex)
            |> raise
