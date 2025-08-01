namespace StarFederation.Datastar.FSharp

open System
open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.RegularExpressions
open StarFederation.Datastar.FSharp.Utility

/// Signals read to and from Datastar on the front end
type Signals = string

/// A dotted path into Signals to access a key/value pair
type SignalPath = string

/// An HTML selector name
type Selector = string

[<Struct>]
type PatchElementsOptions =
    { Selector: Selector voption
      PatchMode: ElementPatchMode
      UseViewTransition: bool
      EventId: string voption
      Retry: TimeSpan }
    static member Defaults =
        { Selector = ValueNone
          PatchMode = Consts.DefaultElementPatchMode
          UseViewTransition = Consts.DefaultElementsUseViewTransitions
          EventId = ValueNone
          Retry = Consts.DefaultSseRetryDuration }

[<Struct>]
type RemoveElementOptions =
    { UseViewTransition: bool
      EventId: string voption
      Retry: TimeSpan }
    static member Defaults =
        { UseViewTransition = Consts.DefaultElementsUseViewTransitions
          EventId = ValueNone
          Retry = Consts.DefaultSseRetryDuration }

[<Struct>]
type PatchSignalsOptions =
    { OnlyIfMissing: bool
      EventId: string voption
      Retry: TimeSpan }
    static member Defaults =
        { OnlyIfMissing = Consts.DefaultPatchSignalsOnlyIfMissing
          EventId = ValueNone
          Retry = Consts.DefaultSseRetryDuration }

[<Struct>]
type ExecuteScriptOptions =
    { EventId: string voption
      Retry: TimeSpan
      AutoRemove: bool
      /// added to the &lt;script&gt; tag
      Attributes: KeyValuePair<string, string> list }
    static member Defaults =
        { EventId = ValueNone
          Retry = Consts.DefaultSseRetryDuration
          AutoRemove = true
          Attributes = [] }

module JsonSerializerOptions =
    let SignalsDefault =
        let options = JsonSerializerOptions()
        options.PropertyNameCaseInsensitive <- true
        options

module Signals =
    let create (signalsString:string) = Signals signalsString
    let tryCreate (signalsString:string) =
        try
            let _ = JsonObject.Parse(signalsString)
            ValueSome(Signals signalsString)
        with _ ->
            ValueNone
    let empty = Signals "{ }"

module SignalPath =
    let isValidKey (signalPathKey:string) =
        signalPathKey
        |> String.isPopulated && signalPathKey.ToCharArray() |> Seq.forall (fun chr -> Char.IsLetter chr || Char.IsNumber chr || chr = '_')

    let isValid (signalPathString:string) =
        signalPathString.Split('.') |> Array.forall isValidKey

    let tryCreate (signalPathString:string) =
        if isValid signalPathString
            then ValueSome(SignalPath signalPathString)
            else ValueNone

    let create (signalPathString:string) =
        if isValid signalPathString
            then SignalPath signalPathString
            else failwith $"{signalPathString} is not a valid signal path"

    let kebabValue signals = signals |> String.toKebab
    let keys (signalPath:SignalPath) = signalPath.Split('.')

module Selector =
    let regex = Regex(@"[#.][-_]?[_a-zA-Z]+(?:\w|\\.)*|(?<=\s+|^)(?:\w+|\*)|\[[^\s""'=<>`]+?(?<![~|^$*])([~|^$*]?=(?:['""].*['""]|[^\s""'=<>`]+))?\]|:[\w-]+(?:\(.*\))?", RegexOptions.Compiled)

    let isValid (selectorString:string) = regex.IsMatch selectorString

    let tryCreate (selectorString:string) =
        if isValid selectorString
            then ValueSome(Selector selectorString)
            else ValueNone

    let create (selectorString:string) =
        if isValid selectorString
            then Selector selectorString
            else failwith $"{selectorString} is not a valid selector"
