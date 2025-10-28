using Microsoft.ML.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Models.ML
{
    [SwaggerSchema("Dados de entrada para predição de manutenção")]
    public class MotoManutencaoInput
    {
        [LoadColumn(0)]
        [SwaggerSchema("Quilometragem atual")]
        public float Quilometragem { get; set; }

        [LoadColumn(1)]
        [SwaggerSchema("Dias desde a última revisão")]
        public float DiasDesdeUltimaRevisao { get; set; }

        [LoadColumn(2)]
        [SwaggerSchema("Quantidade de revisões realizadas")]
        public float QuantidadeRevisoes { get; set; }

        [LoadColumn(3)]
        [SwaggerSchema("Setor de conservação (codificado: 0=Bom, 1=Intermediario, 2=Ruim)", Format = "float")]
        public float SetorEncoded { get; set; }

        [LoadColumn(4)]
        [SwaggerSchema("Indica se precisa de manutenção")]
        public bool PrecisaManutencao { get; set; }

        [SwaggerSchema("Placa da moto")]
        public string Placa { get; set; } = string.Empty;
    }
}