using System.Diagnostics.CodeAnalysis;
using UnitConversion.Core.Domain;

namespace UnitConversion.Core.Abstractions;

/// <summary>Read-only, indexed view over every known category and unit.</summary>
public interface IUnitCatalog
{
    IReadOnlyCollection<UnitCategory> Categories { get; }

    bool TryGetCategory(string key, [NotNullWhen(true)] out UnitCategory? category);

    IReadOnlyCollection<UnitDefinition> GetUnits(string? categoryKey = null);

    /// <summary>
    /// Resolves a unit by symbol (exact case first, e.g. <c>Mm</c> vs <c>mm</c>),
    /// then by symbol, name or alias ignoring case.
    /// </summary>
    UnitDefinition GetUnit(string unit);
}
