namespace Interval.Tests

module Generators =
    open FsCheck
    open Interval.Core

    let generateBoundaryKind () =
        gen {
            let! choice = Gen.oneof [ Gen.constant BoundaryKind.Excluded; Gen.constant BoundaryKind.Included ]
            return choice
        }

    let generateBoundary () =
        gen {
            let! k = generateBoundaryKind ()
            let! n = Arb.generate<int>
            let boundary = { Value = n; Kind = k }
            return boundary
        }

    let rec generateBoundedInterval () =
        gen {
            let! b1 = generateBoundary ()
            let! b2 = generateBoundary ()

            let interval =
                if b1 < b2 then
                    { Start = b1; End = b2 }
                else
                    { Start = b2; End = b1 }

            return interval
        }