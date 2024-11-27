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
    public DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(new Permission
        {
            Id = 1,
            Name = "ReadOnly",
            Read = true,
            Write = false,
            Update = false,
            Delete = false
        });
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
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("admin"),
            IsVerified = true,
            RoleId = 1,
            PermissionId = 1
        });
        base.OnModelCreating(modelBuilder);
    }
}