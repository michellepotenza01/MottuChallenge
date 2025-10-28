using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MottuChallenge.API.DTOs;
using MottuChallenge.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using MottuChallenge.API.Models;
using MottuChallenge.API.Models.Common;

namespace MottuChallenge.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Tags("Pátios")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class PatioController : BaseController
    {
        private readonly PatioService _patioService;

        public PatioController(PatioService patioService)
        {
            _patioService = patioService;
        }

        /// <summary>
        /// Listar todos os pátios
        /// </summary>
        /// <remarks>
        /// Retorna todos os pátios cadastrados no sistema.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <returns>Lista de pátios</returns>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar pátios",
            Description = "Retorna todos os pátios cadastrados",
            OperationId = "GetPatios"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Patio>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Patio>>> GetPatios()
        {
            var response = await _patioService.GetPatiosAsync();
            return HandleServiceResponse(response);
        }

        /// <summary>
        /// Obter pátio específico
        /// </summary>
        /// <remarks>
        /// Retorna os detalhes de um pátio específico pelo nome.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <returns>Detalhes do pátio</returns>
        [HttpGet("{nomePatio}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obter pátio",
            Description = "Retorna detalhes de um pátio específico",
            OperationId = "GetPatio"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Patio))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Patio>> GetPatio(
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio)
        {
            var response = await _patioService.GetPatioAsync(nomePatio);
            return HandleServiceResponse(response);
        }

        /// <summary>
        ///  Criar novo pátio
        /// </summary>
        /// <remarks>
        /// Cria um novo pátio no sistema.
        /// 
        /// **Roles:** Admin
        /// </remarks>
        /// <param name="patioDto">Dados do novo pátio</param>
        /// <returns>Pátio criado</returns>
        [HttpPost]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Criar pátio",
            Description = "Cadastra um novo pátio no sistema",
            OperationId = "CreatePatio"
        )]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Patio))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Patio>> CreatePatio(
            [FromBody, SwaggerRequestBody("Dados do pátio", Required = true)] PatioDto patioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados do pátio inválidos"));

            var response = await _patioService.CreatePatioAsync(patioDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return HandleCreatedResponse(
                nameof(GetPatio),
                new { nomePatio = response.Data!.NomePatio, version = RequestedApiVersion },
                response.Data,
                "Pátio criado com sucesso"
            );
        }

        /// <summary>
        /// Atualizar pátio
        /// </summary>
        /// <remarks>
        /// Atualiza os dados de um pátio existente.
        /// 
        /// **Roles:** Admin
        /// </remarks>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <param name="patioDto">Novos dados do pátio</param>
        /// <returns>Pátio atualizado</returns>
        [HttpPut("{nomePatio}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Atualizar pátio",
            Description = "Atualiza dados de um pátio existente",
            OperationId = "UpdatePatio"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Patio))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Patio>> UpdatePatio(
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio,
            [FromBody, SwaggerRequestBody("Dados atualizados do pátio", Required = true)] PatioDto patioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse("Dados de atualização inválidos"));

            var response = await _patioService.UpdatePatioAsync(nomePatio, patioDto);

            if (!response.Success)
                return BadRequest(CreateErrorResponse(response.Message));

            return Ok(new
            {
                Data = response.Data,
                Message = "Pátio atualizado com sucesso",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        /// <summary>
        /// Excluir pátio
        /// </summary>
        /// <remarks>
        /// Remove um pátio do sistema.
        /// 
        /// **Roles:** Admin
        /// </remarks>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <returns>Confirmação de exclusão</returns>
        [HttpDelete("{nomePatio}")]
        [Authorize(Roles = "Funcionario,Admin")]
        [SwaggerOperation(
            Summary = "Excluir pátio",
            Description = "Remove um pátio do sistema",
            OperationId = "DeletePatio"
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> DeletePatio(
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio)
        {
            var response = await _patioService.DeletePatioAsync(nomePatio);

            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            return NoContent();
        }

        /// <summary>
        /// Estatísticas do pátio (V2)
        /// </summary>
        /// <remarks>
        /// **VERSÃO 2** - Retorna estatísticas detalhadas de um pátio específico.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <returns>Estatísticas do pátio</returns>
        [HttpGet("{nomePatio}/estatisticas")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Estatísticas do pátio (V2)",
            Description = "Retorna estatísticas detalhadas do pátio - Versão 2",
            OperationId = "GetEstatisticasPatioV2"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> GetEstatisticasPatioV2(
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio)
        {
            var patioResponse = await _patioService.GetPatioAsync(nomePatio);
            
            if (!patioResponse.Success || patioResponse.Data == null)
                return NotFound(CreateErrorResponse("Pátio não encontrado"));

            var patio = patioResponse.Data;
            var estatisticas = new
            {
                Patio = patio.NomePatio,
                patio.Localizacao,
                patio.VagasTotais,
                patio.VagasOcupadas,
                patio.VagasDisponiveis,
                TaxaOcupacao = Math.Round(patio.TaxaOcupacao, 2),
                Status = patio.VagasDisponiveis > 0 ? "Com vagas" : "Lotado",
                UltimaAtualizacao = DateTime.Now
            };

            return Ok(new
            {
                Data = estatisticas,
                Message = "Estatísticas recuperadas com sucesso",
                Timestamp = DateTime.Now,
                Version = "2.0"
            });
        }

        /// <summary>
        /// Listar pátios com vagas disponíveis
        /// </summary>
        /// <remarks>
        /// Retorna apenas os pátios que possuem vagas disponíveis.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <returns>Lista de pátios com vagas</returns>
        [HttpGet("com-vagas")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar pátios com vagas",
            Description = "Retorna pátios com vagas disponíveis",
            OperationId = "GetPatiosComVagas"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Patio>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Patio>>> GetPatiosComVagas()
        {
            var response = await _patioService.GetPatiosComVagasDisponiveisAsync();
            return HandleServiceResponse(response);
        }

        /// <summary>
        /// Verificar vagas disponíveis
        /// </summary>
        /// <remarks>
        /// Verifica se um pátio específico possui vagas disponíveis.
        /// 
        /// **Roles:** Nenhuma (público)
        /// </remarks>
        /// <param name="nomePatio">Nome do pátio</param>
        /// <returns>Status das vagas</returns>
        [HttpGet("{nomePatio}/vagas")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Verificar vagas",
            Description = "Verifica vagas disponíveis no pátio",
            OperationId = "GetVagasDisponiveis"
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> GetVagasDisponiveis(
            [FromRoute, SwaggerParameter("Nome do pátio", Required = true)] string nomePatio)
        {
            var response = await _patioService.VerificarVagasDisponiveisAsync(nomePatio);
            
            if (!response.Success)
                return NotFound(CreateErrorResponse(response.Message));

            var patioResponse = await _patioService.GetPatioAsync(nomePatio);
            var patio = patioResponse.Data;

            return Ok(new
            {
                Data = new
                {
                    NomePatio = nomePatio,
                    VagasDisponiveis = patio?.VagasDisponiveis ?? 0,
                    TemVagas = response.Data,
                    Timestamp = DateTime.Now
                },
                Message = response.Data ? "Pátio possui vagas disponíveis" : "Pátio está cheio",
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }
    }
}