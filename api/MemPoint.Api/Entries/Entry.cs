using MemPoint.Api.Data;

namespace MemPoint.Api.Entries;

public class Entry
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateOnly EntryDate { get; set; }
    public string? Title { get; set; }
    public string body { get; set; } = "";
    public bool IsBookmarked { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}