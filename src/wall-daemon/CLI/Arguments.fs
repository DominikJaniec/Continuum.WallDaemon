namespace Continuum.WallDaemon.CLI

open System
open Argu
open Continuum.WallDaemon.Core
open Continuum.WallDaemon.Sources


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


    let wallConst =
        {| NextMode = NextMode.Designed
        ;  WallStyle = WallStyle.Fit
        ;  ByProviders = Index.identities
        |}

    type WallArgs =
        | Next of NextMode option
        | Fit of WallStyle option
        | By of source_identity: string
        | By_Config of items: string list

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | Next _ ->
                    "set next Wallpaper using mode for current config."
                    + defaultsToCase wallConst.NextMode
                | Fit _ ->
                    "use given image fit style for desktop Wallpaper."
                    + defaultsToCase wallConst.WallStyle
                | By _ ->
                    "identity of current Wallpapers source-provider service."
                    + "\nIt is unique profile identity-tag or provider type-name."
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

    let private spread (args: string array) =
        let isSingleton =
            args |> Array.length = 1

        match isSingleton with
        | false -> args
        | true ->
            let arg = args.[0]
            match arg.Contains ' ' with
            | false -> args
            | true ->
                arg.Split(' ', StringSplitOptions.RemoveEmptyEntries)

    let private parser = ArgumentParser.Create<Args>()


    let help () =
        parser.PrintUsage()

    let parse args =
        try
            let args = spread args
            parser.ParseCommandLine(args)
            |> Ok

        with
        | :? ArguParseException as arguEx ->
            arguEx.Message
            |> Error

        | ex ->
            let msg = $"Parser encounter a unexpected problem:"
            InvalidOperationException(msg, ex)
            |> raise
