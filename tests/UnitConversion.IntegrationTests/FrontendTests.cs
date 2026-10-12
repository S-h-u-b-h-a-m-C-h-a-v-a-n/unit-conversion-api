using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UnitConversion.IntegrationTests;

public sealed class FrontendTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Root_ServesTheConverterPage()
    {
        var response = await _client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<title>Unit Converter</title>", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/css/site.css", "text/css")]
    [InlineData("/js/app.js", "text/javascript")]
    public async Task StaticAssets_AreServed(string path, string mediaType)
    {
        var response = await _client.GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(mediaType, response.Content.Headers.ContentType?.MediaType);
    }
}
