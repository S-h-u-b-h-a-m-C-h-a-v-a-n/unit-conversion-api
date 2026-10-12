namespace UnitConversion.Core.Exceptions;

/// <summary>Base type for expected, client-caused conversion errors.</summary>
public abstract class UnitConversionException : Exception
{
    protected UnitConversionException(string message)
        : base(message)
    {
    }

    protected UnitConversionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Short, stable title used in error responses.</summary>
    public abstract string Title { get; }
}

public sealed class UnitNotFoundException(string unit, string? hint = null)
    : UnitConversionException($"Unit '{unit}' is not supported.{(hint is null ? string.Empty : " " + hint)}")
{
    public string Unit { get; } = unit;

    public override string Title => "Unknown unit";
}

public sealed class CategoryNotFoundException(string category)
    : UnitConversionException($"Category '{category}' is not supported.")
{
    public string Category { get; } = category;

    public override string Title => "Unknown category";
}

public sealed class IncompatibleUnitsException(string fromUnit, string fromCategory, string toUnit, string toCategory)
    : UnitConversionException(
        $"Cannot convert '{fromUnit}' ({fromCategory}) to '{toUnit}' ({toCategory}): units belong to different categories.")
{
    public override string Title => "Incompatible units";
}

public sealed class InvalidConversionValueException : UnitConversionException
{
    public InvalidConversionValueException(string message)
        : base(message)
    {
    }

    public InvalidConversionValueException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override string Title => "Invalid value";
}
