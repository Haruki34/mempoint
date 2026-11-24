using System.Net;
using System.Net.Http.Json;
using MemPoint.Api.Tests.Infrastructure;

namespace MemPoint.Api.Tests.Security;

[Collection(ApiCollection.Name)]
public class CrossSiteRequestGuardTests(MemPointApiFactory factory)
{
    [Theory]
    [InlineData("Sec-Fetch-Site", "cross-site", HttpStatusCode.Forbidden)]
    [InlineData("Sec-Fetch-Site", "same-site", HttpStatusCode.Forbidden)]
    [InlineData("Sec-Fetch-Site", "same-origin", HttpStatusCode.BadRequest)]
    [InlineData("Sec-Fetch-Site", "none", HttpStatusCode.BadRequest)]
    [InlineData("Origin", "https://evil.example", HttpStatusCode.Forbidden)]
    [InlineData("Origin", "null", HttpStatusCode.Forbidden)]
    [InlineData("Origin", "https://localhost", HttpStatusCode.BadRequest)]
    public async Task Unsafe_requests_are_checked(string header, string value, HttpStatusCode expected)
    {
        var client = factory.CreateHttpsClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email = "nobody@example.com", password = "wrong" }),
        };
        request.Headers.Add(header, value);

        var response = await client.SendAsync(request);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Safe_methods_are_not_checked()
    {
        var client = factory.CreateHttpsClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add("Sec-Fetch-Site", "cross-site");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}