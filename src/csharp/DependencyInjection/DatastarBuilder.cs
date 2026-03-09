using Microsoft.Extensions.DependencyInjection;

namespace StarFederation.Datastar.DependencyInjection;

public interface IDatastarBuilder
{
    IServiceCollection Services { get; }
}

internal sealed class DatastarBuilder(IServiceCollection services) : IDatastarBuilder
{
    public IServiceCollection Services { get; } = services;
}