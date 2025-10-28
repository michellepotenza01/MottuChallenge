using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [SwaggerSchema("Status das motos no sistema")]
    public enum StatusMoto
    {
        [Display(Name = "Disponivel", Description = "Moto disponivel para aluguel")]
        Disponivel,

        [Display(Name = "Alugada", Description = "Moto atualmente alugada por um cliente")]
        Alugada,

        [Display(Name = "Manutencao", Description = "Moto em manutencao tecnica")]
        Manutencao
    }
}