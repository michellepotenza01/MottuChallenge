using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace MottuChallenge.API.Models
{
    [Table("Patios")]
    public class Patio
    {
        [Key]
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
        [SwaggerSchema("Número total de vagas disponíveis no patio")]
        public int VagasTotais { get; set; }

        [Required(ErrorMessage = "O número de vagas ocupadas é obrigatório.")]
        [Range(0, 1000, ErrorMessage = "As vagas ocupadas devem estar entre 0 e 1000.")]
        [SwaggerSchema("Número de vagas atualmente ocupadas")]
        public int VagasOcupadas { get; set; } = 0;

         [NotMapped]  
        [SwaggerSchema("Número de vagas disponíveis (calculado automaticamente)")]
        public int VagasDisponiveis 
        { 
            get 
            { 
                return VagasTotais - VagasOcupadas; 
            } 
        }

        [NotMapped]
        [SwaggerSchema("Taxa de ocupação do patio em porcentagem")]
        public double TaxaOcupacao 
        { 
            get 
            { 
                return VagasTotais > 0 ? (double)VagasOcupadas / VagasTotais * 100 : 0; 
            } 
        }

        [JsonIgnore]
        public virtual ICollection<Moto> Motos { get; set; } = new List<Moto>();

        [JsonIgnore]
        public virtual ICollection<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();

         public bool TemVagaDisponivel() 
        { 
            return VagasDisponiveis > 0; 
        }

        public bool OcuparVaga()
        {
            if (!TemVagaDisponivel()) return false;
            VagasOcupadas++;
            return true;
        }

        public void LiberarVaga()
        {
            if (VagasOcupadas > 0)
                VagasOcupadas--;
        }
    }
}