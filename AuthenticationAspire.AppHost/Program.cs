var builder = DistributedApplication.CreateBuilder(args);
var authenticationService = builder.AddProject<Projects.AuthService>("AuthenticationService");
builder.Build().Run();
