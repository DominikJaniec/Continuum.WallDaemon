namespace Continuum.WallDaemon.Core

open System


type Order =
    | No of uint
type Display =
    { resolution: uint * uint
    ; order: Order
    }

type IEnv =
    abstract member displays: Display list with get
    abstract member printOut : string -> unit
    abstract member printErr : string -> unit
    abstract member debugOut : string -> unit

module Env =

    let private timestamped message =
        DateTime.Now.ToString("HH:mm:ss.f")
        + $"| %s{message}"

    let std =

        { new IEnv with

            override a.displays with get () : Display list =
                // TODO: get displays information from host
                [ { order = No 1u; resolution = (3u, 3u) } ]

            override a.printOut (message: string) : unit =
                timestamped message
                |> Console.WriteLine

            override a.printErr (message: string) : unit =
                timestamped message
                |> Console.Error.WriteLine

            override a.debugOut (message: string) : unit =
                a.printOut $"DEBUG: {message}"
        }
