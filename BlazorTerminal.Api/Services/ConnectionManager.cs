using System.Collections.Concurrent;
using System.Security.Cryptography;
using Renci.SshNet;

namespace BlazorTerminal.Api.Services;

public class SshConnectionInfo
{
    public required string Host { get; set; }
    public required string Username { get; set; }
    public required string KeyPath { get; set; }
    public int Port { get; set; } = 22;
}

public class ConnectionManager
{
    private readonly ConcurrentDictionary<int, SshClient> _connections = [];
    private readonly ConcurrentDictionary<int, ShellStream> _shells = [];
    private readonly SshConnectionInfo _sshConfig;

    public ConnectionManager(SshConnectionInfo sshConfig)
    {
        _sshConfig = sshConfig;
    }

    public async Task<int> CreateConnectionAsync(string username, CancellationToken ct = default)
    {
        var connectionId = RandomNumberGenerator.GetInt32(int.MaxValue);

        var keyFile = new PrivateKeyFile(_sshConfig.KeyPath);
        var keyFiles = new[] { keyFile };

        var authMethod = new PrivateKeyAuthenticationMethod(_sshConfig.Username, keyFiles);

        var connectionInfo = new Renci.SshNet.ConnectionInfo(
            _sshConfig.Host,
            _sshConfig.Port,
            _sshConfig.Username,
            authMethod
        );

        var client = new SshClient(connectionInfo);

        await Task.Run(() =>
        {
            client.Connect();
            if (!client.IsConnected)
                throw new InvalidOperationException("SSH connection failed");
        }, ct);

        var shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

        _connections.TryAdd(connectionId, client);
        _shells.TryAdd(connectionId, shellStream);

        return connectionId;
    }

    public ShellStream? GetShell(int connectionId) => _shells.GetValueOrDefault(connectionId);

    public void RemoveConnection(int connectionId)
    {
        if (_shells.TryRemove(connectionId, out var shell))
        {
            shell.Dispose();
        }

        if (_connections.TryRemove(connectionId, out var client))
        {
            client.Disconnect();
            client.Dispose();
        }
    }

    public void RemoveAll()
    {
        foreach (var id in _connections.Keys)
        {
            RemoveConnection(id);
        }
    }
}