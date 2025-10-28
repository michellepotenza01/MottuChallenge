using MottuChallenge.API.Models.Auth;
using MottuChallenge.API.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using MottuChallenge.API.Models;
using MottuChallenge.API.Models.Common;

namespace MottuChallenge.API.Services
{
    public class AuthService
    {
        private readonly FuncionarioRepository _funcionarioRepository;
        private readonly IConfiguration _configuration;

        public AuthService(FuncionarioRepository funcionarioRepository, IConfiguration configuration)
        {
            _funcionarioRepository = funcionarioRepository;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Senha))
                    return ServiceResponse<LoginResponse>.Error("Usuário e senha são obrigatórios");

                var funcionario = await _funcionarioRepository.GetByUsernameAsync(request.Usuario);
                if (funcionario is null)
                    return ServiceResponse<LoginResponse>.Error("Credenciais inválidas");

                if (!VerifyPassword(request.Senha, funcionario.SenhaHash))
                    return ServiceResponse<LoginResponse>.Error("Credenciais inválidas");

                var token = GenerateJwtToken(funcionario);
                return ServiceResponse<LoginResponse>.Ok(token, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<LoginResponse>.Error($"Erro durante login: {ex.Message}");
            }
        }

        private LoginResponse GenerateJwtToken(Funcionario funcionario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // ✅ CORRETO para Oracle - DateTime.Now
            var expires = DateTime.Now.AddHours(3);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, funcionario.UsuarioFuncionario),
                new Claim(ClaimTypes.Role, funcionario.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("iss", _configuration["Jwt:Issuer"] ?? "MottuAPI"),
                new Claim("aud", _configuration["Jwt:Audience"] ?? "MottuClient")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresAt = expires,
                Usuario = funcionario.UsuarioFuncionario,
                Role = funcionario.Role,
                ExpiresInHours = 3,
                Message = "Login realizado com sucesso"
            };
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Senha não pode ser vazia", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}