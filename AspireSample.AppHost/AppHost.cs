var builder = DistributedApplication.CreateBuilder(args);

// API service
var apiService = builder
    .AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithEnvironment("ConnectionStrings:Health", "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=Health")
    .WithEnvironment("Jwt:Key", "super-very-long-key-at-least-64-characters-long-1234567890abcdef")
    .WithHttpHealthCheck("/health");

// Web (Blazor Server)
builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
