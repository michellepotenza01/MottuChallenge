using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Models.Auth
{
    [SwaggerSchema("Requisição de login")]
    public class LoginRequest
    {
        [Required(ErrorMessage = "Usuário obrigatório.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Usuário deve ter entre 3 e 50 caracteres.")]
        [SwaggerSchema("Nome de usuário do funcionário")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres.")]
        [SwaggerSchema("Senha do funcionário")]
        public string Senha { get; set; } = string.Empty;
    }
}