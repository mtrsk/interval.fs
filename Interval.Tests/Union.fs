module Interval.Tests.Union

open Expecto
open Expecto.Flip
open FsCheck

open Interval.Core
open Interval.Functions

let checkSimpleUnion () =
    let x1 = { Value = 1; Kind = Excluded }
    let y1 = { Value = 5; Kind = Excluded }
    let x2 = { Value = 3; Kind = Included }
    let y2 = { Value = 7; Kind = Included }
    let i1 = Interval { Start = x1; End = y1 }
    let i2 = Interval { Start = x2; End = y2 }
    let result = union i1 i2
    let expected =
        Interval
            { Start = { Value = 1; Kind = Excluded}
              End = { Value = 7; Kind = Included} }

    Expect.equal "The union should be single large interval" result (Choice1Of2 expected)

let checkDisjointUnion () =
    let x1 = { Value = 1; Kind = Excluded }
    let y1 = { Value = 4; Kind = Excluded }
    let x2 = { Value = 5; Kind = Included }
    let y2 = { Value = 7; Kind = Included }
    let i1 = Interval { Start = x1; End = y1 }
    let i2 = Interval { Start = x2; End = y2 }
    let result = union i1 i2
    let expected = Choice2Of2 <| Union (i1,i2)

    Expect.equal "The union of two disjoint intervals preserves both intervals" result expected

let checkEmptyUnion () =
    let x1 = { Value = 6; Kind = Excluded }
    let y1 = { Value = 10; Kind = Included }
    let i1 = Interval { Start = x1; End = y1 }
    let i2 = Empty
    let result = union i1 i2
    let expected = Choice1Of2 i1

    Expect.equal "The union of an interval with the empty interval is the first interval" result expected

let unitTests =
    let description = "[Unit Tests] Union"

    testList
        description
        [ testCase "Simple union" checkSimpleUnion
          testCase "Disjoint union" checkDisjointUnion
          testCase "Empty union" checkEmptyUnion ]