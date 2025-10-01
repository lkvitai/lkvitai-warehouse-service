using Microsoft.Extensions.Configuration;
using Polly;
using Renci.SshNet;

namespace Lkvitai.Warehouse.Infrastructure.Export;

public class SftpUploader
{
    private readonly IConfiguration _cfg;
    public SftpUploader(IConfiguration cfg) => _cfg = cfg;

    public virtual async Task UploadAsync(string localPath, string remoteFileName, CancellationToken ct = default)
    {
        var s = _cfg.GetSection("Sftp");
        var host = s["Host"]!; var port = int.TryParse(s["Port"], out var p) ? p : 22;
        var user = s["Username"]!;
        var pass = s["Password"];
        var keyPath = s["PrivateKeyPath"];
        var keyPass = s["PrivateKeyPassphrase"];
        var remoteDir = s["RemoteDir"]!;
        var retries = int.TryParse(s["Retries"], out var r) ? r : 3;
        var delay = TimeSpan.FromSeconds(int.TryParse(s["RetryDelaySeconds"], out var d) ? d : 5);

        var policy = Policy.Handle<Exception>().WaitAndRetryAsync(retries, i => delay);

        await policy.ExecuteAsync(async () =>
        {
            using var client = CreateClient(host, port, user, pass, keyPath, keyPass);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(int.TryParse(s["ConnectTimeoutSeconds"], out var t) ? t : 10);
            client.Connect();
            if (!client.IsConnected) throw new InvalidOperationException("SFTP not connected");

            if (!client.Exists(remoteDir)) client.CreateDirectory(remoteDir);
            var remotePath = $"{remoteDir.TrimEnd('/')}/{remoteFileName}";

            using var fs = File.OpenRead(localPath);
            await Task.Run(() => client.UploadFile(fs, remotePath, true), ct);

            client.Disconnect();
        });
    }

    private SftpClient CreateClient(string host, int port, string user, string? pass, string? keyPath, string? keyPass)
    {
        if (!string.IsNullOrEmpty(keyPath))
        {
            var keyFile = string.IsNullOrEmpty(keyPass)
                ? new PrivateKeyFile(keyPath)
                : new PrivateKeyFile(keyPath, keyPass);
            var auth = new PrivateKeyAuthenticationMethod(user, keyFile);
            return new SftpClient(new ConnectionInfo(host, port, user, auth));
        }
        else
        {
            return new SftpClient(host, port, user, pass ?? string.Empty);
        }
    }
}


