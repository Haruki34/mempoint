using System.Text;
using MemPoint.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace MemPoint.Api.Auth;

public static class CreateUserCommand
{
    public static async Task<int> RunAsync(IServiceProvider services, string email)
    {
        var password = ReadSecret("Password: ");
        if (ReadSecret("Confirm Password:") != password)
        {
            Console.Error.WriteLine("Passwords do not match.");
            return 1;
        }

        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) Console.Error.WriteLine(error.Description);
            return 1;
        }

        Console.WriteLine($"Created user {email} ({user.Id}).");
        return 0;
    }

    private static string ReadSecret(string prompt)
    {
        Console.Write(prompt);
        var buffer = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return buffer.ToString(); }
            if (key.Key == ConsoleKey.Backspace) { if (buffer.Length > 0) buffer.Length--; }
            else if (!char.IsControl(key.KeyChar)) buffer.Append(key.KeyChar);
        }
    }
}