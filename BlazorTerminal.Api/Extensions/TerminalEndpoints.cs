using System.Net.WebSockets;
using System.Security.Claims;
using BlazorTerminal.Api.Services;
using BlazorTerminal.Api.Terminal;

namespace BlazorTerminal.Api.Extensions;

public class TerminalEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/ws/terminal")
            .RequireAuthorization();

        group.MapGet("/", HandleTerminal);
    }

    private async Task HandleTerminal(
        HttpContext context,
        WebSocket webSocket,
        ClaimsPrincipal user,
        TerminalWsHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(webSocket, user, ct);
    }
}