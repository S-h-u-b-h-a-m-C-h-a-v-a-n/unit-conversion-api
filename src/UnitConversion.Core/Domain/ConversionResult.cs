namespace UnitConversion.Core.Domain;

/// <summary>Outcome of a successful conversion.</summary>
public sealed record ConversionResult(
    decimal Value,
    UnitDefinition From,
    UnitDefinition To,
    decimal Result,
    UnitCategory Category);
