using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;

namespace AuthService.Services;
public interface ISessionApplicationService
{
    Task<ResponseServiceDto<ResponseCreateSessionDto>> createSessionAsync(CreateSessionDto sessionDto);
};

public class SessionApplicationService : ISessionApplicationService
{
    private readonly ISessionApplicationRepository _sessionApplicationRepository;
    private readonly ILogger<SessionApplicationService> _logger;

    public SessionApplicationService(ISessionApplicationRepository sessionApplicationRepository, ILogger<SessionApplicationService> logger)
    {
        _sessionApplicationRepository = sessionApplicationRepository;
        _logger = logger;
    }

    public async Task<ResponseServiceDto<ResponseCreateSessionDto>> createSessionAsync(CreateSessionDto sessionDto)
    {
        if (sessionDto == null)
        {
            _logger.LogError("Session is null in {MethodName}", nameof(createSessionAsync));
            return new ResponseServiceDto<ResponseCreateSessionDto>(null, "Session is null", false, 400);
        }
        try
        {
            var existingSession = await _sessionApplicationRepository.GetSessionByUserIdAsync(sessionDto.UserId);
            if (existingSession != null)
            {
                existingSession.IsRevoked = true;
                existingSession.RevokedAt = DateTime.UtcNow;
                await _sessionApplicationRepository.RevokeSessionAsyncById(existingSession.Id);
            }
            var session = new Session
            {
                Provider = sessionDto.Provider,
                UserId = sessionDto.UserId,
                AccessToken = sessionDto.AccessToken,
                RefreshToken = sessionDto.RefreshToken,
                AccessExpiresAt = sessionDto.AccessExpiresAt,
                RefreshExpiresAt = sessionDto.RefreshExpiresAt,
            };
            var result = await _sessionApplicationRepository.InsertSessionAsync(session);
            _logger.LogInformation("Session created successfully in {MethodName}", nameof(createSessionAsync));
            return new ResponseServiceDto<ResponseCreateSessionDto>(new ResponseCreateSessionDto(result.Id.ToString()), "Session inserted successfully", true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {MethodName}", nameof(createSessionAsync));
            return new ResponseServiceDto<ResponseCreateSessionDto>(null, ex.Message, false, 500);
        }
    }
}