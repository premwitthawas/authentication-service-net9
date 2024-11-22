using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;


public interface IVerifyEmailRepository
{
    Task<VerifyEmail> InsertVerifyEmail(VerifyEmail verifyEmail);
    Task<bool> UpdateVerifyEmailToken(Guid tokenId, string token, DateTime expiresAt);
    Task<VerifyEmail> SelectVerifyByUserId(Guid userId);
    Task<bool> DeleteVerifyEmailAsync(Guid tokenId);
    Task<VerifyEmail> SelectVerifyByToken(string token);
}

public class VerifyEmailRepository : IVerifyEmailRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<VerifyEmailRepository> _logger;
    public VerifyEmailRepository(AuthDbContext context, ILogger<VerifyEmailRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> DeleteVerifyEmailAsync(Guid tokenId)
    {
        if (tokenId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(tokenId), "Token ID cannot be empty.");
        }
        try
        {
            var verifyEmail = await this._context.VerifyEmails.FirstOrDefaultAsync(x => x.Id == tokenId);
            if (verifyEmail == null)
            {
                return false;
            }
            this._context.VerifyEmails.Remove(verifyEmail);
            await this._context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while deleting verify email.");
            throw;
        }
    }

    public async Task<VerifyEmail> InsertVerifyEmail(VerifyEmail verifyEmail)
    {
        if (verifyEmail == null)
        {
            throw new ArgumentNullException(nameof(verifyEmail), "VerifyEmail cannot be null.");
        }
        try
        {
            var result = await this._context.VerifyEmails.AddAsync(verifyEmail);
            await this._context.SaveChangesAsync();
            return result.Entity;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while inserting verify email.");
            throw;
        }
    }

    public async Task<VerifyEmail> SelectVerifyByToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            this._logger.LogError("Token is null or empty");
            throw new ArgumentNullException(nameof(token), "Token cannot be null or empty.");
        }
        try
        {
            return await this._context.VerifyEmails.FirstOrDefaultAsync(x => x.Token == token);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while selecting verify email by token.");
            throw;
        }
    }

    public async Task<VerifyEmail> SelectVerifyByUserId(Guid userId)
    {
        var result = await this._context.VerifyEmails.FirstOrDefaultAsync(x => x.UserId == userId);
        if (result == null)
        {
            return null;
        }
        return result;
    }
    public async Task<bool> UpdateVerifyEmailToken(Guid tokenId, string token, DateTime expiresAt)
    {
        var verifyEmail = await this._context.VerifyEmails.FirstOrDefaultAsync(x => x.Id == tokenId);
        if (verifyEmail == null)
        {
            return false;
        }
        verifyEmail.Token = token;
        verifyEmail.ExpiresAt = expiresAt;
        await this._context.SaveChangesAsync();
        return true;
    }
}