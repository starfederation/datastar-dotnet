using System.Text.Json;
using Microsoft.Extensions.Primitives;
using Core = StarFederation.Datastar.FSharp;

// ReSharper disable InvalidXmlDocComment

namespace StarFederation.Datastar.DependencyInjection;

public interface IDatastarService
{
    /// <summary>Send a 200 text/event-stream Response. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task StartServerEventStreamAsync();
    /// <summary>Send a 200 text/event-stream Response.</summary>
    Task StartServerEventStreamAsync(CancellationToken cancellationToken);
    /// <summary>Send a 200 text/event-stream Response. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, StringValues>> additionalHeaders);
    /// <summary>Send a 200 text/event-stream Response. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, string>> additionalHeaders);
    /// <summary>Send a 200 text/event-stream Response.</summary>
    Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, StringValues>> additionalHeaders, CancellationToken cancellationToken);
    /// <summary>Send a 200 text/event-stream Response.</summary>
    Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, string>> additionalHeaders, CancellationToken cancellationToken);

    /// <summary>Send an HTML element to the client for DOM manipulation. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    /// <param name="elements">Complete, well-formed HTML elements</param>
    Task PatchElementsAsync(string elements);
    /// <summary>Send an HTML element to the client for DOM manipulation. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    /// <param name="elements">Complete, well-formed HTML elements</param>
    Task PatchElementsAsync(string elements, PatchElementsOptions options);
    /// <summary>Send an HTML element to the client for DOM manipulation.</summary>
    /// <param name="elements">Complete, well-formed HTML elements</param>
    Task PatchElementsAsync(string elements, CancellationToken cancellationToken);
    /// <summary>Send an HTML element to the client for DOM manipulation.</summary>
    /// <param name="elements">Complete, well-formed HTML elements</param>
    Task PatchElementsAsync(string elements, PatchElementsOptions options, CancellationToken cancellationToken);

    /// <summary>Remove an element from the DOM. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task RemoveElementAsync(string selector);
    /// <summary>Remove an element from the DOM. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task RemoveElementAsync(string selector, RemoveElementOptions options);
    /// <summary>Remove an element from the DOM.</summary>
    Task RemoveElementAsync(string selector, CancellationToken cancellationToken);
    /// <summary>Remove an element from the DOM.</summary>
    Task RemoveElementAsync(string selector, RemoveElementOptions options, CancellationToken cancellationToken);

    /// <summary>Update signals on the browser. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task PatchSignalsAsync<TType>(TType signals);
    /// <summary>Update signals on the browser. Uses JsonSerializer.Serialize() to convert TType to JSON</summary>
    Task PatchSignalsAsync<TType>(TType signals, CancellationToken cancellationToken);
    /// <summary>Update signals on the browser. Uses JsonSerializer.Serialize() to convert TType to JSON. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions);
    /// <summary>Update signals on the browser. Uses JsonSerializer.Serialize() to convert TType to JSON.</summary>
    Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken);
    /// <summary>Update signals on the browser. Uses JsonSerializer.Serialize() to convert TType to JSON. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task PatchSignalsAsync<TType>(TType signals, PatchSignalsOptions patchSignalsOptions);
    /// <summary>Update signals on the browser. Uses JsonSerializer.Serialize() to convert TType to JSON.</summary>
    Task PatchSignalsAsync<TType>(TType signals, PatchSignalsOptions patchSignalsOptions, CancellationToken cancellationToken);
    /// <summary>Update signals on the browser. Uses JsonSerializer.Serialize() to convert TType to JSON. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions, PatchSignalsOptions patchSignalsOptions);
    /// <summary>Update signals on the browser. Uses JsonSerializer.Serialize() to convert TType to JSON.</summary>
    Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions, PatchSignalsOptions patchSignalsOptions, CancellationToken cancellationToken);

    /// <summary>Execute a Javascript snippet on the browser. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    /// <param name="script">JS snippet; do not include &lt;script&gt; in string</param>
    Task ExecuteScriptAsync(string script);
    /// <summary>Execute a Javascript snippet on the browser.</summary>
    /// <param name="script">JS snippet; do not include &lt;script&gt; in string</param>
    Task ExecuteScriptAsync(string script, CancellationToken cancellationToken);
    /// <summary>Execute a Javascript snippet on the browser. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    /// <param name="script">JS snippet; do not include &lt;script&gt; in string</param>
    Task ExecuteScriptAsync(string script, ExecuteScriptOptions options);
    /// <summary>Execute a Javascript snippet on the browser.</summary>
    /// <param name="script">JS snippet; do not include &lt;script&gt; in string</param>
    Task ExecuteScriptAsync(string script, ExecuteScriptOptions options, CancellationToken cancellationToken);

    /// <summary>Gets a stream to the signals in JSON format from the Request. For GET requests, a MemoryStream backing will be created.</summary>
    Stream GetSignalsStream();

    /// <summary>Gets the signals in JSON format from the Request. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    /// <returns>JSON string or null, if none</returns>
    Task<string?> ReadSignalsAsync();
    /// <summary>Gets the signals in JSON format from the Request.</summary>
    /// <returns>JSON string or null, if none</returns>
    Task<string?> ReadSignalsAsync(CancellationToken cancellationToken);

    /// <summary>Gets the signals from the Request. Uses JsonSerializer.Deserialize to parse JSON signals, with PropertyNameCaseInsensitive = true. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    /// <returns>TType or null, if none</returns>
    Task<TType?> ReadSignalsAsync<TType>();
    /// <summary>Gets the signals from the Request. Uses JsonSerializer.Deserialize to parse JSON signals. Implicit CancellationToken = HttpContext.RequestAborted.</summary>
    /// <returns>TType or null, if none</returns>
    Task<TType?> ReadSignalsAsync<TType>(JsonSerializerOptions options);
    /// <summary>Gets the signals from the Request. Uses JsonSerializer.Deserialize to parse JSON signals, with PropertyNameCaseInsensitive = true.</summary>
    /// <returns>TType or null, if none</returns>
    Task<TType?> ReadSignalsAsync<TType>(CancellationToken cancellationToken);
    /// <summary>Gets the signals from the Request. Uses JsonSerializer.Deserialize to parse JSON signals.</summary>
    /// <returns>TType or null, if none</returns>
    Task<TType?> ReadSignalsAsync<TType>(JsonSerializerOptions options, CancellationToken cancellationToken);
}

internal class DatastarService(Core.ServerSentEventGenerator serverSentEventGenerator) : IDatastarService
{
    public Task StartServerEventStreamAsync()
        => serverSentEventGenerator.StartServerEventStreamAsync();

    public Task StartServerEventStreamAsync(CancellationToken cancellationToken)
        => serverSentEventGenerator.StartServerEventStreamAsync(cancellationToken);

    public Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, StringValues>> additionalHeaders)
        => serverSentEventGenerator.StartServerEventStreamAsync(additionalHeaders);

    public Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, string>> additionalHeaders)
        => serverSentEventGenerator.StartServerEventStreamAsync(additionalHeaders.Select(kv => new KeyValuePair<string, StringValues>(kv.Key, new(kv.Value))));

    public Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, StringValues>> additionalHeaders, CancellationToken cancellationToken)
        => serverSentEventGenerator.StartServerEventStreamAsync(additionalHeaders, cancellationToken);

    public Task StartServerEventStreamAsync(IEnumerable<KeyValuePair<string, string>> additionalHeaders, CancellationToken cancellationToken)
        => serverSentEventGenerator.StartServerEventStreamAsync(additionalHeaders.Select(kv => new KeyValuePair<string, StringValues>(kv.Key, new(kv.Value))), cancellationToken);

    public Task PatchElementsAsync(string elements)
        => serverSentEventGenerator.PatchElementsAsync(elements);
    public Task PatchElementsAsync(string elements, PatchElementsOptions options)
        => serverSentEventGenerator.PatchElementsAsync(elements, options);
    public Task PatchElementsAsync(string elements, CancellationToken cancellationToken)
        => serverSentEventGenerator.PatchElementsAsync(elements, cancellationToken);
    public Task PatchElementsAsync(string elements, PatchElementsOptions options, CancellationToken cancellationToken)
        => serverSentEventGenerator.PatchElementsAsync(elements, options, cancellationToken);

    public Task RemoveElementAsync(string selector)
        => serverSentEventGenerator.RemoveElementAsync(selector);
    public Task RemoveElementAsync(string selector, RemoveElementOptions options)
        => serverSentEventGenerator.RemoveElementAsync(selector, options);
    public Task RemoveElementAsync(string selector, CancellationToken cancellationToken)
        => serverSentEventGenerator.RemoveElementAsync(selector, cancellationToken);
    public Task RemoveElementAsync(string selector, RemoveElementOptions options, CancellationToken cancellationToken)
        => serverSentEventGenerator.RemoveElementAsync(selector, options, cancellationToken);

    public Task PatchSignalsAsync<TType>(TType signals)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, Core.JsonSerializerOptions.SignalsDefault));
    public Task PatchSignalsAsync<TType>(TType signals, CancellationToken cancellationToken)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, Core.JsonSerializerOptions.SignalsDefault), cancellationToken);
    public Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, jsonSerializerOptions));
    public Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, jsonSerializerOptions), cancellationToken);
    public Task PatchSignalsAsync<TType>(TType signals, PatchSignalsOptions patchSignalsOptions)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, Core.JsonSerializerOptions.SignalsDefault), patchSignalsOptions);
    public Task PatchSignalsAsync<TType>(TType signals, PatchSignalsOptions patchSignalsOptions, CancellationToken cancellationToken)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, Core.JsonSerializerOptions.SignalsDefault), patchSignalsOptions, cancellationToken);
    public Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions, PatchSignalsOptions patchSignalsOptions)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, jsonSerializerOptions), patchSignalsOptions);
    public Task PatchSignalsAsync<TType>(TType signals, JsonSerializerOptions jsonSerializerOptions, PatchSignalsOptions patchSignalsOptions, CancellationToken cancellationToken)
        => serverSentEventGenerator.PatchSignalsAsync(JsonSerializer.Serialize(signals, jsonSerializerOptions), patchSignalsOptions, cancellationToken);

    public Task ExecuteScriptAsync(string script)
        => serverSentEventGenerator.ExecuteScriptAsync(script);
    public Task ExecuteScriptAsync(string script, CancellationToken cancellationToken)
        => serverSentEventGenerator.ExecuteScriptAsync(script, cancellationToken);
    public Task ExecuteScriptAsync(string script, ExecuteScriptOptions options)
        => serverSentEventGenerator.ExecuteScriptAsync(script, options);
    public Task ExecuteScriptAsync(string script, ExecuteScriptOptions options, CancellationToken cancellationToken)
        => serverSentEventGenerator.ExecuteScriptAsync(script, options, cancellationToken);

    public Stream GetSignalsStream()
        => serverSentEventGenerator.GetSignalsStream();

    public async Task<string?> ReadSignalsAsync()
        => await serverSentEventGenerator.ReadSignalsAsync() is { Length: > 0 } signals ? signals : null;
    public async Task<string?> ReadSignalsAsync(CancellationToken cancellationToken)
        => await serverSentEventGenerator.ReadSignalsAsync(cancellationToken) is { Length: > 0 } signals ? signals : null;

    public async Task<TType?> ReadSignalsAsync<TType>()
        => await serverSentEventGenerator.ReadSignalsAsync() is { Length: > 0 } signals ? JsonSerializer.Deserialize<TType>(signals, Core.JsonSerializerOptions.SignalsDefault) : default;
    public async Task<TType?> ReadSignalsAsync<TType>(JsonSerializerOptions options)
        => await serverSentEventGenerator.ReadSignalsAsync() is { Length: > 0 } signals ? JsonSerializer.Deserialize<TType>(signals, options) : default;
    public async Task<TType?> ReadSignalsAsync<TType>(CancellationToken cancellationToken)
        => await serverSentEventGenerator.ReadSignalsAsync(cancellationToken) is { Length: > 0 } signals ? JsonSerializer.Deserialize<TType>(signals, Core.JsonSerializerOptions.SignalsDefault) : default;
    public async Task<TType?> ReadSignalsAsync<TType>(JsonSerializerOptions options, CancellationToken cancellationToken)
        => await serverSentEventGenerator.ReadSignalsAsync(cancellationToken) is { Length: > 0 } signals ? JsonSerializer.Deserialize<TType>(signals, options) : default;
}