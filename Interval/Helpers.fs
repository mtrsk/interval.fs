﻿namespace Interval

module Helpers =
    open Interval.Core

    let isEmpty (i: Interval<'T>) =
        match i with
        | Empty -> true
        | _ -> false

    let isEqual (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Empty, Empty -> true
        | Singleton i1, Singleton i2 -> i1 = i2
        | Union u1, Union u2 -> u1 = u2
        | _ -> false

    let starts (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Singleton i1, Singleton i2 -> i1.Start = i2.Start
        | _ -> false

    let startedBy i1 i2 = starts i2 i1

    let finishes (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Singleton s1, Singleton s2 -> s1.End = s2.End
        | _ -> false

    let finishedBy i1 i2 = finishes i2 i1

    let precedes (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Singleton s1, Singleton s2 -> s1.Start < s2.Start
        | _ -> false

    let parseInterval<'T when 'T: equality and 'T: comparison> (b1: Boundary<'T>) (b2: Boundary<'T>) =
        if b1 < b2 then
            Singleton { Start = b1; End = b2 } |> Ok
        else
            InvalidArgument $"Argument #1 '{b1}' is greater or equal than #2 '{b2}'" |> Error

    let createInterval<'T when 'T: equality and 'T: comparison> (b1: Boundary<'T>) (b2: Boundary<'T>) =
        if b1 < b2 then Singleton { Start = b1; End = b2 } else Empty