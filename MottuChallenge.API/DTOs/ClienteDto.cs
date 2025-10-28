using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.DTOs
{
    [SwaggerSchema("DTO para criação e atualização de clientes")]
    public class ClienteDto
    {
        [Required(ErrorMessage = "O nome de usuário do cliente é obrigatório.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O usuário deve ter entre 3 e 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "O usuário deve conter apenas letras, números e underscore.")]
        [SwaggerSchema("Nome de usuário único do cliente")]
        public string UsuarioCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome completo do cliente é obrigatório.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "O nome deve ter entre 5 e 100 caracteres.")]
        [SwaggerSchema("Nome completo do cliente")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha do cliente é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [SwaggerSchema("Senha do cliente")]
        public string Senha { get; set; } = string.Empty;

        [SwaggerSchema("Placa da moto associada ao cliente (opcional)")]
        [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "Formato de placa inválido. Use: XXX-0000")]
        public string? MotoPlaca { get; set; }
    }

    [SwaggerSchema("DTO para resposta de cliente")]
    public class ClienteResponseDto
    {
        [SwaggerSchema("Nome de usuário único do cliente")]
        public string UsuarioCliente { get; set; } = string.Empty;

        [SwaggerSchema("Nome completo do cliente")]
        public string Nome { get; set; } = string.Empty;

        [SwaggerSchema("Placa da moto associada ao cliente (opcional)")]
        public string? MotoPlaca { get; set; }

        [SwaggerSchema("Data da última manutenção realizada")]
        public DateTime? DataUltimaManutencao { get; set; }

        [SwaggerSchema("Quantidade de manutenções realizadas")]
        public int QuantidadeManutencoes { get; set; }

        [SwaggerSchema("Indica se possui moto")]
        public bool PossuiMoto { get; set; }
    }
}