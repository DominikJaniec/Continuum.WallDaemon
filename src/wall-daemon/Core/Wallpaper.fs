namespace Continuum.WallDaemon.Core


type WallStyle =
    | Fill
    | Fit
    | Stretch
    | Tile
    | Centre
    | Span

type Wallpaper =
    | ImageFile of path: string


type WallTarget =
    Desktop * Display

module WallTarget =

    let allOf (env: IEnv): WallTarget list =
        let asTarget (desktop: Desktop, displays: Display list) =
            displays |> Seq.map (fun d -> (desktop, d))

        Settup.allDisplaysOf env
        |> Seq.collect asTarget
        |> List.ofSeq

    let stringify (target: WallTarget) =
        let (desktop, display) = target
        let order = Order.its display.order
        $"display %d{order}@%s{desktop.nickname}"


type TargetedWallStyle =
    WallStyle * WallTarget

type TargetedWallpaper =
    Wallpaper * WallTarget

type Wallpapers =
    TargetedWallpaper list

type WallStyles =
    TargetedWallStyle list
