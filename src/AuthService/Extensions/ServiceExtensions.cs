using AuthService.Repositories;
using AuthService.Services;
namespace AuthService.Helpers;
public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVerifyEmailService, VerifyEmailService>();
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        return services;
    }

    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVerifyEmailRepository, VerifyEmailRepository>();
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