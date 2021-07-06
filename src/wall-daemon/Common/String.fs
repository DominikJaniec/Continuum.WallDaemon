namespace Continuum.Common

open System.Text.RegularExpressions
open System


module String =

    let nl = "\n"

    let asSafe (str: string) =
        match str |> isNull with
        | true -> None
        | _ -> Some str

    let toLower (str: string) =
        str.ToLowerInvariant()

    let toUpper (str: string) =
        str.ToUpperInvariant()

    let hasLines (str: string) =
        // TODO: less exploitable
        str.Contains(nl)

    let lines items =
        String.concat "\n" items

    let padding width =
        String.replicate width " "

    let center width (str: string) =
        match width <= str.Length with
        | true -> str
        | _ ->
            let margin = width - str.Length
            let postfix = margin / 2
            let prefix = margin - postfix

            String.concat ""
                [ prefix |> padding
                ; str
                ; postfix |> padding
                ]
