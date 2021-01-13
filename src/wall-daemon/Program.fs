namespace Continuum.WallDaemon

open System
open Continuum.WallDaemon.CLI
open Continuum.WallDaemon.Core


module Program =

    let banner =
        let line = String.replicate 69 "#"
        [ line; "Wall Daemon by Dominik Janiec at 12021"; line; "" ]
        |> String.concat (Environment.NewLine)

    let exit =
        {| successful = 0
        ;  invalid = 13
        ;  fail = 100
        |}

    let printerr (message: string) =
        Console.Error.WriteLine(message)

    let printout (message: string) =
        Console.WriteLine(message)


    type Args = Argu.ParseResults<Arguments.Args>

    let execute (args: Args) =
        let rnd = System.Random()
        if rnd.NextDouble () > 0.5 then
            Daemon.setWallpaperDemo ()
            |> Ok
        else
            Error ("Ups...", 42)


    [<EntryPoint>]
    let main argv =

        if argv |> Array.isEmpty then
            [ banner
            ; Arguments.help ()
            ] |> List.iter printout

            exit.successful

        else
            match argv |> Arguments.parse with
            | Error msg ->
                printerr msg
                exit.invalid

            | Ok args ->
                sprintf "Got arguments: %A" args
                |> printout

                match args |> execute with
                | Error (msg, code) ->
                    printerr msg
                    exit.fail
                    + code

                | Ok () ->
                    exit.successful
