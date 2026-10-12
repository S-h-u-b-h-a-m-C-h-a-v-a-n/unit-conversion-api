using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>Mass / weight units. Base unit: kilogram. 1 lb = 0.45359237 kg exactly.</summary>
public sealed class MassUnits : IUnitDefinitionProvider
{
    private const string Key = "mass";

    public UnitCategory Category { get; } = new(Key, "Mass", "kg");

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Linear(Key, "mg", "milligram", 0.000001m, "milligrams"),
        Linear(Key, "g", "gram", 0.001m, "grams"),
        Linear(Key, "kg", "kilogram", 1m, "kilograms", "kilo", "kilos"),
        Linear(Key, "t", "tonne", 1000m, "tonnes", "metric ton", "metric tons"),
        Linear(Key, "oz", "ounce", 0.028349523125m, "ounces"),
        Linear(Key, "lb", "pound", 0.45359237m, "pounds", "lbs"),
        Linear(Key, "st", "stone", 6.35029318m, "stones"),
        Linear(Key, "ton", "short ton", 907.18474m, "us ton", "short tons"),
        Linear(Key, "LT", "long ton", 1016.0469088m, "imperial ton", "long tons"),
    ];
}
