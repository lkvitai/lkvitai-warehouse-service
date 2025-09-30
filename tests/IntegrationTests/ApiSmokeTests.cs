using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lkvitai.Warehouse.IntegrationTests;

public class ApiSmokeTests : IClassFixture<TestingWebAppFactory>
{
    private readonly TestingWebAppFactory _factory;
    public ApiSmokeTests(TestingWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Swagger_and_health_are_up()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var health = await client.GetAsync("/health");
        health.StatusCode.Should().Be(HttpStatusCode.OK);

        var swagger = await client.GetAsync("/swagger/v1/swagger.json");
        swagger.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
