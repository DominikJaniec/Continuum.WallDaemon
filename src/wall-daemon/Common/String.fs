namespace Continuum.Common

module String =

    let toLower (str: string) =
        str.ToLowerInvariant()

    let toUpper (str: string) =
        str.ToUpperInvariant()

    let lines items =
        String.concat "\n" items
