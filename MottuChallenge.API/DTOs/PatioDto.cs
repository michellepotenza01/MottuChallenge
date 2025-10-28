using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.DTOs
{
    [SwaggerSchema("DTO para criação e atualização de pátios")]
    public class PatioDto
    {
        [Required(ErrorMessage = "O nome do patio é obrigatório.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome do patio deve ter entre 3 e 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "O nome do patio deve conter apenas letras, números e espaços.")]
        [SwaggerSchema("Nome único do patio")]
        public string NomePatio { get; set; } = string.Empty;

        [Required(ErrorMessage = "A localização do patio é obrigatória.")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "A localização deve ter entre 10 e 200 caracteres.")]
        [SwaggerSchema("Localização completa do patio")]
        public string Localizacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número total de vagas é obrigatório.")]
        [Range(1, 1000, ErrorMessage = "O patio deve ter entre 1 e 1000 vagas.")]
        [SwaggerSchema("Número total de vagas disponíveis")]
        public int VagasTotais { get; set; }
    }

    [SwaggerSchema("DTO para resposta de pátio")]
    public class PatioResponseDto
    {
        [SwaggerSchema("Nome único do patio")]
        public string NomePatio { get; set; } = string.Empty;

        [SwaggerSchema("Localização completa do patio")]
        public string Localizacao { get; set; } = string.Empty;

        [SwaggerSchema("Número total de vagas disponíveis")]
        public int VagasTotais { get; set; }

        [SwaggerSchema("Número de vagas ocupadas")]
        public int VagasOcupadas { get; set; }

        [SwaggerSchema("Número de vagas disponíveis (calculado)")]
        public int VagasDisponiveis { get; set; }

        [SwaggerSchema("Taxa de ocupação em porcentagem")]
        public double TaxaOcupacao { get; set; }
    }
}