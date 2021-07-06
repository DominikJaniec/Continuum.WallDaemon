namespace Continuum.WallDaemon.Core

open System
open Continuum.Common


[<Measure>]
type deg

type Width =
    | W of uint

type Height =
    | H of uint

type Order =
    | No of uint

module Order =
    let its value =
        match value with
        | No n -> n

type Display =
    { resolution: Width * Height
    ; rotation: int<deg>
    ; order: Order
    }

type Desktop =
    { displays: Display list
    ; nickname: string
    ; order: Order
    }

[<RequireQualifiedAccess>]
type SettupConfig =
    | Gird of Width * Height
    | Horizon of uint
    | Circle of uint
    | Tower of uint

type Settup =
    { desktops: Desktop list
    ; config: SettupConfig
    }


type IEnv =
    abstract member settup: Settup with get
    abstract member printOut : string -> unit
    abstract member printErr : string -> unit
    abstract member debugOut : string -> unit

module Settup =

    let private orderOfDesktop (desktop: Desktop) =
        desktop.order
        |> Order.its

    let private orderOfDisplay (display: Display) =
        display.order
        |> Order.its

    let private withDisplays (desktop: Desktop) =
        let displays =
            desktop.displays
            |> List.sortBy orderOfDisplay

        (desktop, displays)

    let allDisplaysOf (env: IEnv) =
        env.settup.desktops
        |> Seq.sortBy orderOfDesktop
        |> Seq.map withDisplays
        |> List.ofSeq


module Env =

    let private timestamped' header message =
        let timestamp =
            DateTime.Now.ToString("HH:mm:ss.f")

        let prefix = $" %s{header}"

        let separator =
            match message |> String.hasLines with
            | true -> "/" + String.nl
            | _ -> "| "

        String.concat ""
            [ timestamp
            ; prefix
            ; separator
            ; message
            ]

    let private timestamped message =
        timestamped' "" message


    let std =

        { new IEnv with

            override a.settup with get () : Settup =
                let display: Display =
                    { resolution = (W 2560u, H 1080u)
                    ; rotation = 0<deg>
                    ; order = No 1u
                    }

                let desktop =
                    { displays = [ display ]
                    ; nickname = "primary"
                    ; order = No 1u
                    }

                let config = SettupConfig.Horizon 1u

                // TODO: get displays information from host
                { desktops = [ desktop ]
                ; config = config
                }

            override a.printOut (message: string) : unit =
                timestamped message
                |> Console.WriteLine

            override a.printErr (message: string) : unit =
                timestamped message
                |> Console.Error.WriteLine

            override a.debugOut (message: string) : unit =
                timestamped' "DEBUG" message
                |> Console.WriteLine
        }
