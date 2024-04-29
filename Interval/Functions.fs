namespace Interval

open Interval.Core

module Functions =
    open Interval.Core
    open Interval.Helpers

    // https://www.fssnip.net/5D/title/Weighted-QuickUnion-with-Path-Compression
    type DisjointSet(n: int) =
        // Initialize each element with its index as the parent
        let ids = Array.init n id
        // Number of elements rooted at i
        let sz = Array.create n 1

        let rec root i =
            if i = ids.[i] then
                i
            else
                // Path compression
                ids.[i] <- root ids.[i]
                ids.[i]

        member this.Find(p, q) = root p = root q

        member this.Unite(p, q) =
            let i = root p
            let j = root q

            if sz.[i] < sz.[j] then
                ids.[i] <- j
                sz.[j] <- sz.[j] + sz.[i]
            else
                ids.[j] <- i
                sz.[i] <- sz.[i] + sz.[j]

        member this.GetIds() = ids

        override this.ToString() = $"%A{Array.zip ids sz}"

    /// <summary>
    /// Computes the intersection of two bounded intervals
    /// </summary>
    let intersection<'T when 'T: equality and 'T: comparison> (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Empty, _ -> Empty
        | _, Empty -> Empty
        | Singleton i1, Singleton i2 -> i1 * i2
        | Singleton s, Union u
        | Union u, Singleton s ->
            if Set.exists (fun x -> isNotEmpty (x * s)) u then
                let group = Set.add s u
                Union group
            else
                Union u
        | Union u1, Union u2 ->
            let u =
                cartesian u1 u2
                |> List.map (fun (x, y) -> x * y)
                |> List.choose tryGetSingleton

            match u with
            | [] -> Empty
            | [ x ] -> Singleton x
            | xs -> Union(Set.ofList xs)

    /// <summary>
    /// Computes the union of two intervals
    /// </summary>
    let union<'T when 'T: equality and 'T: comparison> (interval1: Interval<'T>) (interval2: Interval<'T>) =
        match interval1, interval2 with
        | Empty, i2 -> i2
        | i1, Empty -> i1
        | Singleton i1, Singleton i2 -> i1 + i2
        | Singleton s, Union u
        | Union u, Singleton s ->
            let group = Set.add s u
            Union group
        | Union u1, Union u2 ->
            let group = Set.union u1 u2
            Union group

    /// <summary>
    /// Computes the union of multiple bounded intervals
    /// </summary>
    let generateForest<'T when 'T: equality and 'T: comparison> (bs: BoundedInterval<'T> list) =
        let isUnionSingleton a b =
            match union (Singleton a) (Singleton b) with
            | Singleton _ -> true
            | _ -> false

        let update index x (forest: DisjointSet) =
            bs
            |> List.removeAt index
            |> List.mapi (fun i item -> if isUnionSingleton x item then forest.Unite(index, i) else ())

        let rec loop (intervals: BoundedInterval<'T> list) (index: int) (clusters: DisjointSet) =
            match intervals with
            | [] -> clusters
            | [ x ] ->
                update index x clusters |> ignore
                clusters
            | x :: xs ->
                update index x clusters |> ignore
                loop xs (index + 1) clusters

        // TODO: Improve this
        let forest = DisjointSet(bs.Length)
        let sets = (loop bs 0 forest).GetIds()
        let clusters =
            sets
            |> List.ofArray
            |> List.zip bs
            |> List.groupBy snd
            |> List.map (fun (_, y) -> y |> List.map fst |> List.sort |> List.map Singleton)
        clusters

    let merge<'T when 'T: equality and 'T: comparison> (bs: BoundedInterval<'T> list) =
        let toSet (i: Interval<'T>) =
            match i with
            | Empty -> Set.empty
            | Singleton boundedInterval -> Set.add boundedInterval Set.empty
            | Union boundedIntervalSet -> boundedIntervalSet

        let groups =
            generateForest bs
            |> List.map (List.fold union Empty)

        match groups with
        | [] -> Empty
        | [ x ] -> x
        | _ ->
            let output = groups |> List.map toSet |> Set.ofList |> Set.fold Set.union Set.empty
            Union output

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
        let isSubset x y = ((intersection x y) = x)
        match isSubset a b, isSubset b a with
        | true, true -> Equals
        | true, false when starts a b -> Starts
        | true, false when finishes a b -> Finishes
        | true, false -> During
        | false, true when startedBy a b -> StartedBy
        | false, true when finishedBy a b -> FinishedBy
        | false, true -> Contains
        | false, false ->
            match (union a b, precedes a b, isEmpty <| intersection a b) with
            | Singleton _interval, true, true ->
                // a ∪ b = { x .. y }, a < b, a ∩ b = ∅
                Meets
            | Singleton _interval, true, false ->
                // a ∪ b = { x .. y }, a < b, a ∩ b ≠ ∅
                Overlaps
            | Singleton _interval, false, true ->
                // a ∪ b = { x .. y }, a > b, a ∩ b = ∅
                MetBy
            | Singleton _interval, false, false ->
                // a ∪ b = { x .. y }, a > b, a ∩ b ≠ ∅
                OverlappedBy
            | Union _union, true, _ ->
                // a ∪ b = { x .. y } ∪ { z .. w }, a < b
                Before
            | Union _union, false, _ ->
                // a ∪ b = { x .. y } ∪ { z .. w }, a > b
                After
            | Empty, _, _ ->
                // a  ∪  b = ∅ ⇔ a = ∅ ^ b = ∅
                Equals
