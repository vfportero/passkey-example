using Microsoft.EntityFrameworkCore;

namespace passkey_example_backend.Data;

public class UserDb : DbContext
{
    public UserDb(DbContextOptions<UserDb> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<UserCredential> UserCredentials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Credentials)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
