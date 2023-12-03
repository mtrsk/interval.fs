namespace Interval

module Functions =
    open Interval.Core
    open Interval.Helpers

    // https://www.fssnip.net/5D/title/Weighted-QuickUnion-with-Path-Compression
    type DisjointSet(n: int) =
        // Initialize each element with its index as the parent
        let id = Array.init n id
        // Number of elements rooted at i
        let sz = Array.create n 1

        let rec root i =
            if i = id.[i] then
                i
            else
                // Path compression
                id.[i] <- root id.[i]
                id.[i]

        member this.Find(p, q) = root p = root q

        member this.Unite(p, q) =
            let i = root p
            let j = root q

            if sz.[i] < sz.[j] then
                id.[i] <- j
                sz.[j] <- sz.[j] + sz.[i]
            else
                id.[j] <- i
                sz.[i] <- sz.[i] + sz.[j]

        member this.GetIds() = id

        override this.ToString() = $"%A{Array.zip id sz}"

    /// <summary>
    /// Computes the intersection of two bounded intervals
    /// </summary>
    let intersection<'T when 'T: equality and 'T: comparison> (itv1: Interval<'T>) (itv2: Interval<'T>) =
        match itv1, itv2 with
        | Empty, _ -> Empty
        | _, Empty -> Empty
        | Interval i1, Interval i2 ->
            let newStart = max i1.Start i2.Start
            let newEnd = min i1.End i2.End

            if newStart < newEnd then
                Interval { Start = newStart; End = newEnd }
            else
                Empty

    /// <summary>
    /// Computes the union of two bounded intervals
    /// </summary>
    let union<'T when 'T: equality and 'T: comparison> (itv1: Interval<'T>) (itv2: Interval<'T>) =
        match itv1, itv2 with
        | Empty, i2 -> i2 |> Choice1Of2
        | i1, Empty -> i1 |> Choice1Of2
        | Interval i1, Interval i2 ->
            let minStart = min i1.Start i2.Start
            let maxStart = max i1.Start i2.Start
            let minEnd = min i1.End i2.End
            let maxEnd = max i1.End i2.End

            if minEnd < maxStart then
                Union(Interval { Start = minStart; End = minEnd }, Interval { Start = maxStart; End = maxEnd })
                |> Choice2Of2
            else
                Interval { Start = minStart; End = maxEnd } |> Choice1Of2

    let merge<'T when 'T: equality and 'T: comparison> (is: Interval<'T> list) =
        let rec loop (intervals: Interval<'T> list) (index: int) (clusters: DisjointSet) =
            match intervals with
            | [] -> clusters
            | [ x ] ->
                let _ =
                    is
                    |> List.removeAt (index)
                    |> List.mapi (fun i item ->
                        match union x item with
                        | Choice1Of2 _interval -> clusters.Unite(index, i)
                        | Choice2Of2 _union -> ())

                clusters
            | x :: xs ->
                let _ =
                    is
                    |> List.removeAt (index)
                    |> List.mapi (fun i item ->
                        match union x item with
                        | Choice1Of2 _interval -> clusters.Unite(index, i)
                        | Choice2Of2 _union -> ())

                loop xs (index + 1) clusters

        let forest = DisjointSet(is.Length)

        let output =
            loop is 0 forest
            |> (fun x -> x.GetIds())
            |> List.ofArray
            |> List.zip is
            |> List.groupBy snd
            |> List.map (fun (_, y) -> y |> List.map fst)

        output

    /// <summary>
    /// Inverts a relation
    /// </summary>
    let invert (r: Relation) =
        match r with
        | After -> Before
        | Before -> After
        | Contains -> During
        | During -> Contains
        | Equals -> Equals
        | Finishes -> FinishedBy
        | FinishedBy -> Finishes
        | Meets -> MetBy
        | MetBy -> Meets
        | Overlaps -> OverlappedBy
        | OverlappedBy -> Overlaps
        | Starts -> StartedBy
        | StartedBy -> Starts

    /// <summary>
    /// Computes the qualitative relationship of two intervals
    /// </summary>
    let relate<'T when 'T: equality and 'T: comparison> (a: Interval<'T>) (b: Interval<'T>) =
        let isEqualA = (intersection a b = a)
        let isEqualB = (intersection a b = b)

        match isEqualA, isEqualB with
        | true, true -> Equals
        | true, false when starts a b -> Starts
        | true, false when finishes a b -> Finishes
        | true, false -> During
        | false, true when startedBy a b -> StartedBy
        | false, true when finishedBy a b -> FinishedBy
        | false, true -> Contains
        | false, false ->
            match (union a b, precedes a b, isEmpty <| intersection a b) with
            | Choice1Of2 _interval, true, true -> Meets
            | Choice1Of2 _interval, true, false -> Overlaps
            | Choice1Of2 _interval, false, true -> MetBy
            | Choice1Of2 _interval, false, false -> OverlappedBy
            | Choice2Of2 _union, true, _ -> Before
            | Choice2Of2 _union, false, _ -> After