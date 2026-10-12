using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using UnitConversion.Core.Exceptions;

namespace UnitConversion.Core.Services;

/// <summary>
/// Converts values by normalising them to the category's base unit and then to the target unit.
/// With N units per category this needs N definitions instead of N² pairwise factors.
/// </summary>
public sealed class UnitConverter(IUnitCatalog catalog) : IUnitConverter
{
    public ConversionResult Convert(decimal value, string fromUnit, string toUnit)
    {
        var from = catalog.GetUnit(fromUnit);
        var to = catalog.GetUnit(toUnit);

        if (!string.Equals(from.CategoryKey, to.CategoryKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new IncompatibleUnitsException(from.Symbol, from.CategoryKey, to.Symbol, to.CategoryKey);
        }

        if (!catalog.TryGetCategory(from.CategoryKey, out var category))
        {
            throw new CategoryNotFoundException(from.CategoryKey);
        }

        try
        {
            var baseValue = from.ToBase(value);
            if (baseValue < category.MinimumBaseValue)
            {
                throw new InvalidConversionValueException(
                    $"{value} {from.Symbol} is below the minimum possible {category.Name.ToLowerInvariant()} " +
                    $"({from.FromBase(category.MinimumBaseValue.Value)} {from.Symbol}).");
            }

            var result = ReferenceEquals(from, to) ? value : to.FromBase(baseValue);
            return new ConversionResult(value, from, to, result, category);
        }
        catch (OverflowException ex)
        {
            throw new InvalidConversionValueException(
                $"Converting {value} {from.Symbol} to {to.Symbol} exceeds the supported numeric range.", ex);
        }
    }
}
