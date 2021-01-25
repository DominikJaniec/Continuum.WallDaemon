namespace Continuum.WallDaemon.Core

open System

type IEnv =
    abstract member printOut : string -> unit
    abstract member printErr : string -> unit
    abstract member debugOut : string -> unit

module Env =

    let std =

        { new IEnv with

            override a.printOut (message: string) : unit =
                Console.WriteLine(message)

            override a.printErr (message: string) : unit =
                Console.Error.WriteLine(message)

            override a.debugOut (message: string) : unit =
                a.printOut $"DEBUG: {message}"
        }
