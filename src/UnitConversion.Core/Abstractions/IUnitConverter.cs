using UnitConversion.Core.Domain;

namespace UnitConversion.Core.Abstractions;

public interface IUnitConverter
{
    /// <summary>Converts <paramref name="value"/> from one unit to another of the same category.</summary>
    /// <exception cref="Exceptions.UnitNotFoundException">A unit is unknown.</exception>
    /// <exception cref="Exceptions.IncompatibleUnitsException">The units belong to different categories.</exception>
    /// <exception cref="Exceptions.InvalidConversionValueException">The value is out of range for the category.</exception>
    ConversionResult Convert(decimal value, string fromUnit, string toUnit);
}
