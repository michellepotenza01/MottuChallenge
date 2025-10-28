using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MottuChallenge.Tests.IntegrationTests
{
    public class ProtectedEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProtectedEndpointsTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateMoto_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            
            var response = await client.PostAsJsonAsync("/api/v1/Moto", new {});
            
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreatePatio_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            
            var response = await client.PostAsJsonAsync("/api/v1/Patio", new {});
            
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateFuncionario_WithoutAuth_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            
            var response = await client.PostAsJsonAsync("/api/v1/Funcionario", new {});
            
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ValidateToken_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v1/auth/validate");
            
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}