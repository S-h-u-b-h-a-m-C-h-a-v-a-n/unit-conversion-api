using Microsoft.Extensions.DependencyInjection;
using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Services;
using UnitConversion.Infrastructure.Units;

namespace UnitConversion.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the conversion services and the hardcoded unit data.
    /// To add a category, implement <see cref="IUnitDefinitionProvider"/> and add it to
    /// <see cref="HardcodedUnitProviders.All"/>.
    /// </summary>
    public static IServiceCollection AddUnitConversion(this IServiceCollection services)
    {
        foreach (var provider in HardcodedUnitProviders.All)
        {
            services.AddSingleton(provider);
        }

        // The catalog is immutable once built, so a singleton is safe and avoids re-indexing per request.
        services.AddSingleton<IUnitCatalog, UnitCatalog>();
        services.AddSingleton<IUnitConverter, UnitConverter>();
        return services;
    }
}

/// <summary>The unit definitions shipped with this version of the service.</summary>
public static class HardcodedUnitProviders
{
    public static IReadOnlyList<IUnitDefinitionProvider> All { get; } =
    [
        new LengthUnits(),
        new MassUnits(),
        new TemperatureUnits(),
        new VolumeUnits(),
        new AreaUnits(),
        new TimeUnits(),
        new SpeedUnits(),
        new DataStorageUnits(),
    ];
}
