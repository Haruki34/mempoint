using System.Security.Claims;
using MemPoint.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace MemPoint.Api.Auth;

public record LoginRequest(string? Email, string? Password);
public record MeResponse(Guid Id, string Email, string TimeZone, DateOnly Today);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", Login).AllowAnonymous();
        app.MapPost("/api/auth/logout", Logout);
        app.MapGet("/api/me", Me);
    }

    private static async Task<IResult> Login(LoginRequest request, SignInManager<ApplicationUser> signInManager, TimeProvider clock)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return InvalidLogin();
        }

        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, isPersistent: true, lockoutOnFailure: true);
        if (!result.Succeeded) return InvalidLogin();

        var user = await signInManager.UserManager.FindByNameAsync(request.Email);
        return Results.Ok(ToMe(user!, clock));
    }

    private static async Task<IResult> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> Me(ClaimsPrincipal principal, UserManager<ApplicationUser> users, TimeProvider clock)
    {
        var user = await users.GetUserAsync(principal);
        return user is null ? Results.Unauthorized() : Results.Ok(ToMe(user, clock));
    }

    private static MeResponse ToMe(ApplicationUser user, TimeProvider clock)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
        var now = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone);
        return new MeResponse(user.Id, user.Email!, user.TimeZone, DateOnly.FromDateTime(now.DateTime));
    }

    private static IResult InvalidLogin() =>
        Results.Problem(title: "Invalid email or password.", statusCode: StatusCodes.Status400BadRequest);
}