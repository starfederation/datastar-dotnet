namespace StarFederation.Datastar.FSharp

open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Text
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open System.Web
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open StarFederation.Datastar.FSharp.Utility

[<Sealed>]
type ServerSentEventGenerator(httpContextAccessor:IHttpContextAccessor) =
    let defaultCancellationToken = httpContextAccessor.HttpContext.RequestAborted
    let httpRequest = httpContextAccessor.HttpContext.Request
    let httpResponse = httpContextAccessor.HttpContext.Response
    let mutable _startResponseTask : Task = null
    let _startResponseLock = obj()
    let _eventQueue = ConcurrentQueue<unit -> Task>()

    static member StartServerEventStreamAsync(httpResponse:HttpResponse, additionalHeaders:KeyValuePair<string, StringValues> seq, cancellationToken:CancellationToken) =
        let task = backgroundTask {
            httpResponse.Headers.ContentType <- "text/event-stream"
            httpResponse.Headers.CacheControl <- "no-cache"

            if (httpResponse.HttpContext.Request.Protocol = HttpProtocol.Http11) then
                httpResponse.Headers.Connection <- "keep-alive"

            for KeyValue(name, content) in additionalHeaders do
                match httpResponse.Headers.TryGetValue(name) with
                | false, _ -> httpResponse.Headers.Add(name, content)
                | true, _ -> ()

            do! httpResponse.StartAsync(cancellationToken)
            return! httpResponse.BodyWriter.FlushAsync(cancellationToken)
            }
        task :> Task

    static member PatchElementsAsync(httpResponse:HttpResponse, elements:string, options:PatchElementsOptions, cancellationToken:CancellationToken) =
        let writer = httpResponse.BodyWriter
        writer |> ServerSentEvent.sendEventType Bytes.EventTypePatchElements
        options.EventId |> ValueOption.iter (fun eventId -> writer |> ServerSentEvent.sendEventId eventId)
        if options.Retry <> Consts.DefaultSseRetryDuration then writer |> ServerSentEvent.sendRetry options.Retry

        options.Selector |> ValueOption.iter (fun selector -> writer |> ServerSentEvent.sendDataStringLine Bytes.DatalineSelector selector)

        if options.PatchMode <> Consts.DefaultElementPatchMode then
            writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineMode (options.PatchMode |> Bytes.ElementPatchMode.toBytes)

        if options.UseViewTransition <> Consts.DefaultElementsUseViewTransitions then
            writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineUseViewTransition (if options.UseViewTransition then Bytes.bTrue else Bytes.bFalse)

        for segment in String.splitLinesToSegments elements do
            writer |> ServerSentEvent.sendDataSegmentLine Bytes.DatalineElements segment

        writer |> ServerSentEvent.writeNewline

        writer.FlushAsync(cancellationToken).AsTask() :> Task

    static member RemoveElementAsync(httpResponse:HttpResponse, selector:Selector, options:RemoveElementOptions, cancellationToken:CancellationToken) =
        let writer = httpResponse.BodyWriter
        writer |> ServerSentEvent.sendEventType Bytes.EventTypePatchElements
        options.EventId |> ValueOption.iter (fun eventId -> writer |> ServerSentEvent.sendEventId eventId)
        if options.Retry <> Consts.DefaultSseRetryDuration then writer |> ServerSentEvent.sendRetry options.Retry

        writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineMode (ElementPatchMode.Remove |> Bytes.ElementPatchMode.toBytes)

        writer |> ServerSentEvent.sendDataStringLine Bytes.DatalineSelector selector

        if options.UseViewTransition <> Consts.DefaultElementsUseViewTransitions then
            writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineUseViewTransition (if options.UseViewTransition then Bytes.bTrue else Bytes.bFalse)

        writer |> ServerSentEvent.writeNewline

        writer.FlushAsync(cancellationToken).AsTask() :> Task

    static member PatchSignalsAsync(httpResponse:HttpResponse, signals:Signals, options:PatchSignalsOptions, cancellationToken:CancellationToken) =
        let writer = httpResponse.BodyWriter
        writer |> ServerSentEvent.sendEventType Bytes.EventTypePatchSignals
        options.EventId |> ValueOption.iter (fun eventId -> writer |> ServerSentEvent.sendEventId eventId)
        if options.Retry <> Consts.DefaultSseRetryDuration then writer |> ServerSentEvent.sendRetry options.Retry

        if options.OnlyIfMissing <> Consts.DefaultPatchSignalsOnlyIfMissing then
            writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineOnlyIfMissing (if options.OnlyIfMissing then Bytes.bTrue else Bytes.bFalse)

        for segment in String.splitLinesToSegments signals do
            writer |> ServerSentEvent.sendDataSegmentLine Bytes.DatalineSignals segment

        writer |> ServerSentEvent.writeNewline

        writer.FlushAsync(cancellationToken).AsTask() :> Task

    static member ExecuteScriptAsync(httpResponse:HttpResponse, script:string, options:ExecuteScriptOptions, cancellationToken:CancellationToken) =
        let writer = httpResponse.BodyWriter
        writer |> ServerSentEvent.sendEventType Bytes.EventTypePatchElements
        options.EventId |> ValueOption.iter (fun eventId -> writer |> ServerSentEvent.sendEventId eventId)
        if options.Retry <> Consts.DefaultSseRetryDuration then writer |> ServerSentEvent.sendRetry options.Retry

        writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineSelector Bytes.bBody

        writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineMode Bytes.ElementPatchMode.bAppend

        // <script ...>
        let attrsToString (attributes: KeyValuePair<string, string> list) =
            attributes |> Seq.map (fun kv -> $@"{kv.Key}=""{kv.Value |> HttpUtility.HtmlEncode}""")

        writer
        |> match (options.AutoRemove, options.Attributes) with
           | true, [] -> ServerSentEvent.sendDataBytesLine Bytes.DatalineElements Bytes.bOpenScriptAutoRemove
           | false, [] -> ServerSentEvent.sendDataBytesLine Bytes.DatalineElements Bytes.bOpenScript
           | true, attributes ->
               ServerSentEvent.sendDataStringSeqLine Bytes.DatalineElements
                   (seq { "<script"; Consts.ScriptDataEffectRemove; yield! (attrsToString attributes); ">" })
           | false, attributes ->
               ServerSentEvent.sendDataStringSeqLine Bytes.DatalineElements
                   (seq { "<script"; yield! (attrsToString attributes); ">" })

        // script
        for segment in String.splitLinesToSegments script do
            writer |> ServerSentEvent.sendDataSegmentLine Bytes.DatalineElements segment

        // </script>
        writer |> ServerSentEvent.sendDataBytesLine Bytes.DatalineElements Bytes.bCloseScript

        writer |> ServerSentEvent.writeNewline

        writer.FlushAsync(cancellationToken).AsTask() :> Task

    static member GetSignalsStream(httpRequest:HttpRequest) =
        match httpRequest.Method with
        | System.Net.WebRequestMethods.Http.Get ->
            match httpRequest.Query.TryGetValue(Consts.DatastarKey) with
            | true, stringValues when stringValues.Count > 0 -> (new MemoryStream(Encoding.UTF8.GetBytes(stringValues[0])) :> Stream)
            | _ -> Stream.Null
        | _ -> httpRequest.Body

    static member ReadSignalsAsync(httpRequest:HttpRequest, cancellationToken:CancellationToken) =
        backgroundTask {
            match httpRequest.Method with
            | System.Net.WebRequestMethods.Http.Get ->
                match httpRequest.Query.TryGetValue(Consts.DatastarKey) with
                | true, stringValues when stringValues.Count > 0 -> return stringValues[0]
                | _ -> return Signals.empty
            | _ ->
                try
                    use readResult = new StreamReader(httpRequest.Body)
                    return! readResult.ReadToEndAsync(cancellationToken)
                with _ ->
                    return Signals.empty
        }

    static member ReadSignalsAsync<'T>(httpRequest:HttpRequest, jsonSerializerOptions:JsonSerializerOptions, cancellationToken:CancellationToken) =
        task {
            try
                match httpRequest.Method with
                | System.Net.WebRequestMethods.Http.Get ->
                    match httpRequest.Query.TryGetValue(Consts.DatastarKey) with
                    | true, stringValues when stringValues.Count > 0 ->
                        return ValueSome (JsonSerializer.Deserialize<'T>(stringValues[0], jsonSerializerOptions))
                    | _ ->
                        return ValueNone
                | _ ->
                    let! t = JsonSerializer.DeserializeAsync<'T>(httpRequest.Body, jsonSerializerOptions, cancellationToken)
                    return (ValueSome t)
            with _ -> return ValueNone
        }

    member this.StartServerEventStreamAsync(additionalHeaders, cancellationToken) =
        lock _startResponseLock (fun () ->
            if _startResponseTask = null then
                _startResponseTask <- ServerSentEventGenerator.StartServerEventStreamAsync(httpResponse, additionalHeaders, cancellationToken)
            )
        _startResponseTask

    member private this.SendEventAsync(sendEventTask:unit -> Task, cancellationToken:CancellationToken) =
        backgroundTask {
            _eventQueue.Enqueue(sendEventTask)
            do!
                if _startResponseTask <> null
                then _startResponseTask
                else this.StartServerEventStreamAsync(Seq.empty, cancellationToken)
            let (_, sendEventTask') = _eventQueue.TryDequeue()
            return! sendEventTask' ()
        }

    member this.PatchElementsAsync(elements, options, cancellationToken) =
        let sendTask = fun () -> ServerSentEventGenerator.PatchElementsAsync(httpResponse, elements, options, cancellationToken)
        this.SendEventAsync(sendTask, cancellationToken) :> Task

    member this.RemoveElementAsync(selector, options, cancellationToken) =
        let sendTask = fun () -> ServerSentEventGenerator.RemoveElementAsync(httpResponse, selector, options, cancellationToken)
        this.SendEventAsync(sendTask, cancellationToken) :> Task

    member this.PatchSignalsAsync(signals, options, cancellationToken) =
        let sendTask = fun () -> ServerSentEventGenerator.PatchSignalsAsync(httpResponse, signals, options, cancellationToken)
        this.SendEventAsync(sendTask, cancellationToken) :> Task

    member this.ExecuteScriptAsync(script, options, cancellationToken) =
        let sendTask = fun () -> ServerSentEventGenerator.ExecuteScriptAsync(httpResponse, script, options, cancellationToken)
        this.SendEventAsync(sendTask, cancellationToken) :> Task

    member this.GetSignalsStream() =
        ServerSentEventGenerator.GetSignalsStream(httpRequest)

    member this.ReadSignalsAsync(cancellationToken) : Task<Signals> =
        ServerSentEventGenerator.ReadSignalsAsync(httpRequest, cancellationToken)

    member this.ReadSignalsAsync<'T>(jsonSerializerOptions, cancellationToken) =
        ServerSentEventGenerator.ReadSignalsAsync<'T>(httpRequest, jsonSerializerOptions, cancellationToken)

    //
    // SHORT HAND METHODS
    //
    static member StartServerEventStreamAsync(httpResponse, additionalHeaders) =
        ServerSentEventGenerator.StartServerEventStreamAsync(httpResponse, additionalHeaders, httpResponse.HttpContext.RequestAborted)
    static member StartServerEventStreamAsync(httpResponse) =
        ServerSentEventGenerator.StartServerEventStreamAsync(httpResponse, Seq.empty, httpResponse.HttpContext.RequestAborted)
    static member StartServerEventStreamAsync(httpResponse, cancellationToken) =
        ServerSentEventGenerator.StartServerEventStreamAsync(httpResponse, Seq.empty, cancellationToken)
    static member PatchElementsAsync(httpResponse, elements, options) =
        ServerSentEventGenerator.PatchElementsAsync(httpResponse, elements, options, httpResponse.HttpContext.RequestAborted)
    static member PatchElementsAsync(httpResponse, elements) =
        ServerSentEventGenerator.PatchElementsAsync(httpResponse, elements, PatchElementsOptions.Defaults, httpResponse.HttpContext.RequestAborted)
    static member RemoveElementAsync(httpResponse, selector, options) =
        ServerSentEventGenerator.RemoveElementAsync(httpResponse, selector, options, httpResponse.HttpContext.RequestAborted)
    static member RemoveElementAsync(httpResponse, selector) =
        ServerSentEventGenerator.RemoveElementAsync(httpResponse, selector, RemoveElementOptions.Defaults, httpResponse.HttpContext.RequestAborted)
    static member PatchSignalsAsync(httpResponse, signals, options) =
        ServerSentEventGenerator.PatchSignalsAsync(httpResponse, signals, options, httpResponse.HttpContext.RequestAborted)
    static member PatchSignalsAsync(httpResponse, signals) =
        ServerSentEventGenerator.PatchSignalsAsync(httpResponse, signals, PatchSignalsOptions.Defaults, httpResponse.HttpContext.RequestAborted)
    static member ExecuteScriptAsync(httpResponse, script, options) =
        ServerSentEventGenerator.ExecuteScriptAsync(httpResponse, script, options, httpResponse.HttpContext.RequestAborted)
    static member ExecuteScriptAsync(httpResponse, script) =
        ServerSentEventGenerator.ExecuteScriptAsync(httpResponse, script, ExecuteScriptOptions.Defaults, httpResponse.HttpContext.RequestAborted)
    static member ReadSignalsAsync(httpRequest) =
        ServerSentEventGenerator.ReadSignalsAsync(httpRequest, cancellationToken = httpRequest.HttpContext.RequestAborted)
    static member ReadSignalsAsync<'T>(httpRequest, jsonSerializerOptions) =
        ServerSentEventGenerator.ReadSignalsAsync<'T>(httpRequest, jsonSerializerOptions, httpRequest.HttpContext.RequestAborted)
    static member ReadSignalsAsync<'T>(httpRequest) =
        ServerSentEventGenerator.ReadSignalsAsync<'T>(httpRequest, JsonSerializerOptions.SignalsDefault, httpRequest.HttpContext.RequestAborted)

    member this.StartServerEventStreamAsync(additionalHeaders) =
        this.StartServerEventStreamAsync(additionalHeaders, defaultCancellationToken)
    member this.StartServerEventStreamAsync(cancellationToken: CancellationToken) =
        this.StartServerEventStreamAsync(Seq.empty, cancellationToken)
    member this.StartServerEventStreamAsync() =
        this.StartServerEventStreamAsync(Seq.empty, defaultCancellationToken)
    member this.PatchElementsAsync(elements, options) =
        this.PatchElementsAsync(elements, options, defaultCancellationToken)
    member this.PatchElementsAsync(elements, cancellationToken) =
        this.PatchElementsAsync(elements, PatchElementsOptions.Defaults, cancellationToken)
    member this.PatchElementsAsync(elements) =
        this.PatchElementsAsync(elements, PatchElementsOptions.Defaults, defaultCancellationToken)
    member this.RemoveElementAsync(selector, options) =
        this.RemoveElementAsync(selector, options, defaultCancellationToken)
    member this.RemoveElementAsync(selector, cancellationToken) =
        this.RemoveElementAsync(selector, RemoveElementOptions.Defaults, cancellationToken)
    member this.RemoveElementAsync(selector) =
        this.RemoveElementAsync(selector, RemoveElementOptions.Defaults, defaultCancellationToken)
    member this.PatchSignalsAsync(signals, options) =
        this.PatchSignalsAsync(signals, options, defaultCancellationToken)
    member this.PatchSignalsAsync(signals, cancellationToken) =
        this.PatchSignalsAsync(signals, PatchSignalsOptions.Defaults, cancellationToken)
    member this.PatchSignalsAsync(signals) =
        this.PatchSignalsAsync(signals, PatchSignalsOptions.Defaults, defaultCancellationToken)
    member this.ExecuteScriptAsync(script, options) =
        this.ExecuteScriptAsync(script, options, defaultCancellationToken)
    member this.ExecuteScriptAsync(script, cancellationToken) =
        this.ExecuteScriptAsync(script, ExecuteScriptOptions.Defaults, cancellationToken)
    member this.ExecuteScriptAsync(script) =
        this.ExecuteScriptAsync(script, ExecuteScriptOptions.Defaults, defaultCancellationToken)
    member this.ReadSignalsAsync() : Task<Signals> =
        this.ReadSignalsAsync(httpRequest.HttpContext.RequestAborted)
    member this.ReadSignalsAsync<'T>(jsonSerializerOptions) =
        this.ReadSignalsAsync<'T>(jsonSerializerOptions, httpRequest.HttpContext.RequestAborted)
    member this.ReadSignalsAsync<'T>(cancellationToken) =
        this.ReadSignalsAsync<'T>(JsonSerializerOptions.SignalsDefault, cancellationToken)
    member this.ReadSignalsAsync<'T>() =
        this.ReadSignalsAsync<'T>(JsonSerializerOptions.SignalsDefault, httpRequest.HttpContext.RequestAborted)
