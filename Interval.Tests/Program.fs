open Expecto

module Main =
    let tests =
        testList "Tests" [
            Interval.Tests.Intersection.unitTests
            Interval.Tests.Union.unitTests
        ]
    [<EntryPoint>]
    let main _ = runTestsWithCLIArgs [] [||] tests