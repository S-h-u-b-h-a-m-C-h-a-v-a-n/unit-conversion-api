using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConversion.Api.Contracts;

namespace UnitConversion.IntegrationTests;

public sealed class ConversionsEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_ValidConversion_ReturnsResult()
    {
        var response = await _client.GetFromJsonAsync<ConversionResponse>(
            "/api/v1/conversions?value=100&from=C&to=F");

        Assert.NotNull(response);
        Assert.Equal(212m, response.Result);
        Assert.Equal("°C", response.From.Symbol);
        Assert.Equal("°F", response.To.Symbol);
        Assert.Equal("temperature", response.Category);
    }

    [Fact]
    public async Task Get_EncodedSymbols_AreAccepted()
    {
        var url = $"/api/v1/conversions?value=1&from={Uri.EscapeDataString("m²")}&to={Uri.EscapeDataString("ft²")}&decimals=3";

        var response = await _client.GetFromJsonAsync<ConversionResponse>(url);

        Assert.Equal(10.764m, response!.Result);
    }

    [Theory]
    [InlineData(0, "33")]
    [InlineData(2, "32.81")]
    [InlineData(5, "32.80840")]
    public async Task Get_Decimals_RoundsResult(int decimals, string expected)
    {
        var response = await _client.GetFromJsonAsync<ConversionResponse>(
            $"/api/v1/conversions?value=10&from=m&to=ft&decimals={decimals}");

        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), response!.Result);
    }

    [Theory]
    [InlineData("/api/v1/conversions?from=m&to=ft", "Value")]
    [InlineData("/api/v1/conversions?value=1&to=ft", "From")]
    [InlineData("/api/v1/conversions?value=abc&from=m&to=ft", "Value")]
    [InlineData("/api/v1/conversions?value=1&from=m&to=ft&decimals=99", "Decimals")]
    public async Task Get_InvalidInput_ReturnsValidationProblem(string url, string field)
    {
        var response = await _client.GetAsync(new Uri(url, UriKind.Relative));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }

    [Theory]
    [InlineData("value=1&from=parsec&to=m", HttpStatusCode.BadRequest, "Unknown unit")]
    [InlineData("value=1&from=kg&to=m", HttpStatusCode.BadRequest, "Incompatible units")]
    [InlineData("value=-300&from=C&to=K", HttpStatusCode.UnprocessableEntity, "Invalid value")]
    public async Task Get_DomainErrors_ReturnProblemDetails(string query, HttpStatusCode status, string title)
    {
        var response = await _client.GetAsync(
            new Uri($"/api/v1/conversions?{query}", UriKind.Relative));

        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(title, problem!.Title);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }

    [Fact]
    public async Task PostBatch_MixedItems_ReportsEachItemIndependently()
    {
        var request = new BatchConversionRequest
        {
            Items =
            [
                new ConversionRequest { Value = 1, From = "mi", To = "km" },
                new ConversionRequest { Value = 1, From = "kg", To = "m" },
            ],
            Decimals = 3,
        };

        var response = await _client.PostAsJsonAsync("/api/v1/conversions/batch", request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<BatchConversionResponse>();
        Assert.Equal(1, body!.Succeeded);
        Assert.Equal(1, body.Failed);
        Assert.Equal(1.609m, body.Items[0].Conversion!.Result);
        Assert.False(body.Items[1].Succeeded);
        Assert.NotNull(body.Items[1].Error);
    }

    [Fact]
    public async Task PostBatch_EmptyItems_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/conversions/batch", new BatchConversionRequest { Items = [] });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostBatch_TooManyItems_ReturnsValidationProblem()
    {
        var items = Enumerable.Range(0, 101).Select(_ => new ConversionRequest { Value = 1, From = "m", To = "ft" }).ToList();

        var response = await _client.PostAsJsonAsync(
            "/api/v1/conversions/batch", new BatchConversionRequest { Items = items });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
