using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;
public interface ISessionApplicationRepository
{
    Task<Session> InsertSessionAsync(Session session);
    Task<Session> GetSessionByUserIdAsync(Guid UserId);
    Task<bool> DeleteSessionAsync(Guid tokenId);
    Task<bool> RevokeSessionAsyncById(Guid tokenId);
};
public class SessionApplicationRepository : ISessionApplicationRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<SessionApplicationRepository> _logger;
    public SessionApplicationRepository(AuthDbContext context, ILogger<SessionApplicationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<bool> DeleteSessionAsync(Guid tokenId)
    {
        if (tokenId == Guid.Empty)
        {
            _logger.LogError("TokenId is empty On {MethodName}", nameof(DeleteSessionAsync));
            return false;
        }
        try {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == tokenId);
            if (session == null)
            {
                return false;
            }
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Session Deleted Successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error On {MethodName}", nameof(DeleteSessionAsync));
            return false;
        }
    }

    public async Task<Session> GetSessionByUserIdAsync(Guid UserId)
    {
        if (UserId == Guid.Empty)
        {
            _logger.LogError("UserId is empty On {MethodName}", nameof(GetSessionByUserIdAsync));
            return null;
        }
        try
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.UserId == UserId);
            if (session == null)
            {
                return null;
            }
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error On {MethodName}", nameof(GetSessionByUserIdAsync));
            return null;
        }

    }

    public async Task<Session> InsertSessionAsync(Session session)
    {
        if (session == null)
        {
            _logger.LogError("Session is null On InsertSessionAsync");
            throw new ArgumentNullException(nameof(session));
        }
        try
        {
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Session Inserted Successfully");
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error On InsertSessionAsync");
            throw new Exception(ex.Message);
        }
    }

    public async Task<bool> RevokeSessionAsyncById(Guid tokenId)
    {
        if (tokenId == Guid.Empty)
        {
            _logger.LogError("TokenId is empty On RevokeSessionAsync");
            return false;
        }
        try
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == tokenId);
            if (session == null)
            {
                return false;
            }
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Session Revoked Successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error On RevokeSessionAsync");
            return false;
        }
    }
}