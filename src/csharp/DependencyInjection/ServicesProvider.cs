using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Core = StarFederation.Datastar.FSharp;

namespace StarFederation.Datastar.DependencyInjection;

public static class ServiceCollectionExtensionMethods
{
    public static IDatastarBuilder AddDatastar(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<DatastarJsonOptions>();

        serviceCollection
            .AddHttpContextAccessor()
            .AddScoped<IDatastarService>(svcPvd =>
                new DatastarService(
                    new Core.ServerSentEventGenerator(svcPvd.GetService<IHttpContextAccessor>()),
                    svcPvd.GetRequiredService<IOptions<DatastarJsonOptions>>()));

        return new DatastarBuilder(serviceCollection);
    }

    public static IDatastarBuilder AddJsonOptions(this IDatastarBuilder datastarBuilder, Action<JsonSerializerOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(datastarBuilder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        datastarBuilder.Services.Configure<DatastarJsonOptions>(options => configureOptions(options.SignalsJsonSerializerOptions));
        return datastarBuilder;
    }
}