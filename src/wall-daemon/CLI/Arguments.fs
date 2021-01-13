namespace Continuum.WallDaemon.CLI

open Argu
open Continuum.WallDaemon.Core


module Arguments =

    type ServiceArgs =
        | Status

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | Status -> "show Daemon's background service status information."


    type WallArgs =
        | Next
        | Image of path: string
        | Fit of WallStyle

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | Next -> "set next Wallpaper using current config."
                | Image _ -> "use given path to set as desktop's Wallpaper."
                | Fit _ -> "use given fit style for Wallpaper."


    type Args =
        | [<CliPrefix(CliPrefix.None)>] Service of ParseResults<ServiceArgs>
        | [<CliPrefix(CliPrefix.None)>] Wall of ParseResults<WallArgs>
        | About

        interface IArgParserTemplate with
            member arg.Usage: string =
                match arg with
                | Service _ -> "manage Daemon background service."
                | Wall _ -> "manage your desktop Wallpaper."
                | About -> "learn more about this tool."


    let private parser = ArgumentParser.Create<Args>()

    let help () =
        parser.PrintUsage()

    let parse args =
        try
            parser.ParseCommandLine(args)
            |> Ok
        with e ->
            e.Message
            |> Error
