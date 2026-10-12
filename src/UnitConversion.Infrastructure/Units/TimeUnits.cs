using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>Time units. Base unit: second. A year is the Julian year (365.25 days).</summary>
public sealed class TimeUnits : IUnitDefinitionProvider
{
    private const string Key = "time";

    public UnitCategory Category { get; } = new(Key, "Time", "s");

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Linear(Key, "ns", "nanosecond", 0.000000001m, "nanoseconds"),
        Linear(Key, "µs", "microsecond", 0.000001m, "us", "microseconds"),
        Linear(Key, "ms", "millisecond", 0.001m, "milliseconds"),
        Linear(Key, "s", "second", 1m, "seconds", "sec", "secs"),
        Linear(Key, "min", "minute", 60m, "minutes", "mins"),
        Linear(Key, "h", "hour", 3600m, "hours", "hr", "hrs"),
        Linear(Key, "d", "day", 86400m, "days"),
        Linear(Key, "wk", "week", 604800m, "weeks"),
        Linear(Key, "yr", "year", 31557600m, "years", "julian year"),
    ];
}
