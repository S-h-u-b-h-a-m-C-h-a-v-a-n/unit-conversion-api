using UnitConversion.Core.Domain;

namespace UnitConversion.Core.Abstractions;

/// <summary>
/// Supplies the units of one category. This is the extension point for new data:
/// today providers are hardcoded, later they can read from a database, a JSON file
/// or a remote service without touching the conversion logic.
/// </summary>
public interface IUnitDefinitionProvider
{
    UnitCategory Category { get; }

    IEnumerable<UnitDefinition> GetUnits();
}
