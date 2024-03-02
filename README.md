# interval.fs

[![Build](https://github.com/mtrsk/interval.fs/actions/workflows/build.yml/badge.svg)](https://github.com/mtrsk/interval.fs/actions/workflows/build.yml)
![Nuget](https://img.shields.io/nuget/v/interval.fs)

An implementation of Allen's Interval Algebra, for .Net

## Usage

### Boundaries and Intervals

This document uses the common interval notation. The examples here all use the `Integer` type, but this library is designed to support anything that implements `IEquatable` and `IComparable`.

<center>

|     Set       |   Notation    |
| :---          |     :---:     |
| $`\{ x \in \mathbb{Z} \mid 1 \lt x \lt 5 \}`$ | $(1,5)$ |
| $`\{ x \in \mathbb{Z} \mid 3 \leq x \leq 7 \}`$ | $[3,7]$ |
| $`\{ x \in \mathbb{Z} \mid 6 \lt x \lt 8 \}`$ | $(6,8)$ |
| $`\{ x \in \mathbb{Z} \mid 6 \lt x \leq 10 \}`$ | $(6,10]$ |
| $`\{ x \in \mathbb{Z} \mid 11 \leq x \leq 12 \}`$ | $[11,12]$ |

</center>

Setting up open/closed boundaries and their respective intervals:

```fsharp
open Interval.Core
open Interval.Functions

// (1,5)
let x1 = { Value = 1; Kind = Excluded }
let y1 = { Value = 5; Kind = Excluded }
let b1 = { Start = x1; End = y1 }

// [3,7]
let x2 = { Value = 3; Kind = Included }
let y2 = { Value = 7; Kind = Included }
let b2 = { Start = x2; End = y2 }

// [6,8)
let x3 = { Value = 6; Kind = Excluded }
let y3 = { Value = 8; Kind = Included }
let b3 = { Start = x3; End = y3 }

// (6,10]
let x4 = { Value = 6; Kind = Excluded }
let y4 = { Value = 10; Kind = Included }
let b4 = { Start = x4; End = y4 }

// [11,12]
let x5 = { Value = 11; Kind = Included }
let y5 = { Value = 12; Kind = Included }
let b5 = { Start = x5; End = y5 }
```
A `Singleton` is a set with only one interval:
```fsharp
// { (1,5) }
let i1 = Singleton b1
// { [3,7] }
let i2 = Singleton b2
// { [6,8) }
let i3 = Singleton b3
// { (6,10] }
let i4 = Singleton b4
// { [11,12] }
let i5 = Singleton b5
```

### Operations

#### Intersection

```fsharp
// { (1,5) } ∩ { [3,7] }
intersection i1 i2
// Generates...
Singleton {
    Start = { Value = 3; Kind = Included }
    End = { Value = 5; Kind = Excluded }
}
```

```fsharp
// { [3,7] } ∩ { (6,8] }
intersection i2 i3
// Generates...
Singleton {
    Start = { Value = 6; Kind = Excluded }
    End = { Value = 7; Kind = Included }
}
```

an intersection between two intervals may also return an `Empty` result.
```fsharp
// { [1,5] } ∩ { (6,8] }
intersection i1 i3
// Generates...
Empty
```

#### Union

One can also take two singletons and compute their union:
```fsharp
// { (1,5) } ∪ { [3,7] }
union i1 i2
// Generates
Singleton {
    Start = { Value = 1; Kind = Excluded }
    End = { Value = 7; Kind = Included } }
}
```
or:
```fsharp
// { [3,7] } ∪ { (6,8] }
union i2 i3
// Generates
Singleton {
    Start = { Value = 3; Kind = Included }
    End = { Value = 8; Kind = Included }
}
```
The result of a disjoint `union` is not a singleton:
```fsharp
// { (1,5) } ∪ { (6,8] }
union i1 i3
// Generates
Union (
    set [ { Start = { Value = 1; Kind = Excluded }
            End = { Value = 5; Kind = Excluded } }
          { Start = { Value = 6; Kind = Excluded }
            End = { Value = 8; Kind = Included } } ]
)
```

#### Relationships

```fsharp
// { (1,5) } Overlaps { [3,7] }
relate i1 i2
// { [3,7] } Overlaps { (6,8] }
relate i2 i3
// { [1,5] } Before { (6,8] } 
relate i1 i3
// { (6,8] } Starts { (6,10] }
relate i3 i4
// { [11,12] } After { (6,10] }
relate i5 i4
```

#### Merge

```fsharp
// Merging (1,5) [3,7] [6,8) (6,10] [11,12] 
let boundaries = [ b1; b2; b3; b4; b5 ]
merge(boundaries)
// Outputs
Union (
    set [{ Start = { Value = 1; Kind = Excluded }
           End = { Value = 10; Kind = Included } }
         { Start = { Value = 11; Kind = Included }
           End = { Value = 12; Kind = Included } }]
)
```

## TODO

### Acknoledgements


