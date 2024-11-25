using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public interface IResetPasswordRepository
{
    Task<ResetPasswordToken> InsertResetPasswordTokenAsync(ResetPasswordToken resetPasswordToken);
    Task<ResetPasswordToken> SelectResetPasswordTokenByTokenAsync(string token);
    Task<ResetPasswordToken> SelectResetPasswordTokenByUserIdAsync(Guid userId);
    Task<ResetPasswordToken> UpdateResetPasswordTokenAsync(string token, Guid userId);
    Task DeleteResetPasswordTokenByTokenIdAsync(Guid tokenId);
}


public class ResetPasswordRepository : IResetPasswordRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<ResetPasswordRepository> _logger;

    public ResetPasswordRepository(AuthDbContext context, ILogger<ResetPasswordRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task DeleteResetPasswordTokenByTokenIdAsync(Guid tokenId)
    {
        if (tokenId == Guid.Empty)
        {
            _logger.LogError("TokenId is null or empty ON DeleteResetPasswordTokenByTokenId Repository");
            throw new ArgumentNullException(nameof(tokenId), "TokenId is null or empty");
        }
        try
        {
            var resetPasswordToken = await _context.ResetPasswordTokens.FirstOrDefaultAsync(x => x.Id == tokenId);
            if (resetPasswordToken == null)
            {
                _logger.LogError("ResetPasswordToken not found ON DeleteResetPasswordTokenByTokenId Repository");
                return;
            }
            _context.ResetPasswordTokens.Remove(resetPasswordToken);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on DeleteResetPasswordTokenByTokenId Repository");
            throw;
        }
    }
    public async Task<ResetPasswordToken> InsertResetPasswordTokenAsync(ResetPasswordToken resetPasswordToken)
    {
        if (resetPasswordToken == null)
        {
            _logger.LogError("ResetPasswordToken is null ON InsertResetPasswordToken Repository");
            throw new ArgumentNullException(nameof(resetPasswordToken), "ResetPasswordToken is null");
        }
        try
        {
            var result = await _context.ResetPasswordTokens.AddAsync(resetPasswordToken);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on InsertResetPasswordToken Repository");
            throw;
        }
    }

    public async Task<ResetPasswordToken> SelectResetPasswordTokenByTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Token is null or empty ON SelectResetPasswordTokenByToken Repository");
            throw new ArgumentNullException(nameof(token), "Token is null or empty");
        }
        try
        {
            return await _context.ResetPasswordTokens.FirstOrDefaultAsync(x => x.Token == token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on SelectResetPasswordTokenByToken Repository");
            throw;
        }
    }

    public async Task<ResetPasswordToken> SelectResetPasswordTokenByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogError("UserId is null or empty ON SelectResetPasswordTokenByUserId Repository");
            throw new ArgumentNullException(nameof(userId), "UserId is null or empty");
        }
        try
        {
            return await _context.ResetPasswordTokens.FirstOrDefaultAsync(x => x.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on SelectResetPasswordTokenByUserId Repository");
            throw;
        }
    }

    public async Task<ResetPasswordToken> UpdateResetPasswordTokenAsync(string token, Guid userId)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Token is null or empty ON UpdateResetPasswordToken Repository");
            throw new ArgumentNullException(nameof(token), "Token is null or empty");
        }
        if (userId == Guid.Empty)
        {
            _logger.LogError("UserId is null or empty ON UpdateResetPasswordToken Repository");
            throw new ArgumentNullException(nameof(userId), "UserId is null or empty");
        }
        try
        {
            var existingResetPasswordToken = await _context.ResetPasswordTokens.FirstOrDefaultAsync(x => x.UserId == userId);
            if (existingResetPasswordToken == null)
            {
                _logger.LogError("ResetPasswordToken not found ON UpdateResetPasswordToken Repository");
                return null;
            }
            existingResetPasswordToken.Token = token;
            existingResetPasswordToken.UpdatedAt = DateTime.UtcNow;
            existingResetPasswordToken.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();
            return existingResetPasswordToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on UpdateResetPasswordToken Repository");
            throw;
        }
    }
}