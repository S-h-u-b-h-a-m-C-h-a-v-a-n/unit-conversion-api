using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>Speed units. Base unit: metre per second.</summary>
public sealed class SpeedUnits : IUnitDefinitionProvider
{
    private const string Key = "speed";

    public UnitCategory Category { get; } = new(Key, "Speed", "m/s");

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Linear(Key, "m/s", "metre per second", 1m, "mps", "meter per second", "metres per second", "meters per second"),
        Ratio(Key, "km/h", "kilometre per hour", 1000m, 3600m, "kph", "kmh", "kilometer per hour", "kilometres per hour"),
        Linear(Key, "mph", "mile per hour", 0.44704m, "mi/h", "miles per hour"),
        Linear(Key, "ft/s", "foot per second", 0.3048m, "fps", "feet per second"),
        Ratio(Key, "kn", "knot", 1852m, 3600m, "kt", "knots"),
    ];
}
