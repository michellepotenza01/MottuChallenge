using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using MottuChallenge.API;
using Xunit;



namespace MottuChallenge.Tests.IntegrationTests
{
    public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HealthCheckTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthCheck_ReturnsHealthyStatus()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", content);
        }

        [Fact]
        public async Task HealthCheckPing_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/health/ping");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HealthCheckVersion_ReturnsVersionInfo()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v1/health/version");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}