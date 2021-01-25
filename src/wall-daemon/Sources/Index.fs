namespace Continuum.WallDaemon.Sources

open Continuum.WallDaemon.Core.Source


module Index =

    let private standardProviders =
        [ Catalog.provider ()
        // ; FilePath.provider ()
        ]


    let availableProviders =
        standardProviders
        // TODO: Load dynamics
        // TODO: Ensure unique


    let identities =
        let getId (p: IProvider) =
            match p.Identity with
            | Id identity -> identity

        availableProviders
        |> List.map getId


    let sourceBy identity =
        let instance (p: IProvider) =
            match p.Identity = identity with
            | true -> p.Instance() |> Some
            | false -> None

        availableProviders
        |> List.tryPick instance
