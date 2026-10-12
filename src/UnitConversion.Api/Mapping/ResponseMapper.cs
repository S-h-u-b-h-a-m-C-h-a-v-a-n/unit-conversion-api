using UnitConversion.Api.Contracts;
using UnitConversion.Core.Domain;

namespace UnitConversion.Api.Mapping;

/// <summary>Maps domain objects to API contracts so the domain can evolve without breaking clients.</summary>
internal static class ResponseMapper
{
    public static ConversionResponse ToResponse(this ConversionResult result, int decimals) =>
        new(
            result.Value,
            new UnitSummary(result.From.Symbol, result.From.Name),
            new UnitSummary(result.To.Symbol, result.To.Name),
            Normalize(Math.Round(result.Result, decimals, MidpointRounding.AwayFromZero)),
            result.Category.Key);

    public static CategoryResponse ToResponse(this UnitCategory category, int unitCount) =>
        new(category.Key, category.Name, category.BaseUnitSymbol, unitCount);

    public static UnitResponse ToResponse(this UnitDefinition unit) =>
        new(unit.Symbol, unit.Name, unit.CategoryKey, unit.Aliases);

    // decimal keeps trailing zeros from arithmetic (212.00); strip them so JSON shows 212.
    private static decimal Normalize(decimal value) => value / 1.000000000000000000000000000000000m;
}
