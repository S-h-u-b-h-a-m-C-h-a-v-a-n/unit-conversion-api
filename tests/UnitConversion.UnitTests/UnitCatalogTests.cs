using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Domain;
using UnitConversion.Core.Exceptions;
using UnitConversion.Core.Services;
using UnitConversion.Infrastructure;

namespace UnitConversion.UnitTests;

public sealed class UnitCatalogTests
{
    private readonly UnitCatalog _catalog = new(HardcodedUnitProviders.All);

    [Fact]
    public void ShippedCatalog_ContainsRequiredCategories()
    {
        var keys = _catalog.Categories.Select(c => c.Key).ToArray();

        Assert.Contains("length", keys);
        Assert.Contains("temperature", keys);
        Assert.Contains("mass", keys);
    }

    [Theory]
    [InlineData("ft", "ft")]
    [InlineData("FT", "ft")]
    [InlineData("feet", "ft")]
    [InlineData("Foot", "ft")]
    [InlineData("tb", "TB")]
    public void GetUnit_ResolvesSymbolsNamesAndAliasesIgnoringCase(string input, string expectedSymbol)
    {
        Assert.Equal(expectedSymbol, _catalog.GetUnit(input).Symbol);
    }

    [Theory]
    [InlineData("Mb", "megabit")]
    [InlineData("MB", "megabyte")]
    [InlineData("b", "bit")]
    [InlineData("B", "byte")]
    public void GetUnit_SymbolsDifferingOnlyByCase_RequireExactCase(string symbol, string expectedName)
    {
        Assert.Equal(expectedName, _catalog.GetUnit(symbol).Name);
    }

    [Fact]
    public void GetUnit_AmbiguousCaseInsensitiveSymbol_ThrowsWithSuggestions()
    {
        var ex = Assert.Throws<UnitNotFoundException>(() => _catalog.GetUnit("mb"));

        Assert.Contains("Mb", ex.Message, StringComparison.Ordinal);
        Assert.Contains("MB", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetUnits_UnknownCategory_Throws()
    {
        Assert.Throws<CategoryNotFoundException>(() => _catalog.GetUnits("luminosity"));
    }

    [Fact]
    public void Constructor_DuplicateSymbol_FailsFast()
    {
        var provider = new TestProvider(
            new UnitCategory("x", "X", "a"),
            UnitDefinition.Linear("x", "a", "alpha", 1m),
            UnitDefinition.Linear("x", "a", "another alpha", 2m));

        var ex = Assert.Throws<InvalidOperationException>(() => new UnitCatalog([provider]));
        Assert.Contains("'a'", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_DuplicateAliasAcrossUnits_FailsFast()
    {
        var provider = new TestProvider(
            new UnitCategory("x", "X", "a"),
            UnitDefinition.Linear("x", "a", "alpha", 1m, "shared"),
            UnitDefinition.Linear("x", "b", "beta", 2m, "SHARED"));

        Assert.Throws<InvalidOperationException>(() => new UnitCatalog([provider]));
    }

    [Fact]
    public void Constructor_MissingBaseUnit_FailsFast()
    {
        var provider = new TestProvider(new UnitCategory("x", "X", "zzz"), UnitDefinition.Linear("x", "a", "alpha", 1m));

        Assert.Throws<InvalidOperationException>(() => new UnitCatalog([provider]));
    }

    [Fact]
    public void Constructor_UnitInWrongCategory_FailsFast()
    {
        var provider = new TestProvider(new UnitCategory("x", "X", "a"), UnitDefinition.Linear("y", "a", "alpha", 1m));

        Assert.Throws<InvalidOperationException>(() => new UnitCatalog([provider]));
    }

    [Fact]
    public void UnitDefinition_ZeroFactor_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => UnitDefinition.Linear("x", "a", "alpha", 0m));
    }

    private sealed class TestProvider(UnitCategory category, params UnitDefinition[] units) : IUnitDefinitionProvider
    {
        public UnitCategory Category { get; } = category;

        public IEnumerable<UnitDefinition> GetUnits() => units;
    }
}
