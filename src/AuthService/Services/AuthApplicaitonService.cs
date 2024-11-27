using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Schemas;

namespace AuthService.Services;

public interface IAuthApplicaitonService
{
    Task<ResponseServiceDto<ReponseLoginDto>> LoginAsync(LoginDto loginDto);
}

public class AuthApplicaitonService : IAuthApplicaitonService
{
    private readonly ILogger<AuthApplicaitonService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    private readonly IPasswordHashedHelper _passwordHashedHelper;
    private readonly IRedisHelper _redisHelper;
    private readonly ISessionApplicationRepository _sessionApplicationRepository;
    public AuthApplicaitonService(ILogger<AuthApplicaitonService> logger, IUserRepository userRepository, IPasswordHashedHelper passwordHashedHelper, IJwtTokenHelper jwtTokenHelper, IRedisHelper redisHelper, ISessionApplicationRepository sessionApplicationRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordHashedHelper = passwordHashedHelper;
        _jwtTokenHelper = jwtTokenHelper;
        _redisHelper = redisHelper;
        _sessionApplicationRepository = sessionApplicationRepository;
    }
    public async Task<ResponseServiceDto<ReponseLoginDto>> LoginAsync(LoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
        {
            _logger.LogError("Username or password is null or empty");
            return new ResponseServiceDto<ReponseLoginDto>(null, "Username or password is null or empty", false, 400);
        }
        try
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var user = await _userRepository.SelectUserByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                _logger.LogError("User not found for username: {Username}", loginDto.Username);
                return new ResponseServiceDto<ReponseLoginDto>(null, "User not found", false, 404);
            }

            if (!ValidatePassword(loginDto.Password, user.Password))
            {
                _logger.LogError("Password is incorrect for username: {Username}", loginDto.Username);
                return new ResponseServiceDto<ReponseLoginDto>(null, "Password is incorrect", false, 400);
            }

            var sessionInDb = await _sessionApplicationRepository.GetSessionByUserIdAsync(user.Id);
            if (sessionInDb == null)
            {
                return await CreateNewSession(user);
            }
            if (sessionInDb != null)
            {
                if (sessionInDb.IsRefreshTokenExpired())
                {
                    await _sessionApplicationRepository.RevokeSessionAsyncById(sessionInDb.Id);
                    return await CreateNewSession(user);
                }
            }
            return await HandleExistingSession(loginDto, user, sessionInDb);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while logging in");
            return new ResponseServiceDto<ReponseLoginDto>(null, "An error occurred while logging in", false, 500);
        }
    }
    private bool ValidatePassword(string inputPassword, string storedPassword)
    {
        return _passwordHashedHelper.ValidatePassword(inputPassword, storedPassword);
    }
    private async Task<ResponseServiceDto<ReponseLoginDto>> CreateNewSession(User user)
    {
        var newSession = await _sessionApplicationRepository.InsertSessionAsync(new Session
        {
            TokenType = "Bearer",
            Provider = "Local",
            UserId = user.Id,
            AccessToken = _jwtTokenHelper.GenerateJwtAccessToken(user.Id.ToString()),
            RefreshToken = _jwtTokenHelper.GenerateJwtRefeshToken(user.Id.ToString()),
            AccessExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        var sessionRedisNew = new SessionRedis
        {
            UserId = user.Id.ToString(),
            SessionId = newSession.Id.ToString(),
            AccessToken = newSession.AccessToken,
            RefreshToken = newSession.RefreshToken,
            AccessTokenExpires = newSession.AccessExpiresAt,
            RefreshTokenExpires = newSession.RefreshExpiresAt
        };
        await _redisHelper.SetAsync($"session:{newSession.Id}", sessionRedisNew, newSession.RefreshExpiresAt - DateTime.UtcNow);
        return new ResponseServiceDto<ReponseLoginDto>(new ReponseLoginDto(sessionRedisNew.SessionId), "Login successful", true, 200);
    }
    private async Task<ResponseServiceDto<ReponseLoginDto>> HandleExistingSession(LoginDto loginDto, User user, Session sessionInDb)
    {
        var sessionKeyRedis = $"session:{sessionInDb.Id}";
        var sessionRedis = await _redisHelper.GetAsync<SessionRedis>(sessionKeyRedis);
        if (sessionRedis == null)
        {
            _logger.LogWarning("Session not found in Redis. Fetching from DB: {SessionId}", sessionInDb.Id);
            sessionRedis = new SessionRedis
            {
                UserId = user.Id.ToString(),
                SessionId = sessionInDb.Id.ToString(),
                AccessToken = sessionInDb.AccessToken,
                RefreshToken = sessionInDb.RefreshToken,
                AccessTokenExpires = sessionInDb.AccessExpiresAt,
                RefreshTokenExpires = sessionInDb.RefreshExpiresAt
            };
            await _redisHelper.SetAsync(sessionKeyRedis, sessionRedis, TimeSpan.FromDays(1));
        }
        if (sessionRedis != null)
        {
            if (sessionRedis.IsAccessTokenExpired())
            {
                if (sessionRedis.IsRefreshTokenExpired())
                {
                    _logger.LogError("Refresh token is expired for username: {Username}", loginDto.Username);
                    await _redisHelper.RemoveAsync(sessionKeyRedis); // ลบ session ออกจาก Redis
                    await _sessionApplicationRepository.RevokeSessionAsyncById(sessionInDb.Id); // อัปเดตสถานะใน DB
                    return await CreateNewSession(user); // สร้าง session ใหม่
                }
                _logger.LogInformation("Access token expired. Generating new access token for username: {Username}", loginDto.Username);
                sessionRedis.AccessToken = _jwtTokenHelper.GenerateJwtAccessToken(user.Id.ToString());
                await _redisHelper.SetAsync(sessionKeyRedis, sessionRedis);
            }
            return new ResponseServiceDto<ReponseLoginDto>(new ReponseLoginDto(sessionRedis.SessionId), "Login successful", true, 200);
        }
        return new ResponseServiceDto<ReponseLoginDto>(new ReponseLoginDto(sessionInDb.Id.ToString()), "Login successful", true, 200);
    }
}