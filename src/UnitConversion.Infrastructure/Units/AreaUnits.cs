using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>Area units. Base unit: square metre.</summary>
public sealed class AreaUnits : IUnitDefinitionProvider
{
    private const string Key = "area";

    public UnitCategory Category { get; } = new(Key, "Area", "m²");

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Linear(Key, "mm²", "square millimetre", 0.000001m, "mm2", "square millimeter"),
        Linear(Key, "cm²", "square centimetre", 0.0001m, "cm2", "square centimeter"),
        Linear(Key, "m²", "square metre", 1m, "m2", "sqm", "square meter", "square metres", "square meters"),
        Linear(Key, "ha", "hectare", 10000m, "hectares"),
        Linear(Key, "km²", "square kilometre", 1000000m, "km2", "square kilometer"),
        Linear(Key, "in²", "square inch", 0.00064516m, "in2", "sq in", "square inches"),
        Linear(Key, "ft²", "square foot", 0.09290304m, "ft2", "sq ft", "square feet"),
        Linear(Key, "yd²", "square yard", 0.83612736m, "yd2", "sq yd", "square yards"),
        Linear(Key, "ac", "acre", 4046.8564224m, "acres"),
        Linear(Key, "mi²", "square mile", 2589988.110336m, "mi2", "sq mi", "square miles"),
    ];
}
