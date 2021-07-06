namespace Continuum.Common

[<AutoOpen>]
module Core =

    let asErr = Result.Error
    let asOk = Result.Ok
    let asOk' orVal optionVal =
        match optionVal with
        | Some x -> x |> asOk
        | None -> orVal |> asErr

    let withFst tuple value =
        let second = tuple |> snd
        (value, second)

    let withSnd tuple value =
        let first = tuple |> fst
        (first, value)


[<AutoOpen>]
module Core_ToDo =

    open System

    let inline todo' details message =
        $"TODO | %s{message}" :: details
        |> String.lines
        |> NotImplementedException
        |> raise

    let inline todo message =
        todo' [] message

    let inline todoImpl what =
        todo $"Implement %s{what}"
