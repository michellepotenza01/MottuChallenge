using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MottuChallenge.Tests.IntegrationTests
{
    public class BasicApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BasicApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HomePage_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HealthCheck_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ApiInfo_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}