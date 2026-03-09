using System.Diagnostics;
using System.Text.Json.Serialization;
using StarFederation.Datastar.DependencyInjection;

namespace HelloWorld;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDatastar();

        WebApplication app = builder.Build();
        app.UseStaticFiles();

        app.MapGet("/stream-element-patches", async (IDatastarService datastarService) =>
        {
            const string message = "Hello, Elements!";
            Signals? mySignals = await datastarService.ReadSignalsAsync<Signals>();
            Debug.Assert(mySignals != null, nameof(mySignals) + " != null");

            await datastarService.PatchSignalsAsync(new { ShowPatchElementMessage = true });

            for (var index = 1; index < message.Length; ++index)
            {
                await datastarService.PatchElementsAsync($"""<div id="message">{message[..index]}</div>""");
                await Task.Delay(TimeSpan.FromMilliseconds(mySignals.Delay.GetValueOrDefault(0)));
            }

            await datastarService.PatchElementsAsync($"""<div id="message">{message}</div>""");
        });

        app.MapGet("/stream-signal-patches", async (IDatastarService datastarService) =>
        {
            const string message = "Hello, Signals!";
            Signals? mySignals = await datastarService.ReadSignalsAsync<Signals>();
            Debug.Assert(mySignals != null, nameof(mySignals) + " != null");

            await datastarService.PatchSignalsAsync(new { ShowPatchElementMessage = false });

            for (var index = 1; index < message.Length; ++index)
            {
                await datastarService.PatchSignalsAsync(new { SignalsMessage = message[..index] });
                await Task.Delay(TimeSpan.FromMilliseconds(mySignals.Delay.GetValueOrDefault(0)));
            }

            await datastarService.PatchSignalsAsync(new { SignalsMessage = message });
        });

        app.MapGet("/execute-script", (IDatastarService datastarService) =>
            datastarService.ExecuteScriptAsync("alert('Hello! from the server 🚀')"));

        app.Run();
    }

    public record Signals
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Delay { get; set; } = null;
    }
}