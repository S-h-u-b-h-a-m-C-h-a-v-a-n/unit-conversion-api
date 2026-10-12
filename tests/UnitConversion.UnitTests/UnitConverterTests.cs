using UnitConversion.Core.Exceptions;
using UnitConversion.Core.Services;
using UnitConversion.Infrastructure;

namespace UnitConversion.UnitTests;

public sealed class UnitConverterTests
{
    private readonly UnitCatalog _catalog = new(HardcodedUnitProviders.All);
    private readonly UnitConverter _converter;

    public UnitConverterTests() => _converter = new UnitConverter(_catalog);

    [Theory]
    // Length
    [InlineData(1, "m", "ft", "3.2808398950131233595800524934")]
    [InlineData(1, "in", "cm", "2.54")]
    [InlineData(1, "mi", "km", "1.609344")]
    [InlineData(5280, "ft", "mi", "1")]
    // Mass
    [InlineData(1, "kg", "lb", "2.2046226218487758072297380135")]
    [InlineData(16, "oz", "lb", "1")]
    [InlineData(14, "lb", "st", "1")]
    // Temperature
    [InlineData(100, "°C", "°F", "212")]
    [InlineData(32, "°F", "°C", "0")]
    [InlineData(-40, "°C", "°F", "-40")]
    [InlineData(0, "K", "°C", "-273.15")]
    [InlineData(0, "°F", "°R", "459.67")]
    // Others
    [InlineData(1, "gal", "L", "3.785411784")]
    [InlineData(1, "ha", "m²", "10000")]
    [InlineData(1, "d", "h", "24")]
    [InlineData(36, "km/h", "m/s", "10")]
    [InlineData(1, "GiB", "MiB", "1024")]
    [InlineData(8, "b", "B", "1")]
    public void Convert_KnownValues_ReturnsExpectedResult(double value, string from, string to, string expected)
    {
        var result = _converter.Convert((decimal)value, from, to);

        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), result.Result, 20);
    }

    [Theory]
    [InlineData("celsius", "fahrenheit")]
    [InlineData("C", "F")]
    [InlineData("degC", "degF")]
    [InlineData(" °C ", "°F")]
    public void Convert_AcceptsNamesAndAliases(string from, string to)
    {
        var result = _converter.Convert(100m, from, to);

        Assert.Equal(212m, result.Result);
        Assert.Equal("°C", result.From.Symbol);
        Assert.Equal("°F", result.To.Symbol);
    }

    [Fact]
    public void Convert_SameUnit_ReturnsInputUnchanged()
    {
        var result = _converter.Convert(123.456m, "m", "metre");

        Assert.Equal(123.456m, result.Result);
    }

    [Fact]
    public void Convert_DifferentCategories_Throws()
    {
        var ex = Assert.Throws<IncompatibleUnitsException>(() => _converter.Convert(1m, "kg", "m"));

        Assert.Contains("different categories", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Convert_UnknownUnit_Throws()
    {
        var ex = Assert.Throws<UnitNotFoundException>(() => _converter.Convert(1m, "parsec", "m"));

        Assert.Equal("parsec", ex.Unit);
    }

    [Theory]
    [InlineData(-273.16, "°C")]
    [InlineData(-1, "K")]
    [InlineData(-460, "°F")]
    public void Convert_BelowAbsoluteZero_Throws(double value, string from)
    {
        Assert.Throws<InvalidConversionValueException>(() => _converter.Convert((decimal)value, from, "K"));
    }

    [Fact]
    public void Convert_AbsoluteZero_IsAllowed()
    {
        Assert.Equal(0m, _converter.Convert(-273.15m, "°C", "K").Result);
    }

    [Fact]
    public void Convert_ResultOutsideDecimalRange_ThrowsInvalidValue()
    {
        Assert.Throws<InvalidConversionValueException>(() => _converter.Convert(decimal.MaxValue, "km", "nm"));
    }

    [Fact]
    public void Convert_RoundTripsEveryUnitThroughEveryOtherUnitInItsCategory()
    {
        foreach (var category in _catalog.Categories)
        {
            var units = _catalog.GetUnits(category.Key);
            foreach (var from in units)
            {
                foreach (var to in units)
                {
                    var there = _converter.Convert(42.5m, from.Symbol, to.Symbol).Result;
                    var back = _converter.Convert(there, to.Symbol, from.Symbol).Result;

                    Assert.True(
                        Math.Abs(back - 42.5m) < 0.000000001m,
                        $"{from.Symbol} -> {to.Symbol} -> {from.Symbol} returned {back}");
                }
            }
        }
    }
}
