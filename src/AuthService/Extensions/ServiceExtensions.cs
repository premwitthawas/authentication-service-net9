using AuthService.Helpers;
using AuthService.Repositories;
using AuthService.Services;
namespace AuthService.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVerifyEmailService, VerifyEmailService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();
        services.AddScoped<IAuthApplicaitonService, AuthApplicaitonService>();
        services.AddScoped<ISessionApplicationService, SessionApplicationService>();
        return services;
    }

    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVerifyEmailRepository, VerifyEmailRepository>();
        services.AddScoped<IResetPasswordRepository, ResetPasswordRepository>();
        services.AddScoped<ISessionApplicationRepository, SessionApplicationRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationHelpers(this IServiceCollection services)
    {
        services.AddScoped<ISendMail, SendMail>();
        services.AddScoped<IPasswordHashedHelper, PasswordHashedHelper>();
        services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
        services.AddScoped<IRedisHelper, RedisHelper>();
        return services;
    }
}