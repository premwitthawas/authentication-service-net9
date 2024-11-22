using AuthService.Data;
using AuthService.Helpers;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDbContext")));
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ISendMail, SendMail>();
builder.Services.AddScoped<IPasswordHashedHelper, PasswordHashedHelper>();
builder.Services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVerifyEmailRepository, VerifyEmailRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVerifyEmailService, VerifyEmailService>();
builder.Services.AddScoped<IAuthApplicationService, AuthApplicationService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => new { Message = "API RUNNING 🚀" });
app.MapHealthChecks("/health");

app.Run();
