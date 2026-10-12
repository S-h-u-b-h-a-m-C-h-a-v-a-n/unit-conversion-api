using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using static UnitConversion.Core.Domain.UnitDefinition;

namespace UnitConversion.Infrastructure.Units;

/// <summary>Volume units. Base unit: litre. Customary units are US unless marked imperial.</summary>
public sealed class VolumeUnits : IUnitDefinitionProvider
{
    private const string Key = "volume";

    public UnitCategory Category { get; } = new(Key, "Volume", "L");

    public IEnumerable<UnitDefinition> GetUnits() =>
    [
        Linear(Key, "mL", "millilitre", 0.001m, "milliliter", "millilitres", "milliliters"),
        Linear(Key, "cm³", "cubic centimetre", 0.001m, "cm3", "cc", "cubic centimeter"),
        Linear(Key, "L", "litre", 1m, "liter", "litres", "liters"),
        Linear(Key, "m³", "cubic metre", 1000m, "m3", "cubic meter", "cubic metres", "cubic meters"),
        Linear(Key, "tsp", "teaspoon", 0.00492892159375m, "teaspoons"),
        Linear(Key, "tbsp", "tablespoon", 0.01478676478125m, "tablespoons"),
        Linear(Key, "fl oz", "fluid ounce", 0.0295735295625m, "floz", "fluid ounces"),
        Linear(Key, "cup", "cup", 0.2365882365m, "cups"),
        Linear(Key, "pt", "pint", 0.473176473m, "pints"),
        Linear(Key, "qt", "quart", 0.946352946m, "quarts"),
        Linear(Key, "gal", "gallon", 3.785411784m, "gallons", "us gallon"),
        Linear(Key, "imp gal", "imperial gallon", 4.54609m, "uk gallon", "imperial gallons"),
    ];
}
