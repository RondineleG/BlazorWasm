using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using BlazorTerminal.Api.Services;
using BlazorTerminal.Api.Terminal;
using Microsoft.AspNetCore.Mvc;

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
        [FromServices] TerminalWsHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(webSocket, user, ct);
    }
}