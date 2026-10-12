using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>
/// Digital storage units. Base unit: byte. Decimal (SI, kB = 1000 B) and binary (IEC, KiB = 1024 B)
/// prefixes are both supported. Bits use a lowercase "b" (Mb), bytes an uppercase "B" (MB),
/// so symbols in this category must be sent with their exact casing.
/// </summary>
public sealed class DataStorageUnits : IUnitDefinitionProvider
{
    private const string Key = "data-storage";

    public UnitCategory Category { get; } = new(Key, "Data storage", "B");

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Ratio(Key, "b", "bit", 1m, 8m, "bits"),
        Linear(Key, "kb", "kilobit", 125m, "kilobits"),
        Linear(Key, "Mb", "megabit", 125000m, "megabits"),
        Linear(Key, "Gb", "gigabit", 125000000m, "gigabits"),
        Linear(Key, "B", "byte", 1m, "bytes"),
        Linear(Key, "kB", "kilobyte", 1000m, "kilobytes"),
        Linear(Key, "MB", "megabyte", 1000000m, "megabytes"),
        Linear(Key, "GB", "gigabyte", 1000000000m, "gigabytes"),
        Linear(Key, "TB", "terabyte", 1000000000000m, "terabytes"),
        Linear(Key, "KiB", "kibibyte", 1024m, "kibibytes"),
        Linear(Key, "MiB", "mebibyte", 1048576m, "mebibytes"),
        Linear(Key, "GiB", "gibibyte", 1073741824m, "gibibytes"),
        Linear(Key, "TiB", "tebibyte", 1099511627776m, "tebibytes"),
    ];
}
