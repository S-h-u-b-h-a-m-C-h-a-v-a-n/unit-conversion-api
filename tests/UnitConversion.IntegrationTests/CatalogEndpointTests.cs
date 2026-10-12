using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConversion.Api.Contracts;

namespace UnitConversion.IntegrationTests;

public sealed class CatalogEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>(
            "/api/v1/categories");

        Assert.Contains(categories!, c => c.Key == "length" && c.BaseUnit == "m" && c.UnitCount > 0);
        Assert.Contains(categories!, c => c.Key == "mass");
        Assert.Contains(categories!, c => c.Key == "temperature");
    }

    [Fact]
    public async Task GetCategoryUnits_ReturnsUnitsOfThatCategory()
    {
        var units = await _client.GetFromJsonAsync<List<UnitResponse>>(
            "/api/v1/categories/temperature/units");

        Assert.All(units!, u => Assert.Equal("temperature", u.Category));
        Assert.Contains(units!, u => u.Symbol == "°F");
    }

    [Theory]
    [InlineData("/api/v1/categories/luminosity")]
    [InlineData("/api/v1/categories/luminosity/units")]
    [InlineData("/api/v1/units?category=luminosity")]
    [InlineData("/api/v1/units/parsec")]
    public async Task UnknownResources_Return404(string url)
    {
        var response = await _client.GetAsync(new Uri(url, UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUnit_ByAlias_ReturnsUnit()
    {
        var unit = await _client.GetFromJsonAsync<UnitResponse>("/api/v1/units/feet");

        Assert.Equal("ft", unit!.Symbol);
        Assert.Equal("length", unit.Category);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var body = await _client.GetStringAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal("Healthy", body);
    }
}
