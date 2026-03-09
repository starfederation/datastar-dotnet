# Datastar + dotnet

[![NuGet Version](https://img.shields.io/nuget/v/Starfederation.Datastar.svg)](https://www.nuget.org/packages/Starfederation.Datastar)

Real-time Hypermedia first Library and Framework for dotnet

The dotnet Datastar library is written in two parts:
- The F# library (StarFederation.Datastar.FSharp) implements the [Architecture Decision Record: Datastar SDK](https://github.com/starfederation/datastar/blob/develop/sdk/ADR.md) to provide the core, Datastar functionality.
- The C# library (StarFederation.Datastar) uses the F# library for its core functionality as well as providing Dependency Injection, Model Binding, and C#-friendly types.

# HTML Frontend

```html
<main class="container" id="main" data-signals="{'input':'','output':'empty'}">
    <button data-on:click="@get('/displayDate')">Display Date</button>
    <div id="target"></div>
    <input type="text" placeholder="input:" data-bind:input/><br/>
    <span data-text="$output"></span>
    <button data-on:click="@post('/changeOutput')">Change Output</button>
</main>
```

# C# Backend

```csharp
using StarFederation.Datastar;
using StarFederation.Datastar.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

// add as an ASP Service
//  allows injection of IDatastarService, to respond to a request with a Datastar friendly ServerSentEvent
//  and to read the signals sent by the client
builder.Services
    .AddDatastar()
    .AddJsonOptions(options =>
    {
        options.Converters.Add(new JsonStringEnumConverter());
    });

// displayDate - patching an element
app.MapGet("/displayDate", async (IDatastarService datastarService) =>
{
    string today = DateTime.Now.ToString("%y-%M-%d %h:%m:%s");
    await datastarService.PatchElementsAsync($"""<div id='target'><span id='date'><b>{today}</b><button data-on:click="@get('/removeDate')">Remove</button></span></div>""");
});

// removeDate - removing an element
app.MapGet("/removeDate", async (IDatastarService datastarService) => { await datastarService.RemoveElementAsync("#date"); });

public record MySignals {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Input { get; init; } = null;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Output { get; init; } = null;

    public string Serialize() => ...
}

// changeOutput - reads the signals, update the Output, and merge back
app.MapPost("/changeOutput", async (IDatastarService datastarService) => ...
{
    MySignals signals = await datastarService.ReadSignalsAsync<MySignals>();
    MySignals newSignals = new() { Output = $"Your Input: {signals.Input}" };
    await datastarService.PatchSignalsAsync(newSignals.Serialize());
});
```

# F# Backend

```fsharp
namespace HelloWorld
open System
open System.Text.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open StarFederation.Datastar.FSharp

module Program =
    let message = "Hello, world!"

    [<CLIMutable>]
    type MySignals = { input:string; output:string }

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        builder.Services.AddHttpContextAccessor();
        let app = builder.Build()
        app.UseStaticFiles()

        app.MapGet("/displayDate", Func<IHttpContextAccessor, Task>(fun ctx -> task {
            do! ServerSentEventGenerator.StartServerEventStreamAsync(ctx.HttpContext.Response)
            let today = DateTime.Now.ToString("%y-%M-%d %h:%m:%s")
            do! ServerSentEventGenerator.PatchElementsAsync(ctx.HttpContext.Response, $"""<div id='target'><span id='date'><b>{today}</b><button data-on:click="@get('/removeDate')">Remove</button></span></div>""")
        }))

        app.MapGet("/removeDate", Func<IHttpContextAccessor, Task>(fun ctx -> task {
            do! ServerSentEventGenerator.StartServerEventStreamAsync(ctx.HttpContext.Response)
            do! ServerSentEventGenerator.RemoveElementAsync (ctx.HttpContext.Response, "#date")
        }))

        app.MapPost("/changeOutput", Func<IHttpContextAccessor, Task>(fun ctx -> task {
            do! ServerSentEventGenerator.StartServerEventStreamAsync(ctx.HttpContext.Response)
            let! signals = ServerSentEventGenerator.ReadSignalsAsync<MySignals>(ctx.HttpContext.Request)
            let signals' = signals |> ValueOption.defaultValue { input = ""; output = "" }
            do! ServerSentEventGenerator.PatchSignalsAsync(ctx.HttpContext.Response, JsonSerializer.Serialize( { signals' with output = $"Your input: {signals'.input}" } ))
        }))

        app.Run()

        0
```

# Model Binding

```csharp
public class MySignals {
    public string MyString { get; set; } = "";
    public int MyInt { get; set; } = 0;
    public InnerSignals MyInner { get; set; } = new();

    public class InnerSignals {
        public string MyInnerString { get; set; } = "";
        public int MyInnerInt { get; set; } = 0;
    }
}

public IActionResult Test_GetSignals([FromSignals] MySignals signals) => ...

public IActionResult Test_GetValues([FromSignals] string myString, [FromSignals] int myInt) => ...

public IActionResult Test_GetInnerPathed([FromSignals(Path = "myInner")] MySignals.InnerSignals myInnerOther) => ...

public IActionResult Test_GetInnerValues([FromSignals(Path = "myInner.myInnerString")] string myInnerStringOther, [FromSignals(Path = "myInner.myInnerInt")] int myInnerIntOther) => ...
```