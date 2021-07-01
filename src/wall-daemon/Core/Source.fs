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
        | ValDef of 'a
        | ValNone

    type IConfigItem =
        interface end

    type WallConfig =
        { mode: CfgValue<NextMode>
        ; styles: OWallStyle list
        ; item: IConfigItem
        }

    type ISource =
        abstract member Identity: Identity with get

        abstract member Parse: config: string list
            -> Result<IConfigItem, string>

        abstract member SetWallpaper: env: IEnv
            -> config: WallConfig
            -> Async<OWall list>


    type IProvider =
        abstract member Identity: Identity with get
        abstract member Instance: unit -> ISource
