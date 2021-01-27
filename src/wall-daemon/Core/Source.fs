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


    type CfgValue<'a> =
        | ValGiven of 'a
        | ValMiss of 'a
        | ValNone

    type WallConfig =
        { mode: CfgValue<NextMode>
        ; style: CfgValue<WallStyle>
        ; items: string list
        }

    type ISource =
        abstract member Identity: Identity with get
        abstract member SetWallpaper: env: IEnv -> config: WallConfig -> unit


    type IProvider =
        abstract member Identity: Identity with get
        abstract member Instance: unit -> ISource
