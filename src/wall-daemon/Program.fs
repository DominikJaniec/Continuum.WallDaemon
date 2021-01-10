open Continuum.WallDaemon.Core


[<EntryPoint>]
let main argv =
    Daemon.setWallpaperDemo ()

    0 // return an integer exit code
