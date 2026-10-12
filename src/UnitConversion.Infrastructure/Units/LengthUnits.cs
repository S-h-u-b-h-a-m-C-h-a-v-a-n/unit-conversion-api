using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>Length units. Base unit: metre. Imperial factors are exact by international definition (1959).</summary>
public sealed class LengthUnits : IUnitDefinitionProvider
{
    private const string Key = "length";

    public UnitCategory Category { get; } = new(Key, "Length", "m");

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Linear(Key, "nm", "nanometre", 0.000000001m, "nanometer", "nanometres", "nanometers"),
        Linear(Key, "µm", "micrometre", 0.000001m, "um", "micrometer", "micron", "microns"),
        Linear(Key, "mm", "millimetre", 0.001m, "millimeter", "millimetres", "millimeters"),
        Linear(Key, "cm", "centimetre", 0.01m, "centimeter", "centimetres", "centimeters"),
        Linear(Key, "m", "metre", 1m, "meter", "metres", "meters"),
        Linear(Key, "km", "kilometre", 1000m, "kilometer", "kilometres", "kilometers"),
        Linear(Key, "in", "inch", 0.0254m, "inches", "\""),
        Linear(Key, "ft", "foot", 0.3048m, "feet", "'"),
        Linear(Key, "yd", "yard", 0.9144m, "yards"),
        Linear(Key, "mi", "mile", 1609.344m, "miles"),
        Linear(Key, "nmi", "nautical mile", 1852m, "nautical miles"),
    ];
}
