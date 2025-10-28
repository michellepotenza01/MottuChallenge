using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using MottuChallenge.API.Enums;

namespace MottuChallenge.API.DTOs
{
    [SwaggerSchema("DTO para criação e atualização de motos")]
    public class MotoDto
    {
        [Required(ErrorMessage = "A placa da moto é obrigatória.")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "A placa deve ter 7 caracteres no formato XXX-0000.")]
        [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "Formato de placa inválido. Use: XXX-0000")]
        [SwaggerSchema("Placa da moto no formato XXX-0000")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo da moto é obrigatório.")]
        [SwaggerSchema("Modelo da moto")]
        public ModeloMoto Modelo { get; set; }

        [Required(ErrorMessage = "O status da moto é obrigatório.")]
        [SwaggerSchema("Status atual da moto")]
        public StatusMoto Status { get; set; } = StatusMoto.Disponivel;

        [Required(ErrorMessage = "O setor de conservação é obrigatório.")]
        [SwaggerSchema("Setor de conservação da moto")]
        public SetorMoto Setor { get; set; } = SetorMoto.Bom;

        [Required(ErrorMessage = "O patio onde a moto está alocada é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome do patio deve ter no máximo 50 caracteres.")]
        [SwaggerSchema("Patio onde a moto está alocada")]
        public string NomePatio { get; set; } = string.Empty;

        [Required(ErrorMessage = "O funcionário responsável é obrigatório.")]
        [StringLength(50, ErrorMessage = "O usuário do funcionário deve ter no máximo 50 caracteres.")]
        [SwaggerSchema("Usuário do funcionário responsável pela moto")]
        public string UsuarioFuncionario { get; set; } = string.Empty;

        [SwaggerSchema("Quilometragem atual da moto")]
        [Range(0, 1000000, ErrorMessage = "A quilometragem deve ser entre 0 e 1.000.000 km")]
        public int Quilometragem { get; set; } = 0;

        [SwaggerSchema("Data da última revisão")]
        public DateTime? DataUltimaRevisao { get; set; }
    }

    [SwaggerSchema("DTO para resposta de moto")]
    public class MotoResponseDto
    {
        [SwaggerSchema("Placa da moto no formato XXX-0000")]
        public string Placa { get; set; } = string.Empty;

        [SwaggerSchema("Modelo da moto")]
        public ModeloMoto Modelo { get; set; }

        [SwaggerSchema("Status atual da moto")]
        public StatusMoto Status { get; set; }

        [SwaggerSchema("Setor de conservação da moto")]
        public SetorMoto Setor { get; set; }

        [SwaggerSchema("Patio onde a moto está alocada")]
        public string NomePatio { get; set; } = string.Empty;

        [SwaggerSchema("Usuário do funcionário responsável pela moto")]
        public string UsuarioFuncionario { get; set; } = string.Empty;

        [SwaggerSchema("Quilometragem atual da moto")]
        public int Quilometragem { get; set; }

        [SwaggerSchema("Indica se precisa de manutenção")]
        public bool PrecisaManutencao { get; set; }

        [SwaggerSchema("Probabilidade de precisar de manutenção")]
        public float ProbabilidadeManutencao { get; set; }

        [SwaggerSchema("Data da última revisão")]
        public DateTime? DataUltimaRevisao { get; set; }

        [SwaggerSchema("Quantidade de revisões realizadas")]
        public int QuantidadeRevisoes { get; set; }

        [SwaggerSchema("Indica se está disponível para aluguel")]
        public bool DisponivelParaAluguel { get; set; }
    }
}