using System.Net;
using System.Net.Http.Json;
using MemPoint.Api.Auth;
using MemPoint.Api.Tests.Infrastructure;

namespace MemPoint.Api.Tests.Auth;

[Collection(ApiCollection.Name)]
public class AuthTests(MemPointApiFactory factory)
{
    [Fact]
    public async Task Me_returns_401_without_login()
    {
        var client = factory.CreateHttpsClient();

        var response = await client.GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_400()
    {
        var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = MemPointApiFactory.UserEmail, password = "wrong" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_then_me_then_logout()
    {
        var client = factory.CreateHttpsClient();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = MemPointApiFactory.UserEmail, password = MemPointApiFactory.UserPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var me = await client.GetFromJsonAsync<MeResponse>("/api/me");
        Assert.Equal(MemPointApiFactory.UserEmail, me!.Email);
        Assert.Equal("Asia/Tokyo", me.TimeZone);

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var after = await client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }
}