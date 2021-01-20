namespace Continuum.WallDaemon.Core

open Continuum.Common


type NextMode =
    | Designed
    | Random
    | Shuffle
    | Sequential


module Source =

    type Identity =
        | Id of string
    module Identity =
        let from (str: string) =
            str.Trim()
            |> String.toLower
            |> Id

    type ISource =
        abstract member Identity: Identity with get

    type IProvider =
        abstract member Identity: Identity with get
        abstract member Instance: unit -> ISource
