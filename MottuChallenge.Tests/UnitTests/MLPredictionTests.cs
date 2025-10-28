using MottuChallenge.API.Models;
using MottuChallenge.API.Services;
using MottuChallenge.API.Enums;
using Microsoft.Extensions.Logging;
using Xunit;


namespace MottuChallenge.Tests.UnitTests
{
    public class MLPredictionTests
    {
        [Fact]
        public void MotoPredictionService_CanBeInitialized()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<MotoPredictionService>();

            var service = new MotoPredictionService(logger);

            Assert.NotNull(service);
        }

        [Fact]
        public void PredictManutencao_WithHighQuilometragem_ReturnsPrecisaManutencao()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<MotoPredictionService>();
            var service = new MotoPredictionService(logger);

            var moto = new Moto
            {
                Placa = "TEST-1234",
                Quilometragem = 20000, 
                QuantidadeRevisoes = 1,
                DataUltimaRevisao = DateTime.Now.AddMonths(-12), 
                Setor = SetorMoto.Ruim 
            };

            var prediction = service.PredictManutencao(moto);

            Assert.NotNull(prediction);
            Assert.True(prediction.PrecisaManutencao);
            Assert.True(prediction.Probability > 0.5f);
        }

        [Fact]
        public void PredictManutencao_WithNewMoto_ReturnsNoManutencao()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<MotoPredictionService>();
            var service = new MotoPredictionService(logger);

            var moto = new Moto
            {
                Placa = "NEW-5678",
                Quilometragem = 1000, 
                QuantidadeRevisoes = 1,
                DataUltimaRevisao = DateTime.Now.AddMonths(-1), 
                Setor = SetorMoto.Bom 
            };

            var prediction = service.PredictManutencao(moto);

            Assert.NotNull(prediction);
            Assert.InRange(prediction.Probability, 0.0f, 1.0f);
        }
    }
}