using AspireSample.Web;
using System.Net;
using AspireSample.Web.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components.Authorization;
using AspireSample.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Register Blazor + core services
// ----------------------------
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddOutputCache();

// ----------------------------
// HTTP clients using Aspire service discovery
// ----------------------------
builder.Services.AddHttpClient<AuthService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var apiUrl =
        config["services:apiservice:https:0"] ??
        config["services:apiservice:http:0"] ??
        throw new InvalidOperationException("ApiService endpoint not found");

    client.BaseAddress = new Uri(apiUrl);
});



// ----------------------------
// Authentication / Authorization
// ----------------------------
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
    });

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// ----------------------------
// Application state singletons
// ----------------------------
builder.Services.AddScoped<ChatState>();

// ----------------------------
// SignalR Hub connection via Aspire discovery
// ----------------------------


// ----------------------------
// Add Aspire defaults (LAST)
// ----------------------------
builder.AddServiceDefaults();

var app = builder.Build();

// ----------------------------
// HTTP pipeline
// ----------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseOutputCache();

// Blazor UI endpoints
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Aspire health endpoints
app.MapDefaultEndpoints();

app.Run();
