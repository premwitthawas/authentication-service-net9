using System.Text.RegularExpressions;
using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<User> InsertUser(User user);
    Task<User> SelectUserByEmail(string email);
    Task<User> SelectUserByUsername(string username);
    Task<User> SelectUserById(Guid id);
    Task<bool> UpdateVerifyEmail(Guid userId);
}
public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UserRepository> _logger;
    public UserRepository(AuthDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<User> SelectUserByEmail(string email)
    {
        return await this._context.Users.FirstOrDefaultAsync<User>(u => u.Email == email);
    }
    public async Task<User> SelectUserById(Guid id)
    {
        if (string.IsNullOrWhiteSpace(id.ToString()))
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null or empty.");
        }
        try
        {
            var user = await this._context.Users.FirstOrDefaultAsync<User>(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while getting user by Id.");
            throw;
        }
    }
    public async Task<User> SelectUserByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");
        }
        try
        {
            var user = await this._context.Users.FirstOrDefaultAsync<User>(u => u.UserName == username);
            if (user == null)
            {
                return null;
            }
            return user;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while getting user by UserName.");
            throw;
        }
    }
    public async Task<User> InsertUser(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }
        try
        {
            var result = await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while inserting user.");
            throw;
        }
    }

    public async Task<bool> UpdateVerifyEmail(Guid userId)
    {
        if (string.IsNullOrWhiteSpace(userId.ToString()))
        {
            this._logger.LogError("UserId is null or empty");
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
        }
        try
        {
            var user = await this._context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                this._logger.LogError("User not found for ID: {UserId}", userId);
                return false;
            }
            user.IsVerified = true;
            await this._context.SaveChangesAsync();
            this._logger.LogInformation("User verify email updated for ID: {UserId}", userId);
            return true;
        }
        catch (System.Exception)
        {
            this._logger.LogError("Error occurred while updating user verify email.");
            throw;
        }
    }
}