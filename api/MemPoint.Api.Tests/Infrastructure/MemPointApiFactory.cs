using MemPoint.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace MemPoint.Api.Tests.Infrastructure;

public sealed class MemPointApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string UserEmail = "test@example.com";
    public const string UserPassword = "Test-Password-123!";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:18").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
    }

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var result = await users.CreateAsync(
            new ApplicationUser { UserName = UserEmail, Email = UserEmail }, UserPassword);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(",", result.Errors.Select(e => e.Description)));
    }

    public HttpClient CreateHttpsClient() =>
    CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }
}