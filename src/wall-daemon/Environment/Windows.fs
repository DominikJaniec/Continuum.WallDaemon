namespace Continuum.WallDaemon.Environment

open System
open System.Runtime.InteropServices
open Microsoft.Win32

open Continuum.WallDaemon.Core


module Windows =

    module private Winuser_h =

        [<DllImport("User32", CharSet = CharSet.Auto)>]
        extern bool SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);

        [<Literal>]
        /// Writes the new system-wide parameter setting to the user profile.
        /// Source: http://www.pinvoke.net/default.aspx/Enums/SPIF.html
        let SPIF_UPDATEINIFILE = 0x01

        [<Literal>]
        /// Broadcasts the WM_SETTINGCHANGE message after updating the user profile.
        /// Source: http://www.pinvoke.net/default.aspx/Enums/SPIF.html
        let SPIF_SENDCHANGE = 0x02

        [<Literal>]
        /// Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa#parameters
        let SPI_SETDESKWALLPAPER = 0x14

    open Winuser_h

    type KeyAccess =
        | Writable
        | ReadOnly

    module private RegKey =

        let tryGetValue (name: string) (regKey: RegistryKey) =
            let value = regKey.GetValue name
            match value |> isNull with
            | false -> Some value
            | true -> None

        let tryGetString (name: string) (regKey: RegistryKey) =
            let extract value =
                let str = value.ToString().Trim()
                match str |> String.IsNullOrWhiteSpace with
                | false -> Some str
                | true -> None

            regKey
            |> tryGetValue name
            |> Option.bind extract

        let openDesktopRegistry access =
            let writable =
                match access with
                | Writable -> true
                | ReadOnly -> false

            let path = @"Control Panel\Desktop"

            let regKey =
                Registry.CurrentUser
                    .OpenSubKey(path, writable)

            if regKey |> isNull  then
                (path, if writable then "writable" else "read-only")
                ||> failwithf "Cannot open HKCU '%s' as %s Registry key."

            regKey


    let private regKeys =
        {| WallPaper = "WallPaper"
        ;  TileWallpaper = "TileWallpaper"
        ;  WallpaperStyle = "WallpaperStyle"
        |}

    type private StyleRegValue =
        { TileWallpaper: string
        ; WallpaperStyle: string
        }

    let private styleMap =
        [ WallStyle.Fill,
            { TileWallpaper = "0"
            ; WallpaperStyle = "10"
            }
        ; WallStyle.Fit,
            { TileWallpaper = "0"
            ; WallpaperStyle = "6"
            }
        ; WallStyle.Stretch,
            { TileWallpaper = "0"
            ; WallpaperStyle = "2"
            }
        ; WallStyle.Tile,
            { TileWallpaper = "1"
            ; WallpaperStyle = "0"
            }
        ; WallStyle.Centre,
            { TileWallpaper = "0"
            ; WallpaperStyle = "0"
            }
        ; WallStyle.Span,
            { TileWallpaper = "0"
            ; WallpaperStyle = "22"
            }
        ]

    let private readWallpaperPath =
        RegKey.tryGetString regKeys.WallPaper

    let private errorUnknownConfig configValue =
        $"Unknown %A{configValue} configuration"
        |> Error


    type DisplayId =
        | DID of uint

    let getWallpaperImagePath () =
        use key = RegKey.openDesktopRegistry ReadOnly
        key |> readWallpaperPath

    let setWallpaperImage (path: string) =
        let uiParam_default = 0x00
        let fWinIni_value =
            SPIF_UPDATEINIFILE ||| SPIF_SENDCHANGE

        let success =
            SystemParametersInfo
                ( SPI_SETDESKWALLPAPER
                , uiParam_default
                , path
                , fWinIni_value
                )

        match success with
        | true -> Ok ()
        | false ->
            nameof SystemParametersInfo
            |> sprintf "Call to '%s' was unsuccessful."
            |> Error


    let getWallpaperStyle () =
        let extract name =
            RegKey.tryGetString name
            >> Option.defaultValue "-1"

        use key = RegKey.openDesktopRegistry ReadOnly
        let tw = key |> extract regKeys.TileWallpaper
        let ws = key |> extract regKeys.WallpaperStyle

        let item =
            { TileWallpaper = tw
            ; WallpaperStyle = ws
            }

        let isMatch (_, known) =
            known = item

        match styleMap |> List.where isMatch with
        | [ (style, _) ] -> Ok style
        | _ -> errorUnknownConfig item

    let setWallpaperStyle (style: WallStyle) =
        let styleValue =
            let asGiven (s, value) =
                match s = style with
                | true -> Some value
                | false -> None

            match styleMap |> List.choose asGiven with
            | [ value ] -> Ok value
            | _ -> errorUnknownConfig style

        let setStyle (style: StyleRegValue) =
            use key = RegKey.openDesktopRegistry Writable
            key.SetValue(regKeys.TileWallpaper, style.TileWallpaper)
            key.SetValue(regKeys.WallpaperStyle, style.WallpaperStyle)
            Ok ()

        styleValue
        |> Result.bind setStyle
