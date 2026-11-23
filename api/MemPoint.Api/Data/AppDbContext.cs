using MemPoint.Api.Entries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MemPoint.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    public DbSet<Entry> Entries => Set<Entry>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>(u =>
        {
            u.ToTable("users", t =>
            t.HasCheckConstraint("ck_users_time_zone_length", "char_length(time_zone)<=64"));
            u.Property(x => x.TimeZone).HasDefaultValue("Asia/Tokyo");
            u.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            u.HasIndex(x => x.NormalizedUserName).HasDatabaseName("users_user_name_key");
            u.HasIndex(x => x.NormalizedEmail).HasDatabaseName("users_email_idx");
        });
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<Entry>(e =>
        {
            e.ToTable("entries", t =>
            {
                t.HasCheckConstraint("ck_entries_title_length", "char_length(title)<=100");
                t.HasCheckConstraint("ck_entries_body_length", "char_length(body)BETWEEN 1 AND 10000");
            });
            e.Property(x => x.Id).UseIdentityAlwaysColumn();
            e.Property(x => x.IsBookmarked).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.UserId, x.EntryDate })
            .IsUnique()
            .HasDatabaseName("entries_user_date_key");
            e.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        });
    }
}