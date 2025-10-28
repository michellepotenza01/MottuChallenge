using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [SwaggerSchema("Setores das motos")]
    public enum SetorMoto
    {
        [Display(Name = "Bom", Description = "Estado excelente")]
        Bom,

        [Display(Name = "Intermediario", Description = "Estado regular")]
        Intermediario,

        [Display(Name = "Ruim", Description = "Estado precisa de reparos")]
        Ruim
    }
}