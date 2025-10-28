using Microsoft.ML.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuChallenge.API.Models.ML
{
    [SwaggerSchema("Resultado da predição de manutenção")]
    public class MotoManutencaoPrediction
    {
        [ColumnName("PredictedLabel")]
        [SwaggerSchema("Indica se precisa de manutenção")]
        public bool PrecisaManutencao { get; set; }

        [ColumnName("Probability")]
        [SwaggerSchema("Probabilidade de precisar de manutenção")]
        public float Probability { get; set; }

        [ColumnName("Score")]
        [SwaggerSchema("Score da predição")]
        public float Score { get; set; }

        [SwaggerSchema("Recomendação baseada na predição")]
        public string Recomendacao => PrecisaManutencao 
            ? "Agendar manutenção preventiva" 
            : "Moto em boas condições";

        [SwaggerSchema("Nível de urgência")]
        public string NivelUrgencia
        {
            get
            {
                return Probability switch
                {
                    > 0.8f => "ALTA URGENCIA",
                    > 0.6f => "MEDIA URGENCIA", 
                    > 0.4f => "BAIXA URGENCIA",
                    _ => "OK, SEM URGENCIA"
                };
            }
        }
    }
}