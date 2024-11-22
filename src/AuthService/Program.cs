using AuthService.Data;
using AuthService.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDbContext")));
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
//APPLICATION
builder.Services.AddApplicationHelpers();
builder.Services.AddApplicationRepositories();
builder.Services.AddApplicationServices();

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
