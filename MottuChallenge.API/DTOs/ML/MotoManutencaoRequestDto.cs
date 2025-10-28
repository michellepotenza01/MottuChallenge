using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.DTOs.ML
{
    [SwaggerSchema("DTO para requisição de predição de manutenção")]
    public class MotoManutencaoRequestDto
    {
        [Required(ErrorMessage = "A placa da moto é obrigatória.")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "A placa deve ter 7 caracteres no formato XXX-0000.")]
        [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "Formato de placa inválido. Use: XXX-0000")]
        [SwaggerSchema("Placa da moto para predição")]
        public string Placa { get; set; } = string.Empty;
    }

    [SwaggerSchema("DTO para resposta de predição de manutenção")]
    public class MotoManutencaoResponseDto
    {
        [SwaggerSchema("Placa da moto analisada")]
        public string Placa { get; set; } = string.Empty;

        [SwaggerSchema("Indica se precisa de manutenção")]
        public bool PrecisaManutencao { get; set; }

        [SwaggerSchema("Probabilidade de precisar de manutenção")]
        public float Probabilidade { get; set; }

        [SwaggerSchema("Score da predição ML")]
        public float Score { get; set; }

        [SwaggerSchema("Recomendação baseada na predição")]
        public string Recomendacao { get; set; } = string.Empty;

        [SwaggerSchema("Nível de urgência")]
        public string NivelUrgencia { get; set; } = string.Empty;

        [SwaggerSchema("Fatores que influenciaram a decisão")]
        public List<string> Fatores { get; set; } = new List<string>();

        [SwaggerSchema("Timestamp da predição")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}