var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AuthService>("AuthService");

builder.Build().Run();
