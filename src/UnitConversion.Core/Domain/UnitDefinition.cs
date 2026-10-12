namespace UnitConversion.Core.Domain;

/// <summary>
/// Describes a single unit and how it relates to its category's base unit.
/// </summary>
/// <remarks>
/// Conversion to the base unit is the affine transform
/// <c>base = (value + Offset) * Numerator / Denominator</c>.
/// Linear units (metre, foot, pound, ...) have <c>Offset = 0</c>; only scales such as
/// Celsius or Fahrenheit need an offset. Keeping the factor as a fraction lets ratios
/// like 5/9 stay exact instead of being truncated to a repeating decimal.
/// </remarks>
public sealed class UnitDefinition
{
    private UnitDefinition(
        string symbol,
        string name,
        string categoryKey,
        decimal numerator,
        decimal denominator,
        decimal offset,
        IReadOnlyList<string> aliases)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryKey);
        if (numerator == 0 || denominator == 0)
        {
            throw new ArgumentException($"Unit '{symbol}' must have a non-zero conversion factor.");
        }

        Symbol = symbol;
        Name = name;
        CategoryKey = categoryKey;
        Numerator = numerator;
        Denominator = denominator;
        Offset = offset;
        Aliases = aliases;
    }

    public string Symbol { get; }

    public string Name { get; }

    public string CategoryKey { get; }

    public decimal Numerator { get; }

    public decimal Denominator { get; }

    public decimal Offset { get; }

    /// <summary>Alternative names accepted when looking the unit up (case-insensitive).</summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>Creates a unit where <c>1 unit = factor * base unit</c>.</summary>
    public static UnitDefinition Linear(
        string categoryKey, string symbol, string name, decimal factor, params string[] aliases) =>
        new(symbol, name, categoryKey, factor, 1m, 0m, aliases);

    /// <summary>Creates a unit where <c>1 unit = numerator / denominator * base unit</c>.</summary>
    public static UnitDefinition Ratio(
        string categoryKey, string symbol, string name, decimal numerator, decimal denominator, params string[] aliases) =>
        new(symbol, name, categoryKey, numerator, denominator, 0m, aliases);

    /// <summary>Creates a unit where <c>base = (value + offset) * numerator / denominator</c>.</summary>
    public static UnitDefinition Affine(
        string categoryKey,
        string symbol,
        string name,
        decimal offset,
        decimal numerator,
        decimal denominator,
        params string[] aliases) =>
        new(symbol, name, categoryKey, numerator, denominator, offset, aliases);

    public decimal ToBase(decimal value) => (value + Offset) * Numerator / Denominator;

    public decimal FromBase(decimal baseValue) => (baseValue * Denominator / Numerator) - Offset;
}
