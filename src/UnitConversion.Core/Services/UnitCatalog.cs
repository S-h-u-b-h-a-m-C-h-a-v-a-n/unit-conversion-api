using System.Diagnostics.CodeAnalysis;
using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using UnitConversion.Core.Exceptions;

namespace UnitConversion.Core.Services;

/// <summary>
/// Builds an in-memory index over all registered <see cref="IUnitDefinitionProvider"/>s.
/// It is built once and is immutable afterwards, so lookups are O(1) and thread-safe
/// regardless of how many units are registered.
/// </summary>
/// <remarks>
/// Data problems (duplicate symbols, a missing base unit, ...) throw at construction time
/// so a bad unit definition fails the application on startup instead of at request time.
/// </remarks>
public sealed class UnitCatalog : IUnitCatalog
{
    private readonly Dictionary<string, UnitCategory> _categories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<UnitDefinition>> _unitsByCategory = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, UnitDefinition> _bySymbol = new(StringComparer.Ordinal);
    private readonly Dictionary<string, UnitDefinition> _byNameOrAlias = new(StringComparer.OrdinalIgnoreCase);

    // Symbols that only differ by case between two units (e.g. "mm" and "Mm"); they need an exact-case lookup.
    private readonly HashSet<string> _ambiguousKeys = new(StringComparer.OrdinalIgnoreCase);

    public UnitCatalog(IEnumerable<IUnitDefinitionProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        foreach (var provider in providers)
        {
            AddCategory(provider.Category, provider.GetUnits());
        }

        IndexSymbolsCaseInsensitively();

        Categories = _categories.Values.OrderBy(c => c.Key, StringComparer.Ordinal).ToArray();
    }

    public IReadOnlyCollection<UnitCategory> Categories { get; }

    public bool TryGetCategory(string key, [NotNullWhen(true)] out UnitCategory? category) =>
        _categories.TryGetValue(key, out category);

    public IReadOnlyCollection<UnitDefinition> GetUnits(string? categoryKey = null)
    {
        if (string.IsNullOrWhiteSpace(categoryKey))
        {
            return _unitsByCategory.Values.SelectMany(u => u).ToArray();
        }

        return _unitsByCategory.TryGetValue(categoryKey.Trim(), out var units)
            ? units
            : throw new CategoryNotFoundException(categoryKey);
    }

    public UnitDefinition GetUnit(string unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        var key = unit.Trim();

        if (_bySymbol.TryGetValue(key, out var exact))
        {
            return exact;
        }

        if (_byNameOrAlias.TryGetValue(key, out var match))
        {
            return match;
        }

        if (_ambiguousKeys.Contains(key))
        {
            var candidates = _bySymbol.Keys.Where(s => string.Equals(s, key, StringComparison.OrdinalIgnoreCase));
            throw new UnitNotFoundException(
                unit, $"This symbol is case-sensitive; did you mean one of: {string.Join(", ", candidates)}?");
        }

        throw new UnitNotFoundException(unit);
    }

    private void AddCategory(UnitCategory category, IEnumerable<UnitDefinition> units)
    {
        if (!_categories.TryAdd(category.Key, category))
        {
            throw new InvalidOperationException($"Category '{category.Key}' is registered more than once.");
        }

        var list = new List<UnitDefinition>();
        foreach (var unit in units)
        {
            if (!string.Equals(unit.CategoryKey, category.Key, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Unit '{unit.Symbol}' declares category '{unit.CategoryKey}' but is provided by '{category.Key}'.");
            }

            if (!_bySymbol.TryAdd(unit.Symbol, unit))
            {
                throw new InvalidOperationException($"Unit symbol '{unit.Symbol}' is registered more than once.");
            }

            foreach (var alias in unit.Aliases.Append(unit.Name))
            {
                if (_byNameOrAlias.TryGetValue(alias, out var existing) && !ReferenceEquals(existing, unit))
                {
                    throw new InvalidOperationException(
                        $"Name/alias '{alias}' is used by both '{existing.Symbol}' and '{unit.Symbol}'.");
                }

                _byNameOrAlias[alias] = unit;
            }

            list.Add(unit);
        }

        if (!list.Exists(u => u.Symbol == category.BaseUnitSymbol))
        {
            throw new InvalidOperationException(
                $"Category '{category.Key}' declares base unit '{category.BaseUnitSymbol}' but does not define it.");
        }

        _unitsByCategory[category.Key] = list;
    }

    private void IndexSymbolsCaseInsensitively()
    {
        foreach (var group in _bySymbol.Values.GroupBy(u => u.Symbol, StringComparer.OrdinalIgnoreCase))
        {
            if (group.Skip(1).Any())
            {
                // e.g. "mm" (millimetre) vs "Mm" (megametre): only an exact-case lookup is safe.
                _ambiguousKeys.Add(group.Key);
                continue;
            }

            // An explicit name/alias wins over a case-insensitive symbol match.
            _byNameOrAlias.TryAdd(group.Key, group.First());
        }
    }
}
