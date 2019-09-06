namespace FunctionalConcurrency
open System
open System.Reflection
open System.Collections.Concurrent
open System.Runtime.Serialization

[<AutoOpen>]
module Utilities =
    open System.IO
    open System

    let inline flip f a b = f b a

    /// Given a value, apply a function to it, ignore the result, then return the original value.
    let inline tee fn x = fn x |> ignore; x

    /// Ensures that the continuation will be called in the same synchronization
    /// context as where the operation was started
    let synchronize f =
        let ctx = System.Threading.SynchronizationContext.Current
        f (fun g arg ->
            let nctx = System.Threading.SynchronizationContext.Current
            if ctx <> null && ctx <> nctx then ctx.Post((fun _ -> g(arg)), null)
            else g(arg) )

    type BaseStream(stream:Stream) =
        member this.AsyncWriteBytes (bytes : byte []) = async {
                do! stream.AsyncWrite(BitConverter.GetBytes bytes.Length, 0, 4)
                do! stream.AsyncWrite(bytes, 0, bytes.Length)
                return! stream.FlushAsync()
            }

        member this.AsyncReadBytes(length : int) =
            let rec readSegment buf offset remaining = async {
                    let! read = stream.AsyncRead(buf, offset, remaining)
                    if read < remaining then
                        return! readSegment buf (offset + read) (remaining - read)
                    else
                        return () }
            async {
                let bytes = Array.zeroCreate<byte> length
                do! readSegment bytes 0 length
                return bytes
            }

        member this.AsyncReadBytes() = async {
                let! lengthArr = this.AsyncReadBytes 4
                let length = BitConverter.ToInt32(lengthArr, 0)
                return! this.AsyncReadBytes length
            }


 // A collection of utilities for authoring F# classes
[<RequireQualifiedAccess>]
module FSharpClass =

    /// <summary>
    ///     Comparison by projection.
    /// </summary>
    /// <param name="proj">Projection function.</param>
    /// <param name="this">this value.</param>
    /// <param name="that">that value.</param>
    let inline compareBy (proj : 'T -> 'U) (this : 'T) (that : obj) : int =
        match that with
        | :? 'T as that -> compare (proj this) (proj that)
        | _ -> invalidArg "that" <| sprintf "invalid comparand %A." that

    /// <summary>
    ///     Hashcode by projection.
    /// </summary>
    /// <param name="proj">Projection function.</param>
    /// <param name="this">this value.</param>
    let inline hashBy (proj : 'T -> 'U) (this : 'T) : int = hash (proj this)

    /// <summary>
    ///     Equality by projection.
    /// </summary>
    /// <param name="proj">Projection function.</param>
    /// <param name="this">this value.</param>
    /// <param name="that">that value.</param>
    let inline equalsBy (proj : 'T -> 'U) (this : 'T) (that : obj): bool =
        match that with
        | :? 'T as that -> proj this = proj that
        | _ -> false

    /// <summary>
    ///     Equality by comparison projection.
    /// </summary>
    /// <param name="proj">Projection function.</param>
    /// <param name="this">this value.</param>
    /// <param name="that">that value.</param>
    let inline equalsByComparison (proj : 'T -> 'U) (this : 'T) (that : obj) : bool =
        match that with
        | :? 'T as that -> compareBy proj this that = 0
        | _ -> false

/// IEnumerable extensions
[<RequireQualifiedAccess>]
module Seq =

    /// <summary>
    ///     Try reading the head of given sequence.
    /// </summary>
    /// <param name="xs">Input sequence.</param>
    let tryHead (xs: seq<'a>) : 'a option =
        if Seq.isEmpty xs then None else xs |> Seq.head |> Some


/// F# option extensions
[<RequireQualifiedAccess>]
module Option =
    let ofChoice choice =
        match choice with
        | Choice1Of2 value -> Some value
        | Choice2Of2 _ -> None

    /// <summary>
    ///     Returns 'Some x' iff f x is satisfied.
    /// </summary>
    /// <param name="f">predicate to be evaluated.</param>
    /// <param name="opt">Input optional.</param>
    let filter f opt =
        match opt with
        | None -> None
        | Some x -> if f x then opt else None

    /// <summary>
    ///     Returns 'Some t' iff t is not null.
    /// </summary>
    /// <param name="t">Value to be examined.</param>
    let ofNull<'T when 'T : not struct> (t : 'T) =
        if obj.ReferenceEquals(t, null) then None
        else
            Some t

    /// <summary>
    ///     Attempt to return the head of a list.
    /// </summary>
    /// <param name="xs">Input list.</param>
    let ofList xs = match xs with [] -> None | h :: _ -> Some h

    /// <summary>
    ///     match t with None -> s | Some t0 -> f t0
    /// </summary>
    /// <param name="f">Mapping function.</param>
    /// <param name="s">Default value.</param>
    /// <param name="t">Optional input.</param>
    let bind2 (f : 'T -> 'S) (s : 'S) (t : 'T option) =
        match t with None -> s | Some t0 -> f t0
