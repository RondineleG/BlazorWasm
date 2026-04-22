using System.Text;
using BlazorTerminal.Api.Data;
using BlazorTerminal.Api.Extensions;
using BlazorTerminal.Api.Services;
using BlazorTerminal.Api.Terminal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=terminal.db"));

// JWT Auth
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "MrBurTerminalSecretKey2026!@#$%";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // For WebSocket, token can come as query string
                if (context.Request.Path.StartsWithSegments("/ws/terminal"))
                {
                    var token = context.Request.Query["token"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Services
builder.Services.AddSingleton(sp =>
{
    var sshConfig = new SshConnectionInfo
    {
        Host = builder.Configuration["Ssh:Host"] ?? "172.245.152.43",
        Username = builder.Configuration["Ssh:Username"] ?? "root",
        KeyPath = builder.Configuration["Ssh:KeyPath"] ?? "/root/.ssh/mrbur_deploy",
        Port = int.Parse(builder.Configuration["Ssh:Port"] ?? "22")
    };
    return new ConnectionManager(sshConfig);
});

builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<TerminalWsHandler>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();

public partial class Program { }