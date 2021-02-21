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
