using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
using MottuChallenge.API.Models;

namespace MottuChallenge.API.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
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
        [StringLength(256, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [SwaggerSchema("Senha do cliente (hash)")]
        [JsonIgnore]
        [Column(TypeName = "VARCHAR2(256)")]
        public string SenhaHash { get; set; } = string.Empty;

        [SwaggerSchema("Placa da moto associada ao cliente (opcional)")]
        [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "Formato de placa inválido. Use: XXX-0000")]
        public string? MotoPlaca { get; set; }

        [ForeignKey("MotoPlaca")]
        [SwaggerSchema("Moto associada ao cliente")]
        public virtual Moto? Moto { get; set; }

        [SwaggerSchema("Data da última manutenção realizada")]
        public DateTime? DataUltimaManutencao { get; set; }

        [SwaggerSchema("Quantidade de manutenções realizadas")]
        [Range(0, 1000, ErrorMessage = "A quantidade de manutenções deve ser entre 0 e 1000")]
        public int QuantidadeManutencoes { get; set; } = 0;

        [SwaggerSchema("Data de criação do registro")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [SwaggerSchema("Data da última atualização")]
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;

        public void RegistrarManutencao()
        {
            DataUltimaManutencao = DateTime.Now;
            QuantidadeManutencoes++;
            DataAtualizacao = DateTime.Now;
        }

         public bool PossuiMoto() 
        { 
            return !string.IsNullOrEmpty(MotoPlaca); 
        }
    }
}