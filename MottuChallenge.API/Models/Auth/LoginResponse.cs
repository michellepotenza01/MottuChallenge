using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Models.Auth
{
    [SwaggerSchema("Resposta de login bem-sucedido")]
    public class LoginResponse
    {
        [SwaggerSchema("Token JWT para autenticação")]
        public string Token { get; set; } = string.Empty;

        [SwaggerSchema("Tipo do token")]
        public string TokenType { get; set; } = "Bearer";

        [SwaggerSchema("Data de expiração do token")]
        public DateTime ExpiresAt { get; set; }

        [SwaggerSchema("Usuário autenticado")]
        public string Usuario { get; set; } = string.Empty;

        [SwaggerSchema("Role do usuário")]
        public string Role { get; set; } = string.Empty;

        [SwaggerSchema("Tempo de expiração em horas")]
        public int ExpiresInHours { get; set; } = 3;

        [SwaggerSchema("Mensagem de sucesso")]
        public string Message { get; set; } = "Login realizado com sucesso";
    }
}