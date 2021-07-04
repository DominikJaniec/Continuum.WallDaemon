namespace Continuum.Common

module Async =

    let sleepOneSecond =
        System.TimeSpan.FromSeconds 1.0
        |> Async.Sleep

    let map mapping source =
        async {
            let! x = source
            return x
            |> mapping
        }

    let StartAsync a =
        async {
            let! r = a
            return r
        }

    let SequentialList computations =
        async {
            let! xs =
                computations
                |> Async.Sequential

            return xs
                |> List.ofArray
        }
