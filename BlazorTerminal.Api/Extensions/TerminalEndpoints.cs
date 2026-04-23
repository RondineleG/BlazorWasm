using System.Net.WebSockets;
using System.Security.Claims;
using BlazorTerminal.Api.Services;
using BlazorTerminal.Api.Terminal;
using Microsoft.Extensions.DependencyInjection;

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
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var handler = context.RequestServices.GetRequiredService<TerminalWsHandler>();
        await handler.HandleAsync(webSocket, user, ct);
    }
}