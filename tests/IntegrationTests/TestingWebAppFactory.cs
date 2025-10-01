using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lkvitai.Warehouse.Infrastructure.Export;

namespace Lkvitai.Warehouse.IntegrationTests;

public class TestingWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<SftpUploader, FakeSftpUploader>();
        });
    }

    private sealed class FakeSftpUploader : SftpUploader
    {
        public FakeSftpUploader()
            : base(new ConfigurationBuilder().AddInMemoryCollection().Build())
        {
        }

        public override Task UploadAsync(string localPath, string remoteFileName, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
