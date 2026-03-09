using System.Text.Json;
using Core = StarFederation.Datastar.FSharp;

namespace StarFederation.Datastar.DependencyInjection;

public class DatastarJsonOptions
{
    public JsonSerializerOptions SignalsJsonSerializerOptions { get; set; } = new(Core.JsonSerializerOptions.SignalsDefault);
}