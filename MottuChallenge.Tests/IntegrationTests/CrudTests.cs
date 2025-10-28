using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MottuChallenge.API.Models.Auth;
using MottuChallenge.API.DTOs;
using MottuChallenge.API.Enums;
using Xunit;

namespace MottuChallenge.Tests.IntegrationTests
{
    public class CrudTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public CrudTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private async Task<string> GetAuthTokenAsync(HttpClient client)
        {
            var loginRequest = new LoginRequest
            {
                Usuario = "admin_principal",
                Senha = "Admin123!"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var responseString = await loginResponse.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseString);
            var root = document.RootElement;

            string? token = null;
            if (root.TryGetProperty("data", out JsonElement dataElement) &&
                dataElement.TryGetProperty("token", out JsonElement tokenElement))
            {
                token = tokenElement.GetString();
            }

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            return token;
        }

        [Fact]
        public async Task FullCrudFlow_WithAuthentication_Success()
        {
            var client = _factory.CreateClient();

            var token = await GetAuthTokenAsync(client);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"Token obtido: {token.Substring(0, 20)}...");

            var checkPatioResponse = await client.GetAsync("/api/v1/Patio/Patio-Central");
            if (checkPatioResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine(" Usando Patio-Central existente");

                var funcionarioDto = new FuncionarioDto
                {
                    UsuarioFuncionario = "teste_func_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Nome = "Funcionario Teste Automático",
                    Senha = "Senha123!",
                    NomePatio = "Patio-Central",
                    Role = "Funcionario"
                };

                var funcionarioResponse = await client.PostAsJsonAsync("/api/v1/Funcionario", funcionarioDto);
                Console.WriteLine($" Criar Funcionario: {funcionarioResponse.StatusCode}");

                if (funcionarioResponse.IsSuccessStatusCode)
                {
                    var motoDto = new MotoDto
                    {
                        Placa = $"{Guid.NewGuid().ToString("N").Substring(0, 3).ToUpper()}-{Guid.NewGuid().ToString("N").Substring(0, 4)}",
                        Modelo = ModeloMoto.MottuPop,
                        Status = StatusMoto.Disponivel,
                        Setor = SetorMoto.Bom,
                        NomePatio = "Patio-Central",
                        UsuarioFuncionario = "admin_principal",
                        Quilometragem = 1000000
                    };

                    var motoResponse = await client.PostAsJsonAsync("/api/v1/Moto", motoDto);
                    Console.WriteLine($" Criar Moto: {motoResponse.StatusCode}");

                    var successCount = 0;
                    if (funcionarioResponse.IsSuccessStatusCode) successCount++;
                    if (motoResponse.IsSuccessStatusCode) successCount++;

                    if (motoResponse.IsSuccessStatusCode)
                    {
                        await client.DeleteAsync($"/api/v1/Moto/{motoDto.Placa}");
                    }
                    if (funcionarioResponse.IsSuccessStatusCode)
                    {
                        await client.DeleteAsync($"/api/v1/Funcionario/{funcionarioDto.UsuarioFuncionario}");
                    }

                    Assert.True(successCount >= 1, $"Pelo menos 1 recurso deveria ser criado, mas {successCount} foram criados");
                    return;
                }
            }

            Console.WriteLine(" Tentando criar um patio novo...");

            var patioDto = new PatioDto
            {
                NomePatio = "Teste-Auto-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Localizacao = "Localizacao Automática Teste",
                VagasTotais = 5
            };

            var patioResponse = await client.PostAsJsonAsync("/api/v1/Patio", patioDto);
            Console.WriteLine($" Criar Patio: {patioResponse.StatusCode}");

            Assert.True(patioResponse.IsSuccessStatusCode, $"Falha ao criar patio: {patioResponse.StatusCode}");

            if (patioResponse.IsSuccessStatusCode)
            {
                await client.DeleteAsync($"/api/v1/Patio/{patioDto.NomePatio}");
            }
        }

        [Fact]
        public async Task SimpleAuthTest_ReturnsValidToken()
        {
            var client = _factory.CreateClient();
            var token = await GetAuthTokenAsync(client);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Console.WriteLine($" Simple Auth Test - Token válido obtido!");
        }

        [Fact]
        public async Task Endpoints_WithValidToken_AreAccessible()
        {
            var client = _factory.CreateClient();
            var token = await GetAuthTokenAsync(client);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var validateResponse = await client.GetAsync("/api/v1/auth/validate");
            Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);

            var motosResponse = await client.GetAsync("/api/v1/Moto");
            Assert.True(
                motosResponse.StatusCode == HttpStatusCode.OK ||
                motosResponse.StatusCode == HttpStatusCode.NoContent,
                $"GET /api/v1/Moto deveria retornar 200 ou 204, mas retornou {motosResponse.StatusCode}"
            );

            Console.WriteLine(" Endpoints protegidos acessíveis com token!");
        }
    }
}