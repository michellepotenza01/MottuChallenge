using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
using MottuChallenge.API.Enums;

namespace MottuChallenge.API.Models
{
    [Table("Motos")]
    public class Moto
    {
        [Key]
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

        [ForeignKey("NomePatio")]
        [JsonIgnore]
        [SwaggerSchema("Patio onde a moto está alocada")]
        public virtual Patio Patio { get; set; } = null!; 

        [ForeignKey("UsuarioFuncionario")]
        [JsonIgnore]
        [SwaggerSchema("Funcionário responsável pela moto")]
        public virtual Funcionario Funcionario { get; set; } = null!; 

        [SwaggerSchema("Quilometragem atual da moto")]
        [Range(0, 1000000, ErrorMessage = "A quilometragem deve ser entre 0 e 1.000.000 km")]
        public int Quilometragem { get; set; } = 0;

        [SwaggerSchema("Data da última revisão")]
        public DateTime? DataUltimaRevisao { get; set; }

        [SwaggerSchema("Quantidade de revisões realizadas")]
        [Range(0, 1000, ErrorMessage = "A quantidade de revisões deve ser entre 0 e 1000")]
        public int QuantidadeRevisoes { get; set; } = 0;

        [SwaggerSchema("Indica se precisa de manutenção (predição ML) - 0=False, 1=True")]
        [Range(0, 1, ErrorMessage = "PrecisaManutencao deve ser 0 (false) ou 1 (true)")]
        public int PrecisaManutencao { get; set; } = 0;  

        [SwaggerSchema("Probabilidade de precisar de manutenção")]
        [Range(0.0, 1.0, ErrorMessage = "A probabilidade deve ser entre 0.0 e 1.0")]
        public float ProbabilidadeManutencao { get; set; } = 0.0f;

        [SwaggerSchema("Data de criação do registro")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [SwaggerSchema("Data da última atualização")]
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;

        [NotMapped]
        [SwaggerSchema("Dias desde a última revisão")]
        public int DiasDesdeUltimaRevisao => DataUltimaRevisao.HasValue 
            ? (DateTime.Now - DataUltimaRevisao.Value).Days 
            : int.MaxValue;

        [NotMapped]
        [SwaggerSchema("Indica se a moto está ocupando vaga no patio")]
        public bool OcupaVaga => Status == StatusMoto.Disponivel || Status == StatusMoto.Manutencao;

        [NotMapped]
        [SwaggerSchema("Indica se a moto está disponível para aluguel")]
        public bool DisponivelParaAluguel => Status == StatusMoto.Disponivel && PrecisaManutencao == 0;

         [NotMapped]
        public bool PrecisaManutencaoBool 
        {
            get => PrecisaManutencao == 1;
            set => PrecisaManutencao = value ? 1 : 0;
        }
    }
}