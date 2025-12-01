using AspireSample.ApiService.Hubs;
using AspireSample.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AspireSample.ApiService;

public class Program
{
    public static void Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);
        Console.WriteLine("JWT KEY => " + builder.Configuration["Jwt:Key"]);



        // -------------------------------------------------
        // 1️⃣ Register framework + app services
        // -------------------------------------------------
        builder.Services.AddProblemDetails();
        builder.Services.AddSignalR();
        builder.Services.AddControllers();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Health")));

        // -------------------------------------------------
        // 2️⃣ Authentication + Authorization
        // -------------------------------------------------
      builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                ,
                ValidateIssuer = false,
                ValidateAudience = false
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/chathub")))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

builder.Services.AddAuthorization();


        builder.Services.AddOpenApi();

        // -------------------------------------------------
        // 3️⃣ Add Aspire DEFAULTS (MUST BE LAST)
        // -------------------------------------------------
        builder.AddServiceDefaults();

        // -------------------------------------------------
        // Build app
        // -------------------------------------------------
        var app = builder.Build();

        // -------------------------------------------------
        // Middleware (correct order)
        // -------------------------------------------------
        app.UseExceptionHandler();

        app.UseRouting();           // 🔥 REQUIRED before auth
        app.UseAuthentication();    // 🔥 Auth must come after routing
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // -------------------------------------------------
        // Endpoints
        // -------------------------------------------------
        app.MapControllers();
        app.MapHub<ChatHub>("/chathub");

        // Aspire health, metrics, dashboard
        app.MapDefaultEndpoints();

        app.Run();
    }
}
