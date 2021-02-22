namespace Continuum.WallDaemon.Core

open Continuum.Common

type OWallStyle =
    Order * WallStyle
type OWall =
    Order * Wallpaper
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
        ; styles: OWallStyle list
        ; parameters: string list
        }

    type ISource =
        abstract member Identity: Identity with get
        abstract member SetWallpaper:
            env: IEnv -> config: WallConfig
            -> Async<OWall list>


    type IProvider =
        abstract member Identity: Identity with get
        abstract member Instance: unit -> ISource
