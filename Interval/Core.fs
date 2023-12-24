namespace Interval

module Core =
    open System

    [<CustomEquality; CustomComparison>]
    type BoundaryKind =
        | Included
        | Excluded

        static member (+) (a: BoundaryKind, b: BoundaryKind) =
            match a,b with
            | Included,  _ -> Included
            | _, Included -> Included
            | Excluded, Excluded -> Excluded

        override this.Equals other =
            match other with
            | :? BoundaryKind as k ->
                match this, k with
                | Included, Included -> true
                | Excluded, Excluded -> true
                | _ -> false
            | _ -> false

        interface IEquatable<BoundaryKind> with
            member this.Equals other =
                match this, other with
                | Included, Included -> true
                | Excluded, Excluded -> true
                | _ -> false

        override this.GetHashCode() = this.GetHashCode()

        interface IComparable with
            member this.CompareTo(other: obj) =
                match other with
                | :? BoundaryKind as k ->
                    match this, k with
                    | Included, Included -> 0
                    | Included, Excluded -> 1
                    | Excluded, Included -> -1
                    | Excluded, Excluded -> 0
                | _ -> raise (ArgumentException $"Object type for {other} must match")


    /// <summary>
    /// Represents a Open/Closed Boundary with a generic value of type 'T
    /// </summary>
    [<CustomEquality; CustomComparison>]
    type Boundary<'T when 'T: equality and 'T: comparison> =
        { Value: 'T
          Kind: BoundaryKind }
        override this.Equals other =
            match other with
            | :? Boundary<'T> as b -> this.Value = b.Value && this.Kind = b.Kind
            | _ -> false

        interface IEquatable<Boundary<'T>> with
            member this.Equals other = (this.Value = other.Value && this.Kind = other.Kind)

        override this.GetHashCode() = this.Value.GetHashCode()

        interface IComparable with
            member this.CompareTo(other: obj) =
                match other with
                | :? Boundary<'T> as b ->
                    // If both values match, we have to verify if they're Open/Closed as well
                    match compare this.Value b.Value with
                    | 0 -> compare this.Kind b.Kind
                    | n -> n
                | _ -> raise (ArgumentException $"Object type for {other} must match")

    /// <summary>
    /// Represents a bounded interval with start and end boundaries of type 'T.
    /// </summary>
    type BoundedInterval<'T when 'T: equality and 'T: comparison> =
        { Start: Boundary<'T>
          End: Boundary<'T> }
        static member (+) (a: BoundedInterval<'T>, b: BoundedInterval<'T>) =
            let minStart = min a.Start b.Start
            let maxStart = max a.Start b.Start
            let minEnd = min a.End b.End
            let maxEnd = max a.End b.End
            if minEnd < maxStart then
                [ { Start = minStart; End = minEnd }; { Start = maxStart; End = maxEnd } ]
            else
                [ { Start = minStart; End = maxEnd } ]

    /// <summary>
    /// Represents an interval, which can be either Empty or a BoundedInterval of type 'T.
    /// </summary>
    type Interval<'T when 'T: equality and 'T: comparison> =
        | Empty
        | Singleton of BoundedInterval<'T>
        | Union of BoundedInterval<'T> Set

    /// <summary>
    /// Represents different temporal relations between intervals.
    /// </summary>
    type Relation =
        | After
        | Before
        | Contains
        | During
        | Equals
        | Finishes
        | FinishedBy
        | Meets
        | MetBy
        | Overlaps
        | OverlappedBy
        | Starts
        | StartedBy

    /// <summary>
    /// Represents an error that can occur during interval parsing.
    /// </summary>
    type ParseIntervalError = InvalidArgument of errorMessage: string