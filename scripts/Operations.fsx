#r "../Interval/obj/Debug/net6.0/Interval.dll"

open Interval.Core
open Interval.Helpers
open Interval.Functions

// [1,5]
let x1 = { Value = 1; Kind = Excluded }
let y1 = { Value = 5; Kind = Excluded }
let b1 = { Start = x1; End = y1 }

// [3,7]
let x2 = { Value = 3; Kind = Included}
let y2 = { Value = 7; Kind = Included}
let b2 = { Start = x2; End = y2 }

// [6,8)
let x3 = { Value = 6; Kind = Excluded}
let y3 = { Value = 8; Kind = Included}
let b3 = { Start = x3; End = y3 }

// (6,10]
let x4 = { Value = 6; Kind = Excluded}
let y4 = { Value = 10; Kind = Included}
let b4 = { Start = x4; End = y4 }

// [11,12]
let x5 = { Value = 11; Kind = Included}
let y5 = { Value = 12; Kind = Included}
let b5 = { Start = x5; End = y5 }

// { [1,5] }
let i1 = Singleton b1
// { [3,7] }
let i2 = Singleton b2
// { [6,8) }
let i3 = Singleton b3
// { (6,10] }
let i4 = Singleton b4
// { [11,12] }
let i5 = Singleton b5

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

let boundaries = [ b1; b2; b3; b4; b5 ]

let forest = generateForest(boundaries)
forest

let merger = merge(boundaries)
merger
