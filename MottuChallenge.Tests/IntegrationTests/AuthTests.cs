using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MottuChallenge.API.Models.Auth;
using Xunit;



namespace MottuChallenge.Tests.IntegrationTests
{
    public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WithAdminCredentials_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var loginRequest = new LoginRequest
            {
                Usuario = "admin_principal",
                Senha = "Admin123!"
            };

            var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", responseString);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var loginRequest = new LoginRequest
            {
                Usuario = "invalid_user",
                Senha = "wrong_password"
            };

            var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMotos_WithoutAuth_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/Moto");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent,
                $"Expected 200 or 204 but got {response.StatusCode}"
            );
        }

        [Fact]
        public async Task GetPatios_WithoutAuth_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/Patio");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent,
                $"Expected 200 or 204 but got {response.StatusCode}"
            );
        }
    }
}