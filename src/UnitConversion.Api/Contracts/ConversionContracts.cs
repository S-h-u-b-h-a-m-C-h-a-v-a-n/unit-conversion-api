using System.ComponentModel.DataAnnotations;

namespace UnitConversion.Api.Contracts;

/// <summary>Query parameters for a single conversion.</summary>
public sealed class ConversionQuery
{
    /// <summary>The number to convert.</summary>
    /// <example>100</example>
    [Required]
    public decimal? Value { get; init; }

    /// <summary>Source unit: symbol (exact case), name or alias, e.g. <c>°C</c>, <c>C</c> or <c>celsius</c>.</summary>
    /// <example>C</example>
    [Required(AllowEmptyStrings = false)]
    public string? From { get; init; }

    /// <summary>Target unit: symbol, name or alias.</summary>
    /// <example>F</example>
    [Required(AllowEmptyStrings = false)]
    public string? To { get; init; }

    /// <summary>Decimal places to round the result to. Defaults to the server setting.</summary>
    /// <example>4</example>
    [Range(0, ApiConstants.MaxDecimalPlaces)]
    public int? Decimals { get; init; }
}

/// <summary>One conversion inside a batch request.</summary>
public sealed class ConversionRequest
{
    [Required]
    public decimal? Value { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string? From { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string? To { get; init; }
}

/// <summary>Several conversions processed in one call; each item succeeds or fails independently.</summary>
public sealed class BatchConversionRequest
{
    [Required]
    [MinLength(1)]
    public IReadOnlyList<ConversionRequest>? Items { get; init; }

    [Range(0, ApiConstants.MaxDecimalPlaces)]
    public int? Decimals { get; init; }
}

public sealed record ConversionResponse(
    decimal Value,
    UnitSummary From,
    UnitSummary To,
    decimal Result,
    string Category);

public sealed record UnitSummary(string Symbol, string Name);

public sealed record BatchConversionItemResponse(int Index, bool Succeeded, ConversionResponse? Conversion, string? Error);

public sealed record BatchConversionResponse(int Succeeded, int Failed, IReadOnlyList<BatchConversionItemResponse> Items);
