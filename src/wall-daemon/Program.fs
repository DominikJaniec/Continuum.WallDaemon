namespace Continuum.WallDaemon

open System
open Continuum.WallDaemon.CLI


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

    let printErr (message: string) =
        Console.Error.WriteLine(message)

    let printOut (message: string) =
        Console.WriteLine(message)

    let debugOut (message: string) =
        // System.Diagnostics.Debug.WriteLine(message)
        printOut $"DEBUG: {message}"

    let environment =
        { new ArgsHandler.IEnv with

            override _.debugOut (message: string) : unit =
                debugOut message

            override _.printErr (message: string) : unit =
                printErr message

            override _.printOut (message: string) : unit =
                printOut message
        }


    [<EntryPoint>]
    let main argv =

        if argv |> Array.isEmpty then
            [ banner
            ; Arguments.help ()
            ] |> List.iter printOut

            exit.successful

        else
            match argv |> Arguments.parse with
            | Error msg ->
                printErr msg
                exit.invalid

            | Ok args ->
                $"Got arguments: %A{args}"
                |> debugOut

                let executionResult =
                    (environment, args)
                    ||> ArgsHandler.execute

                match executionResult with
                | Error (msg, code) ->
                    printErr msg
                    exit.fail
                    + code

                | Ok () ->
                    exit.successful
