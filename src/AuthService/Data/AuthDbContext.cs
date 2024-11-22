using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<VerifyEmail> VerifyEmails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
        modelBuilder.Entity<Role>().HasData(new Role { Id = 1, RoleName = "Admin" });
        modelBuilder.Entity<Role>().HasData(new Role { Id = 2, RoleName = "User" });
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}