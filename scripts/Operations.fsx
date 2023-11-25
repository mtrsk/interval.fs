#r "../Interval/obj/Debug/net6.0/Interval.dll"

open Interval.Core
open Interval.Helpers
open Interval.Functions

let x1 = { Value = 1; Kind = Excluded }
let y1 = { Value = 5; Kind = Excluded }

let x2 = { Value = 3; Kind = Included}
let y2 = { Value = 7; Kind = Included}

let x3 = { Value = 6; Kind = Excluded}
let y3 = { Value = 8; Kind = Included}

let x4 = { Value = 6; Kind = Excluded}
let y4 = { Value = 10; Kind = Included}

let i1 = Interval { Start = x1; End = y1 }
let i2 = Interval { Start = x2; End = y2 }
let i3 = Interval { Start = x3; End = y3 }
let i4 = Interval { Start = x4; End = y4 }

let intersection12 = intersection i1 i2
intersection12

let intersection23 = intersection i2 i3
intersection23

let intersection13 = intersection i1 i3
intersection13

let union12 = union i1 i2
let union23 = union i2 i3
let union13 = union i1 i3

let r1 = relate i1 i2
printfn $"{i1} is {r1} to {i2}"
let r2 = relate i2 i3
printfn $"{i2} is {r2} to {i3}"
let r3 = relate i1 i3
printfn $"{i1} is {r3} to {i3}"
let r4 = relate i3 i4
printfn $"{i3} is {r4} to {i4}"