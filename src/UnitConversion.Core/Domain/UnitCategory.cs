namespace UnitConversion.Core.Domain;

/// <summary>
/// A family of mutually convertible units (e.g. length, mass, temperature).
/// Every unit in a category converts through the category's base unit.
/// </summary>
/// <param name="Key">Stable, URL-friendly identifier, e.g. <c>length</c>.</param>
/// <param name="Name">Human-readable name.</param>
/// <param name="BaseUnitSymbol">Symbol of the unit all conversions pass through.</param>
/// <param name="MinimumBaseValue">
/// Optional physical lower bound expressed in the base unit (e.g. absolute zero for temperature).
/// </param>
public sealed record UnitCategory(
    string Key,
    string Name,
    string BaseUnitSymbol,
    decimal? MinimumBaseValue = null);
