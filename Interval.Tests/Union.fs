module Interval.Tests.Union

open Expecto
open Expecto.Flip
open FsCheck

open Interval.Core
open Interval.Functions

let private checkSimpleUnion () =
    let x1 = { Value = 1; Kind = Excluded }
    let y1 = { Value = 5; Kind = Excluded }
    let x2 = { Value = 3; Kind = Included }
    let y2 = { Value = 7; Kind = Included }
    let b1 = { Start = x1; End = y1 }
    let b2 = { Start = x2; End = y2 }
    let i1 = Singleton b1
    let i2 = Singleton b2
    let result = union i1 i2
    let expected =
        Singleton
            { Start = { Value = 1; Kind = Excluded}
              End = { Value = 7; Kind = Included} }

    Expect.equal "The union should be single large interval" result expected

let private checkDisjointUnion () =
    let x1 = { Value = 1; Kind = Excluded }
    let y1 = { Value = 4; Kind = Excluded }
    let x2 = { Value = 5; Kind = Included }
    let y2 = { Value = 7; Kind = Included }
    let b1 = { Start = x1; End = y1 }
    let b2 = { Start = x2; End = y2 }
    let i1 = Singleton b1
    let i2 = Singleton b2
    let result = union i1 i2
    let expected = Union ([b1; b2] |> Set.ofList)

    Expect.equal "The union of two disjoint intervals preserves both intervals" result expected

let private checkEmptyUnion () =
    let x1 = { Value = 6; Kind = Excluded }
    let y1 = { Value = 10; Kind = Included }
    let b1 = { Start = x1; End = y1 }
    let i1 = Singleton b1
    let i2 = Empty
    let result = union i1 i2
    let expected = Singleton b1

    Expect.equal "The union of an interval with the empty interval is the first interval" result expected

let unitTests =
    let description = "[Unit Tests] Union"

    testList
        description
        [ testCase "Simple union" checkSimpleUnion
          testCase "Disjoint union" checkDisjointUnion
          testCase "Empty union" checkEmptyUnion ]