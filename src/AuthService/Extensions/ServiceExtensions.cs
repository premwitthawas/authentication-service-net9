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
        return services;
    }

    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVerifyEmailRepository, VerifyEmailRepository>();
        services.AddScoped<IResetPasswordRepository, ResetPasswordRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationHelpers(this IServiceCollection services)
    {
        services.AddScoped<ISendMail, SendMail>();
        services.AddScoped<IPasswordHashedHelper, PasswordHashedHelper>();
        services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
        return services;
    }
}