using System.ComponentModel.DataAnnotations;

namespace UnitConversion.Api.Configuration;

/// <summary>Tunable API behaviour, bound from the <c>Conversion</c> configuration section.</summary>
public sealed class ConversionOptions
{
    public const string SectionName = "Conversion";

    /// <summary>Decimal places used when the caller does not specify <c>decimals</c>.</summary>
    [Range(0, ApiConstants.MaxDecimalPlaces)]
    public int DefaultDecimalPlaces { get; init; } = 10;

    /// <summary>Maximum number of conversions accepted in a single batch request.</summary>
    [Range(1, 10_000)]
    public int MaxBatchSize { get; init; } = 100;
}
