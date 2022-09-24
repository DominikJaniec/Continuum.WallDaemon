namespace Continuum.WallDaemon

open Continuum.Common
open Continuum.WallDaemon.Core
open Continuum.WallDaemon.CLI


module Program =

    let env = Env.std

    let banner =
        let width = 69
        let line = String.replicate width "#"
        let pilar = String.replicate 3 "#"
        let header =
            let sidesSize = 2 * (pilar.Length + 1)
            "Wall Daemon by Dominik Janiec at 12022"
            |> String.center (width - sidesSize)
            |> (fun x -> $"%s{pilar} %s{x} %s{pilar}")

        [ line; header; line; "" ]
        |> String.lines

    let exit =
        {| successful = 0
        ;  invalid = 13
        ;  fail = 100
        |}

    [<EntryPoint>]
    let main argv =

        if argv |> Array.isEmpty then
            [ banner
            ; Arguments.help ()
            ] |> List.iter env.printOut

            exit.successful

        else
            $"Got arguments: %A{argv}"
            |> env.debugOut

            match argv |> Arguments.parse with
            | Error msg ->
                env.printErr msg
                exit.invalid

            | Ok args ->
                $"Parsed arguments: %A{args}"
                |> env.debugOut

                let executionResult =
                    (env, args)
                    ||> ArgsHandler.execute

                match executionResult with
                | Error (msg, code) ->
                    env.printErr msg
                    exit.fail
                    + code

                | Ok () ->
                    exit.successful
