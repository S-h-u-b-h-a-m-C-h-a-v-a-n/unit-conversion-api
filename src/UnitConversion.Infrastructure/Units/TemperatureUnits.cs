using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>
/// Temperature units. Base unit: kelvin. Unlike the other categories these scales have
/// different zero points, so they are modelled as affine (offset + factor) conversions.
/// </summary>
public sealed class TemperatureUnits : IUnitDefinitionProvider
{
    private const string Key = "temperature";

    // Absolute zero: nothing can be colder than 0 K.
    public UnitCategory Category { get; } = new(Key, "Temperature", "K", MinimumBaseValue: 0m);

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Linear(Key, "K", "kelvin", 1m, "kelvins"),
        Affine(Key, "°C", "celsius", offset: 273.15m, numerator: 1m, denominator: 1m, "C", "degC", "centigrade"),
        Affine(Key, "°F", "fahrenheit", offset: 459.67m, numerator: 5m, denominator: 9m, "F", "degF"),
        Ratio(Key, "°R", "rankine", 5m, 9m, "R", "degR"),
    ];
}
