using MottuChallenge.API.Services;
using MottuChallenge.API.Models.Common;
using Xunit;


namespace MottuChallenge.Tests.UnitTests
{
    public class ServiceResponseTests
    {
        [Fact]
        public void ServiceResponse_Ok_ReturnsSuccessResponse()
        {
            var data = "Test Data";
            var response = ServiceResponse<string>.Ok(data, "Operacao bem-sucedida");

            Assert.True(response.Success);
            Assert.Equal(data, response.Data);
            Assert.Equal("Operacao bem-sucedida", response.Message);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public void ServiceResponse_Error_ReturnsErrorResponse()
        {
            var errorMessage = "Erro de teste";
            var response = ServiceResponse<string>.Error(errorMessage);

            Assert.False(response.Success);
            Assert.Null(response.Data);
            Assert.Equal(errorMessage, response.Message);
            Assert.Single(response.Errors);
        }

        [Fact]
        public void ServiceResponse_NotFound_ReturnsNotFoundResponse()
        {
            var resourceName = "Moto";
            var response = ServiceResponse<string>.NotFound(resourceName);

            Assert.False(response.Success);
            Assert.Contains(resourceName, response.Message);
        }
    }
}