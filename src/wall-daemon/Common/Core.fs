namespace Continuum.Common

[<AutoOpen>]
module Core =

    let asErr = Result.Error
    let asOk = Result.Ok
    let asOk' orVal optionVal =
        match optionVal with
        | Some x -> x |> asOk
        | None -> orVal |> asErr
