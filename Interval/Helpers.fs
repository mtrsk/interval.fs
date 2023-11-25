namespace Interval

module Helpers =
    open Interval.Core

    let isEmpty (i: Interval<'T>) =
        match i with
        | Empty -> true
        | _ -> false

    let isEqual (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Empty, Empty -> true
        | Interval i1, Interval i2 -> i1 = i2
        | _ -> false

    let starts (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Interval i1, Interval i2 -> i1.Start = i2.Start
        | _ -> false

    let startedBy i1 i2 = starts i2 i1

    let finishes (i1: Interval<'T>) (i2: Interval<'T>) =
        match i1, i2 with
        | Interval ii1, Interval ii2 -> ii1.End = ii2.End
        | _ -> false

    let finishedBy i1 i2 = finishes i2 i1

    let precedes (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Interval i1, Interval i2 -> i1.Start < i2.Start
        | _ -> false

    let parseInterval<'T when 'T: equality and 'T: comparison> (b1: Boundary<'T>) (b2: Boundary<'T>) =
        if b1 < b2 then
            Interval { Start = b1; End = b2 } |> Ok
        else
            InvalidArgument $"Argument #1 '{b1}' is greater or equal than #2 '{b2}'" |> Error

    let createInterval<'T when 'T: equality and 'T: comparison> (b1: Boundary<'T>) (b2: Boundary<'T>) =
        if b1 < b2 then Interval { Start = b1; End = b2 } else Empty

    let (|GetInterval|_|) (choice: Choice<Interval<'T>, Union<'T>>) =
        match choice with
        | Choice1Of2 interval -> Some interval
        | Choice2Of2 _ -> None

    let (|GetUnion|_|) (choice: Choice<Interval<'T>, Union<'T>>) =
        match choice with
        | Choice1Of2 _ -> None
        | Choice2Of2 union -> Some union