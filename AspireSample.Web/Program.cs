using AspireSample.Web;
using AspireSample.Web.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;



var builder = WebApplication.CreateBuilder(args);

// Aspire service discovery
builder.AddServiceDefaults();

// Blazor services
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddOutputCache();

// HttpClient using Aspire discovery
builder.Services.AddHttpClient<WeatherApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
});

// SignalR HubConnection using Aspire discovery
// SignalR HubConnection
builder.Services.AddSingleton<HubConnection>(sp =>
{
    // In Aspire, service URLs are injected into Configuration
    // Format: "services:{serviceName}:{endpointName}:0"
    var config = sp.GetRequiredService<IConfiguration>();

    // Attempt to get the URL. If it's null, fallback to a hardcoded string or throw error.
    var apiUrl = config[$"services:apiservice:http:0"]
                 ?? config[$"services:apiservice:https:0"]
                 ?? throw new InvalidOperationException("Could not resolve endpoint for 'apiservice'");

    return new HubConnectionBuilder()
        .WithUrl($"{apiUrl}/chathub") // Note: Ensure slash handling is correct
        .WithAutomaticReconnect()
        .Build();
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapDefaultEndpoints();

app.Run();
