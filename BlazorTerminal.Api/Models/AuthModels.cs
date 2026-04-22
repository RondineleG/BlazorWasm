namespace BlazorTerminal.Api.Models;

public record RegisterRequest(string Username, string Password);

public record AuthResponse(string Token, int ExpiresIn, string Username);

public record UserDto(int Id, string Username, DateTime CreatedAt);