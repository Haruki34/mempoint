using Microsoft.AspNetCore.Identity;

namespace MemPoint.Api.Data;

public class ApplicationUser : IdentityUser<Guid>
{
    public string TimeZone { get; set; } = "Asia/Tokyo";
    public DateTimeOffset CreatedAt { get; set; }
}