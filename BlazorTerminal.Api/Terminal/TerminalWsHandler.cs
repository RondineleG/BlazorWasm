using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using BlazorTerminal.Api.Services;

namespace BlazorTerminal.Api.Terminal;

public class TerminalWsHandler
{
    private readonly ConnectionManager _connectionManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<TerminalWsHandler> _logger;

    public TerminalWsHandler(
        ConnectionManager connectionManager,
        JwtService jwtService,
        ILogger<TerminalWsHandler> logger)
    {
        _connectionManager = connectionManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task HandleAsync(WebSocket webSocket, ClaimsPrincipal? user, CancellationToken ct)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Unauthorized", ct);
            return;
        }

        var username = user.Identity?.Name ?? "unknown";
        _logger.LogInformation("Terminal WebSocket connected for user: {Username}", username);

        int connectionId = -1;

        try
        {
            connectionId = await _connectionManager.CreateConnectionAsync(username, ct);
            var shell = _connectionManager.GetShell(connectionId);

            if (shell is null)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "SSH connection failed", ct);
                return;
            }

            // Start reading from SSH and sending to WebSocket
            var sshTask = Task.Run(async () =>
            {
                var buffer = new byte[4096];
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var data = await Task.Run(() =>
                        {
                            try
                            {
                                if (shell.DataAvailable)
                                    return shell.Read(buffer, 0, buffer.Length);
                                return 0;
                            }
                            catch
                            {
                                return 0;
                            }
                        }, ct);

                        if (data > 0)
                        {
                            await webSocket.SendAsync(
                                new ArraySegment<byte>(buffer, 0, data),
                                WebSocketMessageType.Binary,
                                false,
                                ct);
                        }
                        else
                        {
                            await Task.Delay(50, ct);
                        }
                    }
                }
                catch (OperationCanceledException) { }
            }, ct);

            // Read from WebSocket and send to SSH
            var wsBuffer = new byte[4096];
            while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(wsBuffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text || result.MessageType == WebSocketMessageType.Binary)
                {
                    var text = Encoding.UTF8.GetString(wsBuffer, 0, result.Count);
                    shell.Write(text);
                }
            }

            await sshTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Terminal WebSocket error for user: {Username}", username);
        }
        finally
        {
            if (connectionId > 0)
            {
                _connectionManager.RemoveConnection(connectionId);
            }

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", ct);
            }

            _logger.LogInformation("Terminal WebSocket disconnected for user: {Username}", username);
        }
    }
}