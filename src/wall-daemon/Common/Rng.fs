namespace Continuum.Common

type Rng = System.Random

module Rng =

    let randomItem (rng: Rng) items =
        let len = Array.length items
        let i = rng.Next (0, len)
        items.[i]
