using System.Security.Claims;
using BlazorTerminal.Api.Data;
using BlazorTerminal.Api.Models;
using BlazorTerminal.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace BlazorTerminal.Api.Extensions;

public record RegisterRequest(string Username, string Password);

public class AuthEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login and get JWT token")
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetMe)
            .WithName("GetMe")
            .WithSummary("Get current user info")
            .RequireAuthorization()
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Results<Created<UserDto>, ProblemHttpResult>> Register(
        RegisterRequest request,
        AppDbContext db,
        JwtService jwtService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.Problem("Username and password are required", statusCode: 400);
        }

        if (request.Password.Length < 6)
        {
            return TypedResults.Problem("Password must be at least 6 characters", statusCode: 400);
        }

        var existing = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username, ct);
        if (existing is not null)
        {
            return TypedResults.Problem("Username already exists", statusCode: 400);
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BC.HashPassword(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/auth/me", new UserDto(user.Id, user.Username, user.CreatedAt));
    }

    private static async Task<Results<Ok<AuthResponse>, ProblemHttpResult>> Login(
        LoginRequest request,
        AppDbContext db,
        JwtService jwtService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.Problem("Username and password are required", statusCode: 400);
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username, ct);
        if (user is null || !BC.Verify(request.Password, user.PasswordHash))
        {
            return TypedResults.Problem("Invalid username or password", statusCode: 401);
        }

        var token = jwtService.GenerateToken(user.Username, user.Id);

        return TypedResults.Ok(new AuthResponse(token, jwtService.GetExpirySeconds(), user.Username));
    }

    private static async Task<Ok<UserDto>> GetMe(ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var username = user.Identity?.Name ?? throw new UnauthorizedAccessException();
        var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct)
            ?? throw new UnauthorizedAccessException();

        return TypedResults.Ok(new UserDto(dbUser.Id, dbUser.Username, dbUser.CreatedAt));
    }
}