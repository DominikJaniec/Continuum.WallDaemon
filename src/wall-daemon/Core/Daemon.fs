namespace Continuum.WallDaemon.Core
open System.IO


module Daemon =

    let setWallpaperDemo () =

        let rnd = System.Random()
        let rndItem form =
            let lim = Array.length form
            let idx = rnd.Next(lim)
            form.[idx]

        let dumpConfigAs ctxName =
            printfn "%s desktop configuration:" ctxName
            Windows.getWallpaperImagePath() |> printfn "%A"
            Windows.getWallpaperStyle() |> printfn "%A"

        let changeWallpaper style wallpaper =
            let changeResult =
                printfn "Changing wallpaper with style..."
                Ok ()
                |> Result.bind (fun () -> Windows.setWallpaperStyle style)
                |> Result.bind (fun () -> Windows.setWallpaperImage wallpaper)

            match changeResult with
            | Error msg -> failwithf "Wallpaper change failed with: %s" msg
            | Ok () -> printfn "Wallpaper changed successfully!"

        let wallpaperStyles =
            [| Fill
            ;  Fit
            ;  Stretch
            ;  Tile
            ;  Centre
            ;  Span
            |]

        let basePath = @"C:\Users\Dominik\Dropbox\Tapety"
        let candidates = Directory.GetFiles basePath

        printfn "Wallpaper DEMO"

        dumpConfigAs "Current"

        ( wallpaperStyles |> rndItem
        , candidates |> rndItem
        ) ||> changeWallpaper

        dumpConfigAs "Fresh"
