namespace Continuum.Common

module String =

    let toLower (str: string) =
        str.ToLowerInvariant()

    let toUpper (str: string) =
        str.ToUpperInvariant()

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
