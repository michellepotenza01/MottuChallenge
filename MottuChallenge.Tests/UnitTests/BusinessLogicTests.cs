using MottuChallenge.API.Models;
using MottuChallenge.API.Enums;
using Xunit;

namespace MottuChallenge.Tests.UnitTests
{
    public class BusinessLogicTests
    {
        [Fact]
        public void Patio_TemVagaDisponivel_WithAvailableVagas_ReturnsTrue()
        {
            var patio = new Patio
            {
                VagasTotais = 10,
                VagasOcupadas = 5
            };

            var temVaga = patio.TemVagaDisponivel();

            Assert.True(temVaga);
        }

        [Fact]
        public void Patio_TemVagaDisponivel_WithNoVagas_ReturnsFalse()
        {
            var patio = new Patio
            {
                VagasTotais = 10,
                VagasOcupadas = 10
            };

            var temVaga = patio.TemVagaDisponivel();

            Assert.False(temVaga);
        }

        [Fact]
        public void Moto_OcupaVaga_WithDisponivelStatus_ReturnsTrue()
        {
            var moto = new Moto
            {
                Status = StatusMoto.Disponivel
            };

            var ocupaVaga = moto.OcupaVaga;

            Assert.True(ocupaVaga);
        }

        [Fact]
        public void Moto_OcupaVaga_WithAlugadaStatus_ReturnsFalse()
        {
            var moto = new Moto
            {
                Status = StatusMoto.Alugada
            };

            var ocupaVaga = moto.OcupaVaga;

            Assert.False(ocupaVaga);
        }

        [Fact]
        public void Funcionario_PertenceAoPatio_WithMatchingPatio_ReturnsTrue()
        {
            var funcionario = new Funcionario
            {
                NomePatio = "Patio-Teste"
            };

            var pertence = funcionario.PertenceAoPatio("Patio-Teste");

            Assert.True(pertence);
        }

        [Fact]
        public void Funcionario_IsAdmin_WithAdminRole_ReturnsTrue()
        {
            var funcionario = new Funcionario
            {
                Role = "Admin"
            };

            var isAdmin = funcionario.IsAdmin();

            Assert.True(isAdmin);
        }

        [Fact]
        public void Cliente_PossuiMoto_WithMotoPlaca_ReturnsTrue()
        {
            var cliente = new Cliente
            {
                MotoPlaca = "ABC-1234"
            };

            var possuiMoto = cliente.PossuiMoto();

            Assert.True(possuiMoto);
        }
    }
}