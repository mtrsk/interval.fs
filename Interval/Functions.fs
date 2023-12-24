﻿namespace Interval

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
    let rec intersection<'T when 'T: equality and 'T: comparison> (a: Interval<'T>) (b: Interval<'T>) =
        match a, b with
        | Empty, _ -> Empty
        | _, Empty -> Empty
        | Singleton i1, Singleton i2 ->
            let newStart = max i1.Start i2.Start
            let newEnd = min i1.End i2.End

            if newStart < newEnd then
                Singleton { Start = newStart; End = newEnd }
            else
                Empty
        | Singleton s , Union u
        | Union u, Singleton s ->
            if Set.exists (fun x -> intersection x (Singleton s) <> Empty) (Set.map Singleton u) then
                let group = Set.add s u
                Union group
            else Union u
        | Union u1, Union u2 -> failwith "todo"

    /// <summary>
    /// Computes the union of two bounded intervals
    /// </summary>
    let union<'T when 'T: equality and 'T: comparison> (itv1: Interval<'T>) (itv2: Interval<'T>) =
        match itv1, itv2 with
        | Empty, i2 -> i2
        | i1, Empty -> i1
        | Singleton i1, Singleton i2 ->
            let minStart = min i1.Start i2.Start
            let maxStart = max i1.Start i2.Start
            let minEnd = min i1.End i2.End
            let maxEnd = max i1.End i2.End

            if minEnd < maxStart then
                let union =
                    [ { Start = minStart; End = minEnd }; { Start = maxStart; End = maxEnd } ]
                    |> Set.ofList
                Union union
            else
                Singleton { Start = minStart; End = maxEnd }
        | Singleton s, Union u
        | Union u, Singleton s ->
            let group = Set.add s u
            Union group
        | Union u1, Union u2 ->
            let group = Set.union u1 u2
            Union group

    let merge<'T when 'T: equality and 'T: comparison> (bs: BoundedInterval<'T> list) =
        let isSingleton x item =
            match union (Singleton x) (Singleton item) with
            | Singleton _s -> true
            | _ -> false
        let update index x (clusters: DisjointSet) =
            bs
            |> List.removeAt index
            |> List.mapi (fun i item -> if isSingleton x item then clusters.Unite(index, i) else ())
        let toSet (i: Interval<'T>) =
            match i with
            | Empty -> Set.empty
            | Singleton boundedInterval -> Set.add boundedInterval Set.empty
            | Union boundedIntervalSet -> boundedIntervalSet
        let rec loop (intervals: BoundedInterval<'T> list) (index: int) (clusters: DisjointSet) =
            match intervals with
            | [] -> clusters
            | [ x ] ->
                update index x clusters |> ignore
                clusters
            | x :: xs ->
                update index x clusters |> ignore
                loop xs (index + 1) clusters

        let forest = DisjointSet(bs.Length)
        let sets = (loop bs 0 forest).GetIds()
        let groups =
            sets
            |> List.ofArray
            |> List.zip bs
            |> List.groupBy snd
            |> List.map (fun (_, y) -> y |> List.map fst |> List.sort |> List.map Singleton)
            |> List.map (List.fold union Empty)
        match groups with
        | [] -> Empty
        | [x] -> x
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
            | Singleton _interval, true, true -> Meets
            | Singleton _interval, true, false -> Overlaps
            | Singleton _interval, false, true -> MetBy
            | Singleton _interval, false, false -> OverlappedBy
            | Union _union, true, _ -> Before
            | Union _union, false, _ -> After
            | _ -> failwith "todo"