module Interval.Tests.Intersection

open Expecto
open Expecto.Flip
open FsCheck

open Interval.Core
open Interval.Functions

let private checkSimpleIntersection () =
    let x1 = { Value = 1; Kind = Excluded }
    let y1 = { Value = 5; Kind = Excluded }
    let x2 = { Value = 3; Kind = Included }
    let y2 = { Value = 7; Kind = Included }
    let i1 = Singleton { Start = x1; End = y1 }
    let i2 = Singleton { Start = x2; End = y2 }
    let result = intersection i1 i2

    let expected =
        Singleton
            { Start = { Value = 3; Kind = Included }
              End = { Value = 5; Kind = Excluded } }

    Expect.equal "There should be a non-empty intersection" result expected

let private checkSubIntervalIntersection () =
    let x1 = { Value = 6; Kind = Excluded }
    let y1 = { Value = 10; Kind = Included }
    let x2 = { Value = 5; Kind = Included }
    let y2 = { Value = 12; Kind = Included }
    let i1 = Singleton { Start = x1; End = y1 }
    let i2 = Singleton { Start = x2; End = y2 }
    let result = intersection i1 i2
    let expected = i1

    Expect.equal "The intersection of a sub-interval should be the smaller interval" result expected

let private checkEmptyIntersection () =
    let x1 = { Value = 6; Kind = Excluded }
    let y1 = { Value = 10; Kind = Included }
    let x2 = { Value = 11; Kind = Included }
    let y2 = { Value = 12; Kind = Included }
    let i1 = Singleton { Start = x1; End = y1 }
    let i2 = Singleton { Start = x2; End = y2 }
    let result = intersection i1 i2
    let expected = Empty

    Expect.equal "There should be a non-empty intersection" result expected

let unitTests =
    let description = "[Unit Tests] Intersection"

    testList
        description
        [ testCase "Intervals that meet have non-empty intersections" checkSimpleIntersection
          testCase "Sub Intervals" checkSubIntervalIntersection
          testCase "Disjoint intervals have empty intersections" checkEmptyIntersection ]